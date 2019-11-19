using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;

namespace OGE.ViewModels
{
    public class FileExplorerViewModel : ReactiveObject
    {
        private string _workingDirectory;

        public FileExplorerViewModel(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
        }
    }
}
