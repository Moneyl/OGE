using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using OGE.Utility;
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
            _fileList.ThrownExceptions.Subscribe(error =>
                WindowLogger.Log($"Error occured in FileExplorerViewModel.FileList OAPH: \"{error.Message}\""));
        }

        private async Task<IEnumerable<FileExplorerItemViewModel>> GenerateFileListTask()
        {
            return EnumerateFileList();
        }

        private IEnumerable<FileExplorerItemViewModel> EnumerateFileList()
        {
            foreach (var filePath in Directory.GetFiles(_workingDirectory))
            {
                yield return new FileExplorerItemViewModel(filePath);
            }
        }
    }
}