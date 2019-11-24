using System.Reactive.Disposables;
using System.Windows.Controls;
using System.Windows.Input;
using OGE.Events;
using OGE.ViewModels;
using ReactiveUI;

namespace OGE.Views
{
    public partial class FileExplorerView : ReactiveUserControl<FileExplorerViewModel>
    {
        public FileExplorerView()
        {
            InitializeComponent();

            ViewModel = new FileExplorerViewModel(@"C:\Users\moneyl\RFG Unpack\data\");
            FileTree.ItemsSource = ViewModel.FileList;

            this.WhenActivated(disposable =>
            {
                this.Bind(ViewModel,
                        vm => vm.SelectedItem,
                        v => v.FileTree.SelectedItem)
                    .DisposeWith(disposable);
            });
        }

        public void OnItemMouseDoubleClick(object sender, MouseButtonEventArgs args)
        {
            var item = sender as TreeViewItem;
            if(item == null)
                return;

            MessageBus.Current.SendMessage(new OpenFileEventArgs(item.Header as FileExplorerItemViewModel));
        }
    }
}
