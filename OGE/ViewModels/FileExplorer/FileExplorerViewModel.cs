using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Linq;
using OGE.Editor;
using OGE.Events;
using ReactiveUI;

namespace OGE.ViewModels.FileExplorer
{
    public class FileExplorerViewModel : ReactiveObject
    {
        private string _workingDirectory;
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

        public FileExplorerViewModel(string workingDirectory)
        {
            WorkingDirectory = workingDirectory;
            FillFilesList();

            MessageBus.Current.Listen<ChangeWorkingDirectoryEventArgs>()
                .Where(args => !string.IsNullOrWhiteSpace(args.NewWorkingDirectory)
                               && Directory.Exists(args.NewWorkingDirectory))
                .Subscribe(action =>
                {
                    WorkingDirectory = action.NewWorkingDirectory;
                    FillFilesList();
                });
        }

        public void FillFilesList()
        {
            FileList.Clear();
            foreach (var packfile in ProjectManager.WorkingDirectoryPackfiles)
            {
                var explorerItem = new FileExplorerItemViewModel(packfile.PackfilePath, null, packfile)
                {
                    IsTopLevelPackfile = true
                };

                explorerItem.FillChildrenList();
                FileList.Add(explorerItem);
            }
        }

        private void TriggerSelectedItemChangedEvent()
        {
            MessageBus.Current.SendMessage(new SelectedItemChangedEventArgs(_selectedItem));
        }
    }
}