using System.Windows;
using System.Windows.Controls;
using OGE.Utility;
using OGE.ViewModels;
using ReactiveUI;
using RfgTools.Formats.Textures;

namespace OGE.Views
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

            });
        }

        private void TextureTree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            WindowLogger.Log("[Debug] Selected texture changed!");
            SetSelectedTexture(GetSelectedTextureIndex());
        }

        private void SetSelectedTexture(int index)
        {
            //if (_peg != null)
            //{
            //    if (index < _peg.Entries.Count && index > -1)
            //    {
            //        TextureView.Source = Util.BitmapToBitmapImage(_peg.Entries[index].Bitmap);
            //        ((TreeViewItem)TextureTree.Items[index]).IsSelected = true;
            //    }
            //}
        }

        int GetSelectedTextureIndex()
        {
            return TextureTree.Items.IndexOf(TextureTree.SelectedItem);
        }
    }
}
