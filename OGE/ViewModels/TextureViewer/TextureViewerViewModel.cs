using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using RfgTools.Formats.Textures;

namespace OGE.ViewModels.TextureViewer
{
    //Todo: Update properties panel with valid info about sub-textures like original editor had
    //Todo: Test that this UI properly updates when files are imported/exported
    //Todo: Consider moving sub-texture list into an anchorable panel
    public class TextureViewerViewModel : ReactiveObject
    {
        private PegFile _peg;

        public string CpuFilePath { get; private set; }
        public string GpuFilePath { get; private set; }
        public string CpuFileName { get; private set; }
        public string GpuFileName { get; private set; }
        public PegFile Peg
        {
            get => _peg;
            private set => this.RaiseAndSetIfChanged(ref _peg, value);
        }

        private ObservableAsPropertyHelper<IEnumerable<TextureEntryViewModel>> _textureEntries;
        public IEnumerable<TextureEntryViewModel> TextureEntries => _textureEntries.Value;

        public TextureViewerViewModel(PegFile peg)
        {
            _peg = peg;

            _textureEntries = this.WhenAnyValue(x => x.Peg)
                .SelectMany(x => GenerateTextureEntriesList())
                .ToProperty(this, x => x.TextureEntries);
        }

        private async Task<IEnumerable<TextureEntryViewModel>> GenerateTextureEntriesList()
        {
            return EnumerateTextureEntries();
        }

        //Enumerates all the zone objects, filters out any that don't meet the search criteria
        private IEnumerable<TextureEntryViewModel> EnumerateTextureEntries()
        {
            foreach(var entry in Peg.Entries)
            {
                yield return new TextureEntryViewModel(entry.Name);
            }
        }
    }
}
