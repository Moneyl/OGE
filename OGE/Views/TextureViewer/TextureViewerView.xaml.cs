using System;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OGE.ViewModels.TextureViewer;
using ReactiveUI;
using RfgTools.Formats.Textures;
using CheckBox = System.Windows.Controls.CheckBox;

namespace OGE.Views.TextureViewer
{
    public partial class TextureViewerView : ReactiveUserControl<TextureViewerViewModel>
    {
        private ImageBrush _checkeredImageBrush;

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

        private void BackgroundCheckbox_OnClick(object sender, RoutedEventArgs e)
        {
            var isChecked = ((CheckBox)sender).IsChecked;
            if (isChecked != null && (bool)isChecked)
            {
                _checkeredImageBrush = (ImageBrush)ImageViewBorder.Background;
                var uri = new Uri("pack://application:,,,/OGE;component/Resources/Solid.png");
                ImageViewBorder.Background = new ImageBrush(new BitmapImage(uri))
                {
                    TileMode = TileMode.Tile,
                    ViewboxUnits = BrushMappingMode.Absolute,
                    Viewport = new Rect(0, 0, 20, 20)
                };
            }
            else
            {
                ImageViewBorder.Background = _checkeredImageBrush;
            }
        }

        private void ContextMenuExtractSingleTexture_OnClick(object sender, RoutedEventArgs e)
        {
            if(TextureList.SelectedIndex == -1)
                return;

            ViewModel.ExtractSingleTexture(TextureList.SelectedIndex);
        }

        private void ExtractAllTexturesButton_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.ExtractAllTextures();
        }
    }
}
