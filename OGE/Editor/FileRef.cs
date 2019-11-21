using System.IO;

namespace OGE.Editor
{
    class FileRef
    {
        private string _filename;
        private string _parentFile;
        private bool _fileOpen;
        private Stream _fileStream = Stream.Null;

        public string Filename
        {
            get => _filename;
            protected set => _filename = value;
        }

        public string ParentFile
        {
            get => _parentFile;
            protected set => _parentFile = value;
        }

        public bool FileOpen
        {
            get => _fileOpen;
            protected set => _fileOpen = value;
        }

        FileRef(string filename, string parentFile)
        {
            Filename = filename;
            ParentFile = parentFile;
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

        private bool TryOpenFile()
        {
            _fileStream.Close();
            _fileStream = Stream.Null;
            
            //Check if file exists in projects edit cache
                //If so, open it

                //Else, check edited file cache
                    //If in there, open

                    //Else, pull from vpp/str2 and save in cache

            return false;
        }
    }
}
