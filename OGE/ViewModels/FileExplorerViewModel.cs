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

        private ObservableAsPropertyHelper<IEnumerable<TreeItem>> _fileList;
        public IEnumerable<TreeItem> FileList => _fileList.Value;

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

        private async Task<IEnumerable<TreeItem>> GenerateFileListTask()
        {
            return EnumerateFileList();
        }

        private bool IsPackfileExtension(string extension)
        {
            return extension == ".vpp_pc" || extension == ".str2_pc";
        }

        private IEnumerable<TreeItem> EnumerateFileList()
        {
            foreach (var filePath in Directory.GetFiles(_workingDirectory))
            {
                if(!IsPackfileExtension(Path.GetExtension(filePath)))
                    continue;

                var newVal = new FileExplorerItemViewModel(filePath);
                newVal.FillChildrenList();
                if (newVal.Children == null)
                {
                    var a = 2;
                }
                else
                {
                    var b = 2;
                }
                yield return newVal;
            }
        }
    }
}