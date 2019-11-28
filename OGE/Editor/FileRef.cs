using System.IO;

namespace OGE.Editor
{
    public class FileRef
    {
        private string _filename;
        private string _parentFile;
        private bool _fileOpen = false;
        private Stream _fileStream = Stream.Null;

        public string Filename
        {
            get => _filename;
            private set => _filename = value;
        }

        public string ParentFile
        {
            get => _parentFile;
            private set => _parentFile = value;
        }

        public bool FileOpen
        {
            get => _fileOpen;
            private set => _fileOpen = value;
        }

        public RfgFileTypes FileType { get; private set; }

        public bool InEditorCache { get; private set; } = false;
        public bool InProjectCache { get; private set; } = false;

        public FileRef(string filename, string parentFile)
        {
            Filename = filename;
            ParentFile = parentFile;
            UpdateFileType();
        }

        private void UpdateFileType()
        {
            string extension = Path.GetExtension(Filename);
            if (extension == ".vpp_pc")
            {
                FileType = RfgFileTypes.Packfile;
            }
            else if (extension == ".str2_pc")
            {
                FileType = RfgFileTypes.Container;
            }
            else
            {
                FileType = RfgFileTypes.Primitive;
            }
        }

        public bool FileExists(string parentFolderPathOverride = null)
        {
            string filePath;
            if (parentFolderPathOverride == null)
            {
                filePath = $"{ProjectManager.GlobalCachePath}{ParentFile}\\{Filename}"; //Todo: Maybe cache this
            }
            else
            {
                filePath = $"{parentFolderPathOverride}\\{Filename}";
            }

            return File.Exists(filePath);
        }

        public bool TryOpenOrGet(out Stream stream, string parentFolderPathOverride = null)
        {
            if (FileOpen)
            {
                stream = _fileStream;
                return true;
            }
            if (TryOpenFile(parentFolderPathOverride))
            {
                stream = _fileStream;
                return true;
            }

            stream = Stream.Null;
            return false;
        }

        //Todo: Have separate global and project file caches, currently only has global one
        private bool TryOpenFile(string parentFolderPathOverride = null)
        {
            _fileStream.Close();
            _fileStream = Stream.Null;
            string filePath;
            if (parentFolderPathOverride == null)
            {
                filePath = $"{ProjectManager.GlobalCachePath}{ParentFile}\\{Filename}"; //Todo: Maybe cache this
            }
            else
            {
                filePath = $"{parentFolderPathOverride}\\{Filename}";
            }

            //Check if file exists in EditorCache
            if (!File.Exists(filePath))
                return false;

            _fileStream = new FileStream(filePath, FileMode.Open);
            return true;
        }
    }
}
