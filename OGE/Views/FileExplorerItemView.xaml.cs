using System.Reactive.Disposables;
using System.Windows.Input;
using OGE.Events;
using OGE.ViewModels;
using ReactiveUI;

namespace OGE.Views
{
    public partial class FileExplorerItemView : ReactiveUserControl<FileExplorerItemViewModel>
    {
        public FileExplorerItemView()
        {
            InitializeComponent();

            this.WhenActivated(disposable =>
            {
                this.OneWayBind(ViewModel,
                        vm => vm.ShortName,
                        v => v.ItemName.Text)
                    .DisposeWith(disposable);
            });
        }
    }
}
