using System.IO;
using OGE.Editor.Managers;
using OGE.Helpers;
using ReactiveUI;
using RfgTools.Formats.Packfiles;

namespace OGE.ViewModels.FileExplorer
{
    public class FileExplorerItemViewModel : TreeItem
    {
        private string _filePath;

        public Packfile Packfile { get; private set; }
        public FileExplorerItemViewModel Parent { get; private set; }
        public string Filename { get; set; }
        public string FileExtension { get; set; }
        public bool IsTopLevelPackfile { get; set; } = false;
        public override object ViewModel => this;
        public string Key { get; private set; }
        public bool IsEmbeddedPackfile { get; private set; }
        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = this.RaiseAndSetIfChanged(ref _filePath, value);
                Filename = Path.GetFileName(_filePath);
                FileExtension = Path.GetExtension(_filePath);

                //Set Key
                if (IsTopLevelPackfile || Parent == null)
                    Key = Filename;
                else
                    Key = $"{Parent.Filename}--{Filename}";

                //Set IsEmbeddedPackfile
                IsEmbeddedPackfile = PathHelpers.IsPackfilePath(FilePath) && !IsTopLevelPackfile;
            }
        }

        public FileExplorerItemViewModel(string filePath, FileExplorerItemViewModel parent, Packfile packfile = null, bool isTopLevelPackfile = false)
        {
            Packfile = packfile;
            IsTopLevelPackfile = isTopLevelPackfile;
            Parent = parent;
            FilePath = filePath;
        }

        /// <summary>
        /// Try to get a sibling file (same parent) with the provided targetFilename.
        /// </summary>
        /// <param name="targetFilename">The file to find.</param>
        /// <param name="target">The target if it's found, or null.</param>
        /// <returns>True if target found, false if not.</returns>
        public bool TryGetSiblingItem(string targetFilename, out FileExplorerItemViewModel target)
        {
            target = null;
            if (Parent == null)
                return false;

            foreach (var sibling in Parent.Children)
            {
                var siblingCast = (FileExplorerItemViewModel)sibling;
                if (siblingCast.Filename == targetFilename)
                {
                    target = siblingCast;
                    return true;
                }
            }
            return false;
        }

        public void FillChildrenList(string searchTerm)
        {
            //Handle internal packfiles
            if (Packfile == null)
            {
                //Need to read data about self and subfiles from parent
                if(Parent == null)
                    return;
                //Ignore non packfiles
                if (!PathHelpers.IsPackfilePath(FilePath)) 
                    return;

                //Try to see if it's in the cache
                if (ProjectManager.IsFileCached(FilePath, Path.GetFileName(Parent.FilePath)))
                {
                    string packfilePath = $"{ProjectManager.GlobalCachePath}{Path.GetFileName(Parent.FilePath)}\\{FilePath}";
                    Packfile = new Packfile(false);
                    Packfile.ReadMetadata(packfilePath);

                    foreach (var subfile in Packfile.DirectoryEntries)
                    {
                        if(!subfile.FileName.Contains(searchTerm))
                            continue;

                        var explorerItem = new FileExplorerItemViewModel(subfile.FileName, this);
                        AddChild(explorerItem);
                    }
                }
                else //If not in cache, get subfiles list from asm_pc files in parent.
                {
                    if(Parent.Packfile == null)
                        return;

                    //Containers don't have extension in asm_pc files, so strip extension for comparisons
                    string filenameNoExtension = Path.GetFileNameWithoutExtension(FilePath);
                    foreach (var asmFile in Parent.Packfile.AsmFiles)
                    {
                        foreach (var container in asmFile.Containers)
                        {
                            if (container.Name != filenameNoExtension)
                                continue;

                            foreach (var primitive in container.Primitives)
                            {
                                if (!primitive.Name.Contains(searchTerm))
                                    continue;

                                var explorerItem = new FileExplorerItemViewModel(primitive.Name, this);
                                AddChild(explorerItem);
                            }
                        }
                    }
                }
            }
            else //Handle top level packfiles
            {
                foreach (var filename in Packfile.Filenames)
                {
                    //Don't show non packfiles that don't fit the search term
                    if (!PathHelpers.IsPackfilePath(filename))
                        if(!filename.Contains(searchTerm))
                            continue;

                    var explorerItem = new FileExplorerItemViewModel(filename, this);
                    explorerItem.FillChildrenList(searchTerm);
                    AddChild(explorerItem);
                }
            }
        }
    }
}
