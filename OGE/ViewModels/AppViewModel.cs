using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Forms;
using OGE.Utility;
using ReactiveUI;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace OGE.ViewModels
{
    public class AppViewModel : ReactiveObject
    {
        //Filters for file open/close dialogs
        private static readonly string _zoneFileFilter = "Rfg zone files (*.rfgzone_pc, *.layer_pc)|*.rfgzone_pc;*.layer_pc";
        private static readonly string _xmlFileFilter = "Xml zone files (*.xml)|*.xml";

        private FolderBrowserDialog _openFolderDialog = new FolderBrowserDialog();
        private OpenFileDialog _openFileDialog = new OpenFileDialog();

        private string _workingDirectory;
        public string WorkingDirectory
        {
            get => _workingDirectory;
            set => this.RaiseAndSetIfChanged(ref _workingDirectory, value);
        }

        private ObservableAsPropertyHelper<FileExplorerViewModel> _fileExplorerVm;
        public FileExplorerViewModel FileExplorerVm => _fileExplorerVm.Value;

        public ReactiveCommand<Unit, Unit> OpenWorkingFolder { get; }
        //public ReactiveCommand<Unit, Unit> OpenFile { get; }
        //public ReactiveCommand<Unit, Unit> SaveFile { get; }
        //public ReactiveCommand<Unit, Unit> SaveAsFile { get; }

        public AppViewModel()
        {
            _fileExplorerVm = this.WhenAnyValue(x => x.WorkingDirectory)
                .Where(workingDir => !string.IsNullOrWhiteSpace(workingDir))
                .Select(workingDir => new FileExplorerViewModel(workingDir))
                .ToProperty(this, x => x.FileExplorerVm);

            OpenWorkingFolder = ReactiveCommand.Create(() =>
            {
                _openFolderDialog.ShowDialog();
                string folderPath = _openFolderDialog.SelectedPath;
                if (!Directory.Exists(folderPath))
                {
                    WindowLogger.Log($"Folder does not exist at \"{folderPath}\"");
                    return;
                }

                WindowLogger.Log($"Setting working directory to \"{folderPath}\"");
                WorkingDirectory = folderPath;
            });
        }
    }
}
