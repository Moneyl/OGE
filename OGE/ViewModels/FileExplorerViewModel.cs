using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using OGE.Views;
using ReactiveUI;

namespace OGE.ViewModels
{
    public class FileExplorerViewModel : ReactiveObject
    {
        private string _workingDirectory;

        private ObservableAsPropertyHelper<IEnumerable<FileExplorerItemViewModel>> _fileList;
        public IEnumerable<FileExplorerItemViewModel> FileList => _fileList.Value;

        public FileExplorerViewModel(string workingDirectory)
        {
            _workingDirectory = workingDirectory;

            _fileList = this.WhenAnyValue(x => x._workingDirectory)
                .Where(x => Directory.Exists(_workingDirectory))
                .SelectMany(x => GenerateFileListTask())
                .ToProperty(this, x => x.FileList);
        }

        private async Task<IEnumerable<FileExplorerItemViewModel>> GenerateFileListTask()
        {
            return EnumerateFileList();
        }

        private IEnumerable<FileExplorerItemViewModel> EnumerateFileList()
        {
            var fileList = Directory.GetFiles(_workingDirectory);
            foreach (var filePath in Directory.GetFiles(_workingDirectory))
            {
                yield return new FileExplorerItemViewModel(filePath);
            }
        }
    }
}