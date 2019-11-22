using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OGE.ViewModels;
using ReactiveUI;

namespace OGE.Views
{
    /// <summary>
    /// Interaction logic for FileExplorerItemView.xaml
    /// </summary>
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
                //    this.Bind(ViewModel,
                //            vm => vm.ShortName,
                //            v => v.TreeItem.Header)
                //        .DisposeWith(disposable);

                //    this.Bind(ViewModel,
                //            vm => vm.IsSelected,
                //            v => v.TreeItem.IsSelected)
                //        .DisposeWith(disposable);

                //    this.Bind(ViewModel,
                //            vm => vm.IsExpanded,
                //            v => v.TreeItem.IsExpanded)
                //        .DisposeWith(disposable);

                //    this.OneWayBind(ViewModel,
                //            vm => vm.SubFileList,
                //            v => v.TreeItem.ItemsSource)
                //        .DisposeWith(disposable);
            });
        }
    }
}
