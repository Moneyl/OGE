using System.Collections;
using System.IO;
using OGE.Editor;
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

                    for (var i = 0; i < Packfile.DirectoryEntries.Count; i++)
                    {
                        var subfile = Packfile.DirectoryEntries[i];
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
                    for (var i = 0; i < Parent.Packfile.AsmFiles.Count; i++) //Todo: Benchmark speed dif here between for and foreach again. Likely negligable
                    {
                        var asmFile = Parent.Packfile.AsmFiles[i];
                        for (var j = 0; j < asmFile.Containers.Count; j++)
                        {
                            var container = asmFile.Containers[j];
                            if (container.Name != filenameNoExtension)
                                continue;

                            for (var k = 0; k < container.Primitives.Count; k++)
                            {
                                var primitive = container.Primitives[k];
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
                for (var i = 0; i < Packfile.Filenames.Count; i++)
                {
                    var filename = Packfile.Filenames[i];

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
