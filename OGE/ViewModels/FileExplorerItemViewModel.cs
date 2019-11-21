using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using RfgTools.Formats.Packfiles;

namespace OGE.ViewModels
{
    public class FileExplorerItemViewModel : ReactiveObject
    {
        private string _filePath;
        private bool _isVirtualFile;
        private string _shortName;
        private string _fileExtension;
        private Packfile _packfile;

        private ObservableAsPropertyHelper<IEnumerable<FileExplorerItemViewModel>> _subFileList;
        public IEnumerable<FileExplorerItemViewModel> SubFileList => _subFileList.Value;

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

        public FileExplorerItemViewModel(string filePath, bool isVirtualFile = false)
        {
            FilePath = filePath;
            //ShortName = Path.GetFileName(_filePath);
            IsVirtualFile = isVirtualFile;

            var subFileListObservable = this.WhenAnyValue(x => x.FilePath)
                .Where(x => IsPackfile() && File.Exists(FilePath))
                .SelectMany(x => GenerateSubFileListTask());
            _subFileList = subFileListObservable.ToProperty(this, nameof(SubFileList), deferSubscription: true);
        }

        private bool IsPackfile()
        {
            return FileExtension == ".vpp_pc" || FileExtension == ".str_pc";
        }

        private async Task<IEnumerable<FileExplorerItemViewModel>> GenerateSubFileListTask()
        {
            return EnumerateSubFileList();
        }

        private IEnumerable<FileExplorerItemViewModel> EnumerateSubFileList()
        {
            if (_packfile == null)
            {
                _packfile = new Packfile(false);
                _packfile.ReadMetadata(FilePath);
            }

            foreach (var filename in _packfile.Filenames)
            {
                if(IsVirtualFile)
                    continue;
                
                yield return new FileExplorerItemViewModel(filename, true);
            }
        }
    }
}
