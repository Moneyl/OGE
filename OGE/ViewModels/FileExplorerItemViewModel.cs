using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ReactiveUI;
using RfgTools.Formats.Packfiles;

namespace OGE.ViewModels
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

        public override object ViewModel => this;

        public FileExplorerItemViewModel(string filePath, FileExplorerItemViewModel parent, Packfile packfile = null)
        {
            _packfile = packfile;
            FilePath = filePath;
            Parent = parent;
        }

        public void FillChildrenList()
        {
            if(_packfile?.Filenames == null)
                return;

            foreach (var filename in _packfile.Filenames)
            {
                 AddChild(new FileExplorerItemViewModel(filename, this));
            }
        }
    }
}
