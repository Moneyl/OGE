using ReactiveUI;

namespace OGE.ViewModels.TextureViewer
{
    public class TextureEntryViewModel : ReactiveObject
    {
        public string Name { get; private set; }
        public int Index { get; private set; }

        public TextureEntryViewModel(string name, int index)
        {
            Name = name;
            Index = index;
        }
    }
}
