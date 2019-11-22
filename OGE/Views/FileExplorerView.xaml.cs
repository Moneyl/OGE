using System.ComponentModel;
using System.Reactive.Disposables;
using System.Windows.Controls;
using OGE.ViewModels;
using ReactiveUI;
using Xceed.Wpf.AvalonDock.Layout;

namespace OGE.Views
{
    /// <summary>
    /// Interaction logic for FileExplorerView.xaml
    /// </summary>
    public partial class FileExplorerView : ReactiveUserControl<FileExplorerViewModel>
    {
        public FileExplorerView()
        {
            InitializeComponent();

            this.WhenActivated(disposable =>
            {
                //FileTree.ItemsSource = ViewModel.FileList;
                this.OneWayBind(ViewModel,
                        vm => vm.FileList,
                        v => v.FileTree.ItemsSource)
                    .DisposeWith(disposable);
            });
        }
    }
}
