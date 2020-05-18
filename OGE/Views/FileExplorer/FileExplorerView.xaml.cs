using System.Diagnostics;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OGE.Editor.Events;
using OGE.ViewModels.FileExplorer;
using ReactiveUI;

namespace OGE.Views.FileExplorer
{
    public partial class FileExplorerView : ReactiveUserControl<FileExplorerViewModel>
    {
        public FileExplorerView()
        {
            InitializeComponent();

            ViewModel = new FileExplorerViewModel(@"");
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
