using System.Diagnostics;
using System.Reactive.Disposables;
using System.Windows.Controls;
using System.Windows.Input;
using OGE.Editor.Events;
using OGE.ViewModels.FileExplorer;
using ReactiveUI;

namespace OGE.Views.FileExplorer
{
    public partial class FileExplorerView : ReactiveUserControl<FileExplorerViewModel>
    {
        private Stopwatch _searchChangedTimer = new Stopwatch();
        private long _minSearchUpdateTimer = 500; //Time in ms since the last file explorer list update

        public FileExplorerView()
        {
            InitializeComponent();

#if DEBUG
            ViewModel = new FileExplorerViewModel(@"C:\Users\moneyl\RFG Unpack\data"); //Set debug dir for convenience
#else
            ViewModel = new FileExplorerViewModel(@"C:\");
#endif
            FileTree.ItemsSource = ViewModel.FileList;

            this.WhenActivated(disposable =>
            {
                this.Bind(ViewModel,
                        vm => vm.SelectedItem,
                        v => v.FileTree.SelectedItem)
                    .DisposeWith(disposable);

                this.Bind(ViewModel,
                        vm => vm.SearchTerm,
                        v => v.SearchBox.Text)
                    .DisposeWith(disposable);
            });
        }

        public void OnItemMouseDoubleClick(object sender, MouseButtonEventArgs args)
        {
            var item = sender as TreeViewItem;
            //Prevent event from being recursively triggered on parents
            if (item == null || !item.IsSelected) 
                return;

            MessageBus.Current.SendMessage(new OpenFileEventArgs(item.Header as FileExplorerItemViewModel));
        }
    }
}
