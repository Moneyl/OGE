using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OGE.Editor
{
    //Todo: Track open files, extracted files, and edited files (last one is per project, others should be cached cross-project)
    //Todo: Keep undo/redo stack and track changes
    //Todo: Generate modinfo.xml from changes
    public static class ProjectManager
    {
        private static string _workingDirectory;
        private static Dictionary<string, List<FileRef>> _files = new Dictionary<string, List<FileRef>>(); //Todo: Populate _files

        public static string WorkingDirectory
        {
            get => _workingDirectory;
            set => _workingDirectory = value;
        }

        static ProjectManager()
        {

        }

        public static bool TryGetFile(string filename, string parentFile, out Stream stream)
        {
            stream = Stream.Null;
            if (!_files.TryGetValue(parentFile, out List<FileRef> files))
                return false;

            foreach (var subFile in files)
            {
                if(subFile.Filename != filename)
                    continue;
                if (!subFile.TryOpenOrGet(out Stream fileStream))
                    break;

                stream = fileStream;
                return true;
            }
            return false;
        }
    }
}
