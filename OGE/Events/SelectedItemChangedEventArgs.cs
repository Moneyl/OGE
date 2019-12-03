using OGE.ViewModels.FileExplorer;

namespace OGE.Events
{
    /// <summary>
    /// Triggered when the selected item in the file explorer changes
    /// </summary>
    class SelectedItemChangedEventArgs
    {
        public FileExplorerItemViewModel SelectedItem { get; }

        public SelectedItemChangedEventArgs(FileExplorerItemViewModel selectedItem)
        {
            SelectedItem = selectedItem;
        }
    }
}
