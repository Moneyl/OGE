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
        private bool _isVirtualFile;
        private string _shortName;
        private string _fileExtension;

        private bool _isSelected;
        private bool _isExpanded;

        //Todo: Probably should have a static class that owns all file data and manages parsing / saving / loading them
        private Packfile _packfile;
        public Packfile Packfile => _packfile;

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

        public bool IsVirtualFile
        {
            get => _isVirtualFile;
            set => _isVirtualFile = value;
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

        public FileExplorerItemViewModel(string filePath, bool isVirtualFile = false)
        {
            FilePath = filePath;
            IsVirtualFile = isVirtualFile;
        }

        public void FillChildrenList()
        {
            if (_packfile == null)
            {
                _packfile = new Packfile(false);
                _packfile.ReadMetadata(FilePath);
            }
            if(_packfile.Filenames == null)
                return;

            foreach (var filename in _packfile.Filenames)
            {
                 AddChild(new FileExplorerItemViewModel(filename, true));
            }
        }

        private bool Predicate(string x)
        {
            bool val = IsPackfile() && !IsVirtualFile && File.Exists(FilePath);
            return val;
        }

        private bool IsPackfile()
        {
            return FileExtension == ".vpp_pc" || FileExtension == ".str_pc";
        }

        private async Task<IEnumerable<TreeItem>> GenerateSubFileListTask()
        {
            return EnumerateSubFileList();
        }

        private IEnumerable<TreeItem> EnumerateSubFileList()
        {
            if (_packfile == null)
            {
                _packfile = new Packfile(false);
                _packfile.ReadMetadata(FilePath);
            }

            foreach (var filename in _packfile.Filenames)
            {
                yield return new FileExplorerItemViewModel(filename, true);
            }
        }
    }
}
