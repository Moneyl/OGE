using System.IO;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using FontAwesome.WPF;
using OGE.Editor;
using OGE.Editor.Managers;
using OGE.Utility.Helpers;
using ReactiveUI;

namespace OGE.ViewModels.FileExplorer
{
    public class FileExplorerItemViewModel : TreeItem
    {
        private string _filename;

        public CacheFile File { get; private set; }
        public FileExplorerItemViewModel Parent { get; private set; }
        public uint Depth { get; set; } = 0;
        public string FileExtension { get; set; }
        public override object ViewModel => this;
        public string Key { get; private set; }
        public bool IsEmbeddedPackfile { get; private set; }
        public string Filename
        {
            get => _filename;
            set
            {
                _filename = this.RaiseAndSetIfChanged(ref _filename, value);
                FileExtension = Path.GetExtension(_filename);

                //Set Key
                if (Depth == 0 || Parent == null)
                    Key = Filename;
                else
                    Key = $"{Parent.Filename}--{Filename}";

                //Set IsEmbeddedPackfile
                IsEmbeddedPackfile = PathHelpers.IsPackfilePath(Filename) && Depth > 0;
            }
        }

        public FileExplorerItemViewModel(string filename, FileExplorerItemViewModel parent, uint depth, CacheFile cacheFile = null)
        {
            File = cacheFile;
            Parent = parent;
            Depth = depth;
            Filename = filename;
            UpdateIcon();
        }

        public bool GetCacheFile(bool extractIfNotFound = false, bool readFormatData = false)
        {
            if (!ProjectManager.TryGetCacheFile(Filename, Parent?.Filename, out CacheFile file, extractIfNotFound))
                return false;

            File = file;
            if(readFormatData)
                File.ReadFormatData();

            return true;
        }

        public void CollapseAll()
        {
            foreach (var treeItem in Children)
            {
                var child = (FileExplorerItemViewModel)treeItem;
                child.CollapseAll();
            }
            Collapse();
        }

        public void FillChildrenList(string searchTerm)
        {
            if (Depth == 0) //Handle depth 0 packfiles
            {
                if(File?.PackfileData == null)
                    return;

                foreach (var subFile in File.PackfileData.DirectoryEntries)
                {
                    if(!subFile.FileName.Contains(searchTerm))
                        continue;

                    var explorerItem = new FileExplorerItemViewModel(subFile.FileName, this, Depth + 1);
                    explorerItem.FillChildrenList(searchTerm);
                    AddChild(explorerItem);
                }
            }
            else //Handle depth > 0 packfiles
            {
                if (!IsEmbeddedPackfile || Parent == null)
                    return;

                //Check parent for asm file, get children files. Cheaper than extracting/finding and parsing
                string filenameNoExtension = Path.GetFileNameWithoutExtension(Filename);
                foreach (var asmFile in Parent.File.PackfileData.AsmFiles)
                {
                    foreach (var container in asmFile.Containers)
                    {
                        if(container.Name != filenameNoExtension)
                            continue;

                        foreach (var primitive in container.Primitives)
                        {
                            if(!primitive.Name.Contains(searchTerm))
                                continue;

                            //Todo: Figure out a better way of handling this. Likely other files with this problem
                            //Check for asm_pc/file name mismatch. Currently only occurs with one file
                            //The asm has the extension cvbm_pc, when it should be cpeg_pc. Breaking cache operations on it
                            if (primitive.Name == "interface-badges.cvbm_pc")
                            {
                                var explorerItem = new FileExplorerItemViewModel("interface-badges.cpeg_pc", this, Depth + 1);
                                AddChild(explorerItem);
                            }
                            else
                            {
                                var explorerItem = new FileExplorerItemViewModel(primitive.Name, this, Depth + 1);
                                AddChild(explorerItem);
                            }
                        }
                    }
                }
            }
        }

        protected override void OnExpansionChanged()
        {
            base.OnExpansionChanged();
            UpdateIcon();
        }

        private void UpdateIcon()
        {
            Icon = FileExtension switch
            {
                ".vpp_pc" => IsExpanded ? FontAwesomeIcon.FolderOpen : FontAwesomeIcon.Folder,
                ".str2_pc" => IsExpanded ? FontAwesomeIcon.FolderOpen : FontAwesomeIcon.Folder,
                ".xtbl" => FontAwesomeIcon.FileCodeOutline,
                ".mtbl" => FontAwesomeIcon.FileCodeOutline,
                ".dtodx" => FontAwesomeIcon.FileCodeOutline,
                ".gtodx" => FontAwesomeIcon.FileCodeOutline,
                ".scriptx" => FontAwesomeIcon.FileCodeOutline,
                ".cpeg_pc" => FontAwesomeIcon.FileImageOutline,
                ".gpeg_pc" => FontAwesomeIcon.FileImageOutline,
                ".cvbm_pc" => FontAwesomeIcon.FileImageOutline,
                ".gvbm_pc" => FontAwesomeIcon.FileImageOutline,
                ".xwb_pc" => FontAwesomeIcon.FileAudioOutline,
                ".rfgzone_pc" => FontAwesomeIcon.MapOutline,
                ".layer_pc" => FontAwesomeIcon.MapOutline,
                _ => FontAwesomeIcon.File
            };

            ForegroundBrush = Icon switch
            {
                FontAwesomeIcon.Folder => new SolidColorBrush(Color.FromRgb(216, 172, 106)),
                FontAwesomeIcon.FolderOpen => new SolidColorBrush(Color.FromRgb(216, 172, 106)),
                _ => new SolidColorBrush(Color.FromRgb(196, 196, 196))
            };
            ForegroundBrush.Freeze();
        }
    }
}
