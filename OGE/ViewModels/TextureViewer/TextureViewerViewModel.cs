using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using OGE.Editor;
using OGE.Editor.Actions;
using OGE.Utility.Helpers;
using ReactiveUI;
using RfgTools.Formats.Textures;

namespace OGE.ViewModels.TextureViewer
{
    public class TextureViewerViewModel : ReactiveObject
    {
        private CacheFile _file;
        private TextureEntryViewModel _selectedItem;
        private FolderBrowserDialog _openFolderDialog = new FolderBrowserDialog();
        private OpenFileDialog _openFileDialog = new OpenFileDialog();
        private bool _textureViewNeedsUpdate = false;

        public PegFile Peg
        {
            get => _file.PegData;
            private set => this.RaiseAndSetIfChanged(ref _file.PegData, value);
        }
        public TextureEntryViewModel SelectedItem
        {
            get => _selectedItem;
            set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
        }
        public bool TextureViewNeedsUpdate
        {
            get => _textureViewNeedsUpdate;
            set => this.RaiseAndSetIfChanged(ref _textureViewNeedsUpdate, value);
        }

        
        private ObservableAsPropertyHelper<IEnumerable<TextureEntryViewModel>> _textureEntries;
        public IEnumerable<TextureEntryViewModel> TextureEntries => _textureEntries.Value;

        private ObservableAsPropertyHelper<BitmapImage> _currentTexture;
        public BitmapImage CurrentTexture => _currentTexture.Value;

        public TextureViewerViewModel(CacheFile file)
        {
            _file = file;

            _textureEntries = this.WhenAnyValue(x => x.Peg)
                .SelectMany(x => GenerateTextureEntriesList())
                .ToProperty(this, x => x.TextureEntries);

            _currentTexture = this.WhenAnyValue(x => x.SelectedItem, x => x.TextureViewNeedsUpdate)
                .Where(x => x.Item1 != null && x.Item1.Index >= 0 && x.Item1.Index < Peg.Entries.Count)
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

        public void ForceTextureViewUpdate()
        {
            TextureViewNeedsUpdate = !TextureViewNeedsUpdate;
        }

        public void ExtractSingleTexture(int targetIndex)
        {
            if(Peg == null || Peg.Entries.Count <= 0)
                return;

            if (_openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                var selectedEntry = _file.PegData.Entries[targetIndex];
                string outputPath = $"{_openFolderDialog.SelectedPath}\\{Path.GetFileNameWithoutExtension(selectedEntry.Name)}.png";
                selectedEntry.Bitmap.Save(outputPath, ImageFormat.Png);
            }
        }

        public void ReplaceTexture(int targetIndex)
        {
            if(Peg == null || Peg.Entries.Count <= 0)
                return;

            //Todo: Move into TextureReplaceAction and track in project
            if(_openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string inputPath = _openFileDialog.FileName;
                var selectedEntry = _file.PegData.Entries[targetIndex];

                var replaceAction = new TextureReplaceAction();
                replaceAction.Init(_file, selectedEntry, inputPath);

                ForceTextureViewUpdate();
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
