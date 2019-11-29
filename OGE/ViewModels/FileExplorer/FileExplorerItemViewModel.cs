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
        private string _shortName;
        private string _fileExtension;

        private bool _isSelected;
        private bool _isExpanded;

        private Packfile _packfile;
        public Packfile Packfile => _packfile;

        public FileExplorerItemViewModel Parent { get; private set; }

        //Todo: Make sure the path is actually passed to this, or just rename to Filename and have separate path variable. Current name is misleading
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

        public void FillChildrenList(string searchTerm)
        {
            //Handle internal packfiles
            if (_packfile == null && Parent != null)
            {
                //Ignore non packfiles
                if (!PathHelpers.IsPackfilePath(FilePath)) 
                    return;

                //Try to see if it's in the cache
                if (ProjectManager.IsFileCached(FilePath, Path.GetFileName(Parent.FilePath)))
                {
                    string packfilePath = $"{ProjectManager.GlobalCachePath}{Path.GetFileName(Parent.FilePath)}\\{FilePath}";
                    _packfile = new Packfile(false);
                    _packfile.ReadMetadata(packfilePath);

                    for (var i = 0; i < _packfile.DirectoryEntries.Count; i++)
                    {
                        var subfile = _packfile.DirectoryEntries[i];
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
                    for (var i = 0; i < Parent.Packfile.AsmFiles.Count; i++)
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
            else if(_packfile != null)//Handle top level packfiles
            {
                for (var i = 0; i < _packfile.Filenames.Count; i++)
                {
                    var filename = _packfile.Filenames[i];

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
