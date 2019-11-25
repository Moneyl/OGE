using System.Reflection;
using System.Windows;
using ReactiveUI;
using Splat;

namespace OGE
{
    public partial class App : Application
    {
        public App()
        {
            //Todo: See if there's a performance gain from manually registering ViewModels here
            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetCallingAssembly());
        }
    }
}
