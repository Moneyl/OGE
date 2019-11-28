using OGE.ViewModels.FileExplorer;

namespace OGE.Events
{
    public class OpenFileEventArgs
    {
        public FileExplorerItemViewModel TargetItem { get; private set; }

        public OpenFileEventArgs(FileExplorerItemViewModel targetItem)
        {
            TargetItem = targetItem;
        }
    }
}
