using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;

namespace OGE.ViewModels.TextureViewer
{
    public class TextureEntryViewModel : ReactiveObject
    {
        public string Name { get; private set; }

        public TextureEntryViewModel(string name)
        {
            Name = name;
        }
    }
}
