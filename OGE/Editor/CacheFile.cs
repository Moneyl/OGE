using System.IO;

namespace OGE.Editor
{
    public class CacheFile
    {
        private Stream _fileStream = Stream.Null;

        public string Filename { get; private set; }
        public string ParentFile { get; private set; }
        public bool IsOpen { get; private set; } = false;
        public RfgFileTypes FileType { get; private set; }
        public bool InEditorCache { get; private set; } = false;
        public bool InProjectCache { get; private set; } = false;

        public CacheFile(string filename, string parentFile)
        {
            Filename = filename;
            ParentFile = parentFile;
            UpdateFileType();
        }

        private void UpdateFileType()
        {
            string extension = Path.GetExtension(Filename);
            FileType = extension switch
            {
                ".vpp_pc" => RfgFileTypes.Packfile,
                ".str2_pc" => RfgFileTypes.Container,
                _ => RfgFileTypes.Primitive
            };
        }

        public bool FileExists(string parentFolderPathOverride = null)
        {
            string filePath = parentFolderPathOverride == null 
                ? $"{ProjectManager.GlobalCachePath}{ParentFile}\\{Filename}" 
                : $"{parentFolderPathOverride}\\{Filename}";

            return File.Exists(filePath);
        }

        public bool TryOpenOrGet(out Stream stream, string parentFolderPathOverride = null)
        {
            if (IsOpen || TryOpenFile(parentFolderPathOverride))
            {
                stream = _fileStream;
                return true;
            }

            stream = Stream.Null;
            return false;
        }

        private bool TryOpenFile(string parentFolderPathOverride = null)
        {
            _fileStream.Close();
            _fileStream = Stream.Null;
            string filePath = parentFolderPathOverride == null 
                ? $"{ProjectManager.GlobalCachePath}{ParentFile}\\{Filename}"
                : $"{parentFolderPathOverride}\\{Filename}";

            //Todo: Check project cache first once those are added
            //Check if file exists in EditorCache
            if (!File.Exists(filePath))
                return false;

            _fileStream = new FileStream(filePath, FileMode.Open);
            return true;
        }
    }
}
