using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using OGE.Events;
using ReactiveUI;

namespace OGE.ViewModels
{
    public class FileExplorerViewModel : ReactiveObject
    {
        private string _workingDirectory;
        public ObservableCollection<TreeItem> FileList = new ObservableCollection<TreeItem>();

        public ReactiveCommand<object, Unit> SelectedItemChangedCommand;

        public FileExplorerItemViewModel _selectedItem = null;
        public FileExplorerItemViewModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedItem, value);
                TriggerSelectedItemChangedEvent();
            }
        }

        public FileExplorerViewModel(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
            FillFilesList();

            MessageBus.Current.Listen<ChangeWorkingDirectoryEventArgs>()
                .Where(args => !string.IsNullOrWhiteSpace(args.NewWorkingDirectory)
                               && Directory.Exists(args.NewWorkingDirectory))
                .Subscribe(action =>
                {
                    _workingDirectory = action.NewWorkingDirectory;
                    FillFilesList();
                });
        }

        private bool IsPackfileExtension(string extension)
        {
            return extension == ".vpp_pc" || extension == ".str2_pc";
        }

        public void FillFilesList()
        {
            var directoryFiles = Directory.GetFiles(_workingDirectory);
            FileList.Clear();
            foreach(var filePath in Directory.GetFiles(_workingDirectory))
            {
                if(!IsPackfileExtension(Path.GetExtension(filePath)))
                    continue;

                var newVal = new FileExplorerItemViewModel(filePath);
                newVal.FillChildrenList();
                FileList.Add(newVal);
            }
        }

        private void TriggerSelectedItemChangedEvent()
        {
            MessageBus.Current.SendMessage(new SelectedItemChangedEventArgs(_selectedItem));
        }
    }
}