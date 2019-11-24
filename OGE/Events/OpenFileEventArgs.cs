using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using OGE.ViewModels;

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
