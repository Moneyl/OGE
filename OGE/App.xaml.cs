using System.Reflection;
using System.Windows;
using OGE.ViewModels;
using OGE.ViewModels.FileExplorer;
using OGE.ViewModels.TextureViewer;
using OGE.Views;
using OGE.Views.FileExplorer;
using OGE.Views.TextureViewer;
using ReactiveUI;
using Splat;

namespace OGE
{
    public partial class App : Application
    {
        public App()
        {
            Locator.CurrentMutable.Register(() => new MainWindow(), typeof(IViewFor<AppViewModel>));
            Locator.CurrentMutable.Register(() => new FileExplorerView(), typeof(IViewFor<FileExplorerViewModel>));
            Locator.CurrentMutable.Register(() => new FileExplorerItemView(), typeof(IViewFor<FileExplorerItemViewModel>));
            Locator.CurrentMutable.Register(() => new PropertiesPanelView(), typeof(IViewFor<PropertiesPanelViewModel>));
            Locator.CurrentMutable.Register(() => new TextureViewerView(), typeof(IViewFor<TextureViewerViewModel>));
            Locator.CurrentMutable.Register(() => new TextureEntryView(), typeof(IViewFor<TextureEntryViewModel>));
        }
    }
}
