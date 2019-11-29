using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Data;
using System.Windows.Forms;
using OGE.Editor;
using OGE.Events;
using ReactiveUI;
using Application = System.Windows.Application;

namespace OGE.ViewModels.FileExplorer
{
    public class FileExplorerViewModel : ReactiveObject
    {
        private string _searchTerm = "";
        private string _workingDirectory;

        private object _fileListLock = new object();
        public ObservableCollection<TreeItem> FileList = new ObservableCollection<TreeItem>();

        private FileExplorerItemViewModel _selectedItem = null;
        public FileExplorerItemViewModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedItem, value);
                TriggerSelectedItemChangedEvent();
            }
        }
        
        public string WorkingDirectory
        {
            get => _workingDirectory;
            set
            {
                ProjectManager.WorkingDirectory = value;
                _workingDirectory = value;
            }
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set => this.RaiseAndSetIfChanged(ref _searchTerm, value);
        }

        public ReactiveCommand<string, Unit> ReloadFilesListCommand;

        public FileExplorerViewModel(string workingDirectory)
        {
            //Enable synchronization on file list to avoid issues with multiple threads accessing it
            BindingOperations.EnableCollectionSynchronization(FileList, _fileListLock);

            WorkingDirectory = workingDirectory;
            //ReloadFilesList();

            ReloadFilesListCommand = ReactiveCommand.Create((string searchTerm) =>
            {
                ReloadFilesList();
            });

            this.WhenAnyValue(x => x.SearchTerm)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .Where(x => x != null)
                .InvokeCommand(this, x => x.ReloadFilesListCommand);

            MessageBus.Current.Listen<ChangeWorkingDirectoryEventArgs>()
                .Where(args => !string.IsNullOrWhiteSpace(args.NewWorkingDirectory)
                               && Directory.Exists(args.NewWorkingDirectory))
                .Subscribe(action =>
                {
                    WorkingDirectory = action.NewWorkingDirectory;
                    ReloadFilesList();
                });
        }

        public void ReloadFilesList()
        {
            FileList.Clear();
            
            //Assuming that any packfile in this list is already confirmed to be a packfile by ProjectManager
            for (var i = 0; i < ProjectManager.WorkingDirectoryPackfiles.Count; i++)
            {
                var packfile = ProjectManager.WorkingDirectoryPackfiles[i];
                var explorerItem = new FileExplorerItemViewModel(packfile.PackfilePath, null, packfile)
                {
                    IsTopLevelPackfile = true
                };

                explorerItem.FillChildrenList(SearchTerm);
                FileList.Add(explorerItem);
            }
            
        }

        private void TriggerSelectedItemChangedEvent()
        {
            MessageBus.Current.SendMessage(new SelectedItemChangedEventArgs(_selectedItem));
        }
    }
}