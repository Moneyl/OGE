using System.IO;
using OGE.Editor;
using OGE.Editor.Managers;
using OGE.Utility.Helpers;
using ReactiveUI;

namespace OGE.ViewModels.FileExplorer
{
    public class FileExplorerItemViewModel : TreeItem
    {
        private string _filename;

        public CacheFile File { get; private set; }
        public FileExplorerItemViewModel Parent { get; private set; }
        public uint Depth { get; set; } = 0;
        public string FileExtension { get; set; }
        public override object ViewModel => this;
        public string Key { get; private set; }
        public bool IsEmbeddedPackfile { get; private set; }
        public string Filename
        {
            get => _filename;
            set
            {
                _filename = this.RaiseAndSetIfChanged(ref _filename, value);
                FileExtension = Path.GetExtension(_filename);

                //Set Key
                if (Depth == 0 || Parent == null)
                    Key = Filename;
                else
                    Key = $"{Parent.Filename}--{Filename}";

                //Set IsEmbeddedPackfile
                IsEmbeddedPackfile = PathHelpers.IsPackfilePath(Filename) && Depth > 0;
            }
        }

        public FileExplorerItemViewModel(string filename, FileExplorerItemViewModel parent, uint depth, CacheFile cacheFile = null)
        {
            File = cacheFile;
            Parent = parent;
            Depth = depth;
            Filename = filename;
        }

        public bool GetCacheFile(bool extractIfNotFound = false)
        {
            ProjectManager.TryGetCacheFile(Filename, Parent?.Filename, out CacheFile file, extractIfNotFound);
            File = file;
            return File != null;
        }

        public void CollapseAll()
        {
            foreach (var treeItem in Children)
            {
                var child = (FileExplorerItemViewModel)treeItem;
                child.CollapseAll();
            }
            Collapse();
        }

        public void FillChildrenList(string searchTerm)
        {
            if (Depth == 0) //Handle depth 0 packfiles
            {
                if(File?.PackfileData == null)
                    return;

                foreach (var subFile in File.PackfileData.DirectoryEntries)
                {
                    if(!subFile.FileName.Contains(searchTerm))
                        continue;

                    var explorerItem = new FileExplorerItemViewModel(subFile.FileName, this, Depth + 1);
                    explorerItem.FillChildrenList(searchTerm);
                    AddChild(explorerItem);
                }
            }
            else //Handle depth > 0 packfiles
            {
                if (!IsEmbeddedPackfile || Parent == null)
                    return;

                //Check parent for asm file, get children files. Cheaper than extracting/finding and parsing
                string filenameNoExtension = Path.GetFileNameWithoutExtension(Filename);
                foreach (var asmFile in Parent.File.PackfileData.AsmFiles)
                {
                    foreach (var container in asmFile.Containers)
                    {
                        if(container.Name != filenameNoExtension)
                            continue;

                        foreach (var primitive in container.Primitives)
                        {
                            if(!primitive.Name.Contains(searchTerm))
                                continue;

                            //Todo: Figure out a better way of handling this. Likely other files with this problem
                            //Check for asm_pc/file name mismatch. Currently only occurs with one file
                            //The asm has the extension cvbm_pc, when it should be cpeg_pc. Breaking cache operations on it
                            if (primitive.Name == "interface-badges.cvbm_pc")
                            {
                                var explorerItem = new FileExplorerItemViewModel("interface-badges.cpeg_pc", this, Depth + 1);
                                AddChild(explorerItem);
                            }
                            else
                            {
                                var explorerItem = new FileExplorerItemViewModel(primitive.Name, this, Depth + 1);
                                AddChild(explorerItem);
                            }
                        }
                    }
                }
            }
        }
    }
}
