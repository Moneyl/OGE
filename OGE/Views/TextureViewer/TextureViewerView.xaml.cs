using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OGE.Editor.Events;
using OGE.Editor.Managers;
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
            _checkeredImageBrush = (ImageBrush)ImageViewBorder.Background; //Save checkered background for toggling between dark background

            //Set initial checkbox value to global setting and update.
            BackgroundCheckbox.IsChecked = SettingsManager.TextureViewerDarkBackground;
            UpdateBackgroundPattern();

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

            MessageBus.Current.Listen<TextureViewerGlobalSettingChangedEventArgs>()
                .Subscribe(HandleGlobalSettingsChange);
        }

        /// <summary>
        /// Updates checkbox and background when the global background setting for
        /// texture viewer documents changes. This value is the same for all
        /// texture viewer documents.
        /// </summary>
        /// <param name="args"></param>
        private void HandleGlobalSettingsChange(TextureViewerGlobalSettingChangedEventArgs args)
        {
            //Update checkbox value and background
            BackgroundCheckbox.IsChecked = SettingsManager.TextureViewerDarkBackground;
            UpdateBackgroundPattern();
        }

        private void BackgroundCheckbox_OnClick(object sender, RoutedEventArgs e)
        {
            //Get checked value from checkbox
            var isChecked = ((CheckBox)sender).IsChecked;
            if(isChecked == null)
                return;
            SettingsManager.TextureViewerDarkBackground = (bool)isChecked;

            //Send event to tell all other texture viewers to update their backgrounds
            MessageBus.Current.SendMessage(new TextureViewerGlobalSettingChangedEventArgs());
        }

        private void UpdateBackgroundPattern()
        {
            //Uses global value in SettingsManager to determine background type across texture viewer documents
            if (SettingsManager.TextureViewerDarkBackground)
            {
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
