using System.Windows;
using System.Windows.Controls;

namespace OGE.Utility
{
    public static class WindowLogger
    {
        public static VirtualizingStackPanel LogPanel { get; private set; }

        public static void SetLogPanel(VirtualizingStackPanel logPanel)
        {
            LogPanel = logPanel;
        }

        public static void Log(string message)
        {
            LogPanel?.Children.Add(new TextBlock
            {
                Text = message,
                Margin = new Thickness(5, 0, 0, 0)
            });
        }
    }
}
