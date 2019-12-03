using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using OGE.Helpers;
using ReactiveUI;
using RfgTools.Formats.Textures;

namespace OGE.ViewModels.TextureViewer
{
    public class TextureViewerViewModel : ReactiveObject
    {
        private PegFile _peg;
        private TextureEntryViewModel _selectedItem;
        private FolderBrowserDialog _openFolderDialog = new FolderBrowserDialog();

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

        public void ExtractSingleTexture(int targetIndex)
        {
            if(Peg == null || Peg.Entries.Count <= 0)
                return;

            if (_openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                var selectedEntry = _peg.Entries[targetIndex];
                string outputPath = $"{_openFolderDialog.SelectedPath}\\{Path.GetFileNameWithoutExtension(selectedEntry.Name)}.png";
                selectedEntry.Bitmap.Save(outputPath, ImageFormat.Png);
            }
        }

        public void ExtractAllTextures()
        {
            if (Peg == null || Peg.Entries.Count <= 0)
                return;

            if (_openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (var entry in Peg.Entries)
                {
                    string outputPath = $"{_openFolderDialog.SelectedPath}\\{Path.GetFileNameWithoutExtension(entry.Name)}.png";
                    entry.Bitmap.Save(outputPath, ImageFormat.Png);
                }
            }
        }
    }
}
