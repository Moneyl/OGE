using System.Reactive.Disposables;
using System.Windows;
using OGE.Editor.Events;
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

                this.OneWayBind(ViewModel,
                        vm => vm.Icon,
                        v => v.ItemIcon.Icon)
                    .DisposeWith(disposable);

                this.OneWayBind(ViewModel,
                        vm => vm.ForegroundBrush,
                        v => v.ItemIcon.Foreground)
                    .DisposeWith(disposable);
            });
        }

        private void Expand_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.Expand();
        }

        private void Collapse_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.Collapse();
        }

        private void CollapseAll_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBus.Current.SendMessage(new FileExplorerCollapseAllEventArgs());
        }

        private void CollapsePath_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.CollapsePath();
        }
    }
}
