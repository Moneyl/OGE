using System.Reactive.Disposables;
using OGE.ViewModels.TextureViewer;
using ReactiveUI;

namespace OGE.Views.TextureViewer
{
    public partial class TextureEntryView : ReactiveUserControl<TextureEntryViewModel>
    {
        public TextureEntryView()
        {
            InitializeComponent();

            this.WhenActivated(disposable =>
            {
                this.OneWayBind(ViewModel,
                        vm => vm.Name,
                        v => v.NameRun.Text)
                    .DisposeWith(disposable);
            });
        }
    }
}
