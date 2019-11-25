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

        public bool TryOpenOrGet(out Stream stream)
        {
            if (FileOpen)
            {
                stream = _fileStream;
                return true;
            }
            if (TryOpenFile())
            {
                stream = _fileStream;
                return true;
            }

            stream = Stream.Null;
            return false;
        }

        //Todo: Have separate global and project file caches, currently only has global one
        private bool TryOpenFile()
        {
            _fileStream.Close();
            _fileStream = Stream.Null;
            string filePath = $"{ProjectManager.GlobalCachePath}{ParentFile}\\{Filename}"; //Todo: Maybe cache this

            //Check if file exists in EditorCache
            if (!File.Exists(filePath))
                return false;

            //Todo: Add support for per-project edit cache, and check the active projects cache before editor cache
            _fileStream = new FileStream(filePath, FileMode.Open);
            return true;

            //Check if file exists in projects edit cache
                //If so, open it

                //Else, check edited file cache
                    //If in there, open

                    //Else, pull from vpp/str2 and save in cache
        }
    }
}
