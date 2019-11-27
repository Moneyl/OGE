using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using RfgTools.Formats.Textures;

namespace OGE.ViewModels
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

        public TextureViewerViewModel(PegFile peg)
        {
            _peg = peg;
        }
    }
}
