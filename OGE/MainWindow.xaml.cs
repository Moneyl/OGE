using System.Reactive.Disposables;
using System.Windows;
using OGE.Utility;
using OGE.ViewModels;
using ReactiveUI;

namespace OGE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ReactiveWindow<AppViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            //WindowLogger.SetLogPanel(LogBox);

            ViewModel = new AppViewModel();

            //this.WhenActivated(disposable =>
            //{
            //    this.OneWayBind(ViewModel,
            //            vm => vm.FileExplorerVm,
            //            v => v.fileExplorerView.ViewModel)
            //        .DisposeWith(disposable);
            //});
        }

        private void MenuExit_OnClick(object sender, RoutedEventArgs e)
        {
#if DEBUG
            var debug = 2; //Set breakpoint here for easy debugging
#else
            Close();
#endif
        }
    }
}
