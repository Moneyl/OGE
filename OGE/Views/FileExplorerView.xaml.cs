using System.Reactive.Disposables;
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
    }
}
