using System.Reactive.Disposables;
using OGE.ViewModels.FileExplorer;
using ReactiveUI;

namespace OGE.Views.FileExplorer
{
    public partial class FileExplorerItemView : ReactiveUserControl<FileExplorerItemViewModel>
    {
        public FileExplorerItemView()
        {
            InitializeComponent();

            this.WhenActivated(disposable =>
            {
                this.OneWayBind(ViewModel,
                        vm => vm.Filename,
                        v => v.ItemName.Text)
                    .DisposeWith(disposable);
            });
        }
    }
}
