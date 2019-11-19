using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using ReactiveUI;

namespace OGE.ViewModels
{
    public class AppViewModel : ReactiveObject
    {
        private string _workingDirectory;
        public string WorkingDirectory
        {
            get => _workingDirectory;
            set => this.RaiseAndSetIfChanged(ref _workingDirectory, value);
        }

        private ObservableAsPropertyHelper<FileExplorerViewModel> _fileExplorerVm;
        public FileExplorerViewModel FileExplorerVm => _fileExplorerVm.Value;

        public AppViewModel()
        {
            _fileExplorerVm = this.WhenAnyValue(x => x.WorkingDirectory)
                .Where(workingDir => !string.IsNullOrWhiteSpace(workingDir))
                .Select(workingDir => new FileExplorerViewModel(workingDir))
                .ToProperty(this, x => x.FileExplorerVm);
        }
    }
}
