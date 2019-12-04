using OGE.ViewModels.FileExplorer;

namespace OGE.Editor.Events
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
