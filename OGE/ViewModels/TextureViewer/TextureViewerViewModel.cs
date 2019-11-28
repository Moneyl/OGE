using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using OGE.Helpers;
using ReactiveUI;
using RfgTools.Formats.Textures;

namespace OGE.ViewModels.TextureViewer
{
    //Todo: Add checkbox above entries list to change background between checkerboard and dark
    //Todo: Update properties panel with valid info about sub-textures like original editor had
    //Todo: Test that this UI properly updates when files are imported/exported
    //Todo: Consider moving sub-texture list into an anchorable panel
    public class TextureViewerViewModel : ReactiveObject
    {
        private PegFile _peg;
        private TextureEntryViewModel _selectedItem;

        public string CpuFilePath { get; private set; }
        public string GpuFilePath { get; private set; }
        public string CpuFileName { get; private set; }
        public string GpuFileName { get; private set; }
        public PegFile Peg
        {
            get => _peg;
            private set => this.RaiseAndSetIfChanged(ref _peg, value);
        }
        public TextureEntryViewModel SelectedItem
        {
            get => _selectedItem;
            set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
        }
        
        private ObservableAsPropertyHelper<IEnumerable<TextureEntryViewModel>> _textureEntries;
        public IEnumerable<TextureEntryViewModel> TextureEntries => _textureEntries.Value;

        private ObservableAsPropertyHelper<BitmapImage> _currentTexture;
        public BitmapImage CurrentTexture => _currentTexture.Value;

        public TextureViewerViewModel(PegFile peg)
        {
            _peg = peg;

            _textureEntries = this.WhenAnyValue(x => x.Peg)
                .SelectMany(x => GenerateTextureEntriesList())
                .ToProperty(this, x => x.TextureEntries);

            _currentTexture = this.WhenAnyValue(x => x.SelectedItem)
                .Where(x => x != null && x.Index >= 0 && x.Index < Peg.Entries.Count)
                .Select(x => GetSelectedTextureBitmap())
                .ToProperty(this, x => x.CurrentTexture);
        }

        private async Task<IEnumerable<TextureEntryViewModel>> GenerateTextureEntriesList()
        {
            return EnumerateTextureEntries();
        }

        private IEnumerable<TextureEntryViewModel> EnumerateTextureEntries()
        {
            foreach(var entry in Peg.Entries)
            {
                yield return new TextureEntryViewModel(entry.Name, Peg.Entries.IndexOf(entry));
            }
        }

        private BitmapImage GetSelectedTextureBitmap()
        {
            return ImageHelpers.BitmapToBitmapImage(Peg.Entries[SelectedItem.Index].Bitmap);
        }
    }
}
