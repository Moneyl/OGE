using System.Reactive.Disposables;
using System.Windows.Controls;
using System.Windows.Forms;
using OGE.Utility;
using OGE.ViewModels.TextureViewer;
using ReactiveUI;
using RfgTools.Formats.Textures;

namespace OGE.Views.TextureViewer
{
    public partial class TextureViewerView : ReactiveUserControl<TextureViewerViewModel>
    {
        public TextureViewerView()
        {

        }

        public TextureViewerView(PegFile peg)
        {
            InitializeComponent();
            ViewModel = new TextureViewerViewModel(peg);
            
            this.WhenActivated(disposable =>
            {
                this.OneWayBind(ViewModel,
                        vm => vm.TextureEntries,
                        v => v.TextureList.ItemsSource)
                    .DisposeWith(disposable);

                this.OneWayBind(ViewModel,
                        vm => vm.CurrentTexture,
                        v => v.TextureView.Source)
                    .DisposeWith(disposable);

                this.Bind(ViewModel,
                        vm => vm.SelectedItem,
                        v => v.TextureList.SelectedItem)
                    .DisposeWith(disposable);

                //Set first entry as selected item
                ViewModel.SelectedItem = TextureList.Items[0] as TextureEntryViewModel;
            });
        }
    }
}
