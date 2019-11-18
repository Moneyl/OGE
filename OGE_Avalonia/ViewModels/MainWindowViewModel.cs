using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Interactivity;

namespace OGE_Avalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string Greeting => "Welcome to Avalonia!";

        private void MenuExit_OnClick(object sender, RoutedEventArgs e)
        {
#if DEBUG
            var debug = 2; //Set breakpoint here for easy debugging
#else
            Close();
#endif
        }
    }
}
