using System.IO;
using System.Reactive;
using System.Windows.Forms;
using OGE.Editor.Events;
using OGE.Utility;
using ReactiveUI;

namespace OGE.ViewModels
{
    public class AppViewModel : ReactiveObject
    {
        private FolderBrowserDialog _openFolderDialog = new FolderBrowserDialog();
        private string _workingDirectory;

        public string WorkingDirectory
        {
            get => _workingDirectory;
            set => this.RaiseAndSetIfChanged(ref _workingDirectory, value);
        }
        public ReactiveCommand<Unit, Unit> OpenWorkingFolder { get; }

        public AppViewModel()
        {
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

                MessageBus.Current.SendMessage(new ChangeWorkingDirectoryEventArgs(WorkingDirectory));
            });
        }
    }
}
