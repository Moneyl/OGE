using System;
using System.Collections.Generic;
using System.IO;
using OGE.Helpers;
using OGE.Utility;
using RfgTools.Formats.Packfiles;

namespace OGE.Editor
{
    //Todo: Track open files, extracted files, and edited files (last one is per project, others should be cached cross-project)
    //Todo: Keep undo/redo stack and track changes
    //Todo: Generate modinfo.xml from changes
    public static class ProjectManager
    {
        private static string _workingDirectory;
        private static Dictionary<string, List<FileRef>> _files = new Dictionary<string, List<FileRef>>(); //Todo: Populate _files
        private static List<Packfile> _workingDirectoryPackfiles = new List<Packfile>();

        //Todo: Maybe have subfolders for different working directories in EditorCache
        public static string GlobalCachePath { get; private set; } = @".\EditorCache\";

        public static string WorkingDirectory
        {
            get => _workingDirectory;
            set
            {
                _workingDirectory = value;
                UpdateWorkingDirectoryData();
            }
        }

        public static Dictionary<string, List<FileRef>> Files => _files;
        public static IReadOnlyList<Packfile> WorkingDirectoryPackfiles => _workingDirectoryPackfiles;

        static ProjectManager()
        {
            ScanEditorCache();
        }

        public static void Init()
        {
            Directory.CreateDirectory(GlobalCachePath);
        }

        public static bool IsFileCached(string filename, string parentFileName)
        {
            if (Files.TryGetValue(parentFileName, out List<FileRef> files))
            {
                foreach (var subFile in files)
                {
                    if (subFile.Filename != filename)
                        continue;
                    if (!subFile.TryOpenOrGet(out Stream fileStream))
                        break;

                    return true;
                }
            }
            return false;
        }

        private static Stream GetCachedFileStream(string filename, string parentFilePath)
        {
            if (Files.TryGetValue(parentFilePath, out List<FileRef> files))
            {
                foreach (var subFile in files)
                {
                    if (subFile.Filename != filename)
                        continue;
                    if (!subFile.TryOpenOrGet(out Stream fileStream))
                        break;

                    return fileStream;
                }
            }
            return Stream.Null;
        }

        public static bool TryGetFile(string filename, string parentFilePath, out Stream stream, bool extractIfNotFound = false)
        {
            stream = Stream.Null;
            string parentFileName = Path.GetFileName(parentFilePath);

            if (!IsFileCached(filename, parentFileName))
            {
                if (!extractIfNotFound)
                    return false;

                ExtractAndCacheFile(parentFilePath, filename);
            }

            stream = GetCachedFileStream(filename, parentFileName);
            return stream != Stream.Null;
        }

        public static void ExtractAndCacheFile(string parentFilePath, string filename)
        {
            string parentFileName = Path.GetFileName(parentFilePath);
            if (PathHelpers.IsPackfilePath(filename))
                Directory.CreateDirectory($"{GlobalCachePath}{parentFileName}\\{filename}");
            else
                Directory.CreateDirectory($"{GlobalCachePath}{parentFileName}\\");

            if (!Files.ContainsKey(parentFileName)) 
                Files[parentFileName] = new List<FileRef>();


            var fileRefs = Files[parentFileName];
            string outputPath = $"{GlobalCachePath}{parentFileName}\\{filename}";
            string packfileOutputPath = $"{GlobalCachePath}{parentFileName}\\";

            foreach (var packfile in _workingDirectoryPackfiles)
            {
                if (packfile.Filename == parentFileName)
                {
                    if (packfile.CanExtractSingleFile() && packfile.TryExtractSingleFile(filename, outputPath))
                    {
                        fileRefs.Add(new FileRef(filename, parentFileName));
                    }
                    else
                    {
                        try
                        {
                            //Failed to extract single file, so extract whole vpp
                            //Todo: Ask user if they want to extract the whole vpp
                            WindowLogger.Log($"Failed to extract single file \"{filename}\" from \"{parentFilePath}\". Extracting entire packfile.");
                            packfile.ExtractFileData(packfileOutputPath);
                            foreach (var subfileName in packfile.Filenames)
                            {
                                fileRefs.Add(new FileRef(subfileName, parentFileName));
                            }
                        }
                        catch (Exception e)
                        {
                            WindowLogger.Log($"Exception caught while trying to extract \"{parentFileName}\"! " +
                                             $"Failed to extract files. Message: \"{e.Message}\"");
                            throw;
                        }
                    }
                }
            }
        }

        private static void UpdateWorkingDirectoryData()
        {
            _workingDirectoryPackfiles.Clear();
            var directoryFiles = Directory.GetFiles(WorkingDirectory);

            foreach (var filePath in directoryFiles)
            {
                if(!PathHelpers.IsPackfilePath(filePath))
                    return;

                var packfile = new Packfile(false);
                packfile.ReadMetadata(filePath);
                _workingDirectoryPackfiles.Add(packfile);
            }
        }

        public static void ScanEditorCache()
        {
            var cacheFolders = Directory.GetDirectories(GlobalCachePath);
            foreach (var cacheFolder in cacheFolders)
            {
                string parentFileName = Path.GetFileName(cacheFolder);
                var files = Directory.GetFiles(cacheFolder);
                if(files.Length <= 0)
                    continue;

                _files[parentFileName] = new List<FileRef>();
                var fileList = _files[parentFileName];
                foreach (var file in files)
                {
                    fileList.Add(new FileRef(Path.GetFileName(file), parentFileName));
                }
            }
        }
    }
}