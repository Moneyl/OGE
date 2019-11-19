using System.ComponentModel;
using System.Windows.Controls;
using OGE.ViewModels;
using ReactiveUI;
using Xceed.Wpf.AvalonDock.Layout;

namespace OGE.Views
{
    /// <summary>
    /// Interaction logic for FileExplorerView.xaml
    /// </summary>
    public partial class FileExplorerView : ReactiveUserControl<FileExplorerViewModel>, ILayoutPanelElement
    {
        public FileExplorerView()
        {
            InitializeComponent();

            this.WhenActivated(disposable =>
            {

            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;
        public ILayoutContainer Parent { get; }
        public ILayoutRoot Root { get; }
    }
}
