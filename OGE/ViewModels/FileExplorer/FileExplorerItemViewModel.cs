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
        private string _shortName;
        private string _fileExtension;

        private bool _isSelected;
        private bool _isExpanded;

        private Packfile _packfile;
        public Packfile Packfile => _packfile;

        public FileExplorerItemViewModel Parent { get; private set; }

        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = this.RaiseAndSetIfChanged(ref _filePath, value);
                ShortName = Path.GetFileName(_filePath);
                FileExtension = Path.GetExtension(_filePath);
            }
        }

        public string ShortName
        {
            get => _shortName;
            set => this.RaiseAndSetIfChanged(ref _shortName, value);
        }

        public string FileExtension
        {
            get => _fileExtension;
            set => _fileExtension = value;
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }

        public bool IsExpanded //Todo: Check if performance gain from waiting to load/parse children until this is set to true
        {
            get => _isExpanded;
            set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
        }

        public bool IsTopLevelPackfile { get; set; } = false;
        public override object ViewModel => this;

        public FileExplorerItemViewModel(string filePath, FileExplorerItemViewModel parent, Packfile packfile = null)
        {
            _packfile = packfile;
            FilePath = filePath;
            Parent = parent;
        }

        public void FillChildrenList()
        {
            //Try to handle packfiles that are inside other packfiles
            if (_packfile == null && Parent != null)
            {
                //Ignore non packfiles
                if (!PathHelpers.IsPackfilePath(FilePath)) 
                    return;

                //Try to see if it's in the cache
                if (ProjectManager.IsFileCached(FilePath, Parent.FilePath))
                {
                    //Todo: Add func to ProjectManager that gets path to cached file
                    string packfilePath = $"{ProjectManager.GlobalCachePath}{Packfile.Filename}\\{Parent.FilePath}";
                    _packfile = new Packfile(false);
                    _packfile.ReadMetadata(packfilePath);
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
                                var explorerItem = new FileExplorerItemViewModel(primitive.Name, this);
                                //explorerItem.FillChildrenList();
                                AddChild(explorerItem);
                            }
                        }
                    }
                }
            }
            else
            {
                if (_packfile?.Filenames == null)
                    return;

                foreach (var filename in _packfile.Filenames)
                {
                    var explorerItem = new FileExplorerItemViewModel(filename, this);
                    explorerItem.FillChildrenList();
                    AddChild(explorerItem);
                }
            }
        }
    }
}
