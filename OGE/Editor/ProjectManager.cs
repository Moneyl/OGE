using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OGE.Helpers;
using OGE.Utility;
using OGE.ViewModels;
using RfgTools.Formats.Packfiles;

namespace OGE.Editor
{
    //Todo: Keep undo/redo stack and track changes
    //Todo: Generate modinfo.xml from changes
    public static class ProjectManager
    {
        private static string _workingDirectory;
        private static Dictionary<string, List<FileRef>> _files = new Dictionary<string, List<FileRef>>();
        private static List<Packfile> _workingDirectoryPackfiles = new List<Packfile>();
        private static Dictionary<string, Packfile> _embeddedPackfiles = new Dictionary<string, Packfile>();

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
        //Key is the parent file name, value is the packfile info.
        public static IReadOnlyDictionary<string, Packfile> EmbeddedPackfiles => _embeddedPackfiles;

        static ProjectManager()
        {
            ScanEditorCache();
        }

        public static void Init()
        {
            Directory.CreateDirectory(GlobalCachePath);
        }

        public static bool TryGetFile(FileExplorerItemViewModel target, out Stream stream, bool extractIfNotCached = false)
        {
            bool result = TryGetFile(target.FilePath, out Stream outStream, target.Parent, extractIfNotCached);
            stream = outStream;
            return result;
        }

        public static bool TryGetFile(string targetFilePath, out Stream stream, FileExplorerItemViewModel targetParent = null, bool extractIfNotCached = false)
        {
            stream = Stream.Null;
            if (targetParent == null)
                return false;

            string parentFileName = Path.GetFileName(targetParent.FilePath);
            string parentFilePath = targetParent.FilePath;
            string filename = Path.GetFileName(targetFilePath);

            //If parent isn't top level packfile, must first extract it from it's parent.
            if (!targetParent.IsTopLevelPackfile)
            {
                if (targetParent.Parent == null)
                    return false;

                string topParentFilePath = targetParent.Parent.FilePath;
                string topParentFileName = Path.GetFileName(targetParent.Parent.FilePath);
                string parentKey = $"{topParentFileName}--{parentFileName}";

                //If file is cached, just return it.
                if (IsFileCached(filename, parentKey))
                {
                    stream = GetCachedFileStream(filename, parentKey);
                    return stream != Stream.Null;
                }

                //Ensure parent is extracted. Extract parent from top level parent - parent is level 1 file in this case
                if (!IsFileCached(parentFileName, topParentFileName))
                {
                    ExtractAndCacheFile(topParentFileName, parentFileName, true, false);
                }

                //Try to extract target file from parent
                if (!ExtractFileIfNotCached(filename, $"{topParentFilePath}\\{parentFileName}", false, true, topParentFilePath))
                    return false;

                stream = GetCachedFileStream(filename, parentKey, $"{GlobalCachePath}{parentKey}\\");
                return stream != Stream.Null;
            }

            //Extract file from parent - target is a level 1 file in this case
            if (ExtractFileIfNotCached(filename, parentFilePath))
                stream = GetCachedFileStream(filename, parentFileName);

            return stream != Stream.Null;
        }

        /// <summary>
        /// Extracts a one level deep file from it's parent.
        /// Does not work for files that are more than one level deep!
        /// </summary>
        /// <param name="targetFileName">The name of the target file to be extracted.</param>
        /// <param name="parentFilePath">The path to the parent file.</param>
        /// <returns>Returns a bool that is true if it was successful and false otherwise.</returns>
        public static bool ExtractFileIfNotCached(string targetFileName, string parentFilePath, bool targetIsPackfile = false, bool parentIsEmbeddedPackfile = false, string topLevelParentPath = null)
        {
            string parentFileName = Path.GetFileName(parentFilePath);
            if (!IsFileCached(targetFileName, parentFileName))
                ExtractAndCacheFile(parentFilePath, targetFileName, targetIsPackfile, parentIsEmbeddedPackfile, topLevelParentPath);

            if (parentIsEmbeddedPackfile)
            {
                if (topLevelParentPath == null)
                    return false;

                string topLevelParentName = Path.GetFileName(topLevelParentPath);
                string parentKey = $"{topLevelParentName}--{parentFileName}";
                return IsFileCached(targetFileName, parentKey, $"{GlobalCachePath}{parentKey}\\");
            }
            else
            { 
                return IsFileCached(targetFileName, parentFileName);
            }
        }

        public static bool IsFileCached(string filename, string parentFileName, string parentFolderPathOverride = null)
        {
            if (Files.TryGetValue(parentFileName, out List<FileRef> files))
            {
                foreach (var subFile in files)
                {
                    if (subFile.Filename != filename)
                        continue;
                    if (!subFile.TryOpenOrGet(out Stream fileStream, parentFolderPathOverride))
                        break;

                    return true;
                }
            }
            return false;
        }

        private static Stream GetCachedFileStream(string filename, string parentFilePath, string parentFolderPathOverride = null)
        {
            if (Files.TryGetValue(parentFilePath, out List<FileRef> files))
            {
                foreach (var subFile in files)
                {
                    if (subFile.Filename != filename)
                        continue;
                    if (!subFile.TryOpenOrGet(out Stream fileStream, parentFolderPathOverride))
                        break;

                    return fileStream;
                }
            }
            return Stream.Null;
        }

        //Todo: Make this support files that are two layers deep
        public static void ExtractAndCacheFile(string parentFilePath, string filename, bool targetIsPackfile = false, bool parentIsEmbeddedPackfile = false, string topLevelParentPath = null)
        {

            if (parentIsEmbeddedPackfile && topLevelParentPath == null)
                return;

            string parentFileName = Path.GetFileName(parentFilePath);
            string parentKey;
            string outputPath;
            string packfileOutputPath;
            string topLevelParentName = "NOT SET";
            if (parentIsEmbeddedPackfile)
            {
                topLevelParentName = Path.GetFileName(topLevelParentPath);
                parentKey = $"{topLevelParentName}--{parentFileName}";
                outputPath = $"{GlobalCachePath}{parentKey}\\{filename}";
                packfileOutputPath = $"{GlobalCachePath}{parentKey}\\";
            }
            else
            {
                parentKey = parentFileName;
                outputPath = $"{GlobalCachePath}{parentFileName}\\{filename}";
                packfileOutputPath = $"{GlobalCachePath}{parentFileName}\\";
            }

            //Ensure output directory exists
            Directory.CreateDirectory(packfileOutputPath);

            //Create parent list if it doesn't exist. Get parent list
            if (!Files.ContainsKey(parentKey))
                Files[parentKey] = new List<FileRef>();
            var fileRefs = Files[parentKey];

            Packfile packfile = null;
            if(parentIsEmbeddedPackfile)
                packfile = _embeddedPackfiles.First(item => item.Value.Filename == parentFileName).Value;
            else
                packfile = _workingDirectoryPackfiles.First(item => item.Filename == parentFileName);


            if (packfile.CanExtractSingleFile() && packfile.TryExtractSingleFile(filename, outputPath))
            {
                fileRefs.Add(new FileRef(filename, parentFileName));
            }
            else
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

            if (targetIsPackfile)
            {
                string targetCache = $"{GlobalCachePath}{parentFileName}--{filename}\\";
                Directory.CreateDirectory(targetCache);

                var targetPackfile = new Packfile(false);
                targetPackfile.ReadMetadata(outputPath);
                targetPackfile.ParseAsmFiles(targetCache);
                _embeddedPackfiles[$"{parentFileName}--{filename}"] = targetPackfile;
            }
        }

        private static void UpdateWorkingDirectoryData()
        {
            Directory.CreateDirectory(_workingDirectory);
            _workingDirectoryPackfiles.Clear();
            var directoryFiles = Directory.GetFiles(WorkingDirectory);

            foreach (var filePath in directoryFiles)
            {
                if(!PathHelpers.IsPackfilePath(filePath))
                    continue;

                var packfile = new Packfile(false);
                packfile.ReadMetadata(filePath);
                packfile.ParseAsmFiles($"{GlobalCachePath}{packfile.Filename}\\");

                _workingDirectoryPackfiles.Add(packfile);
            }
        }

        public static void ScanEditorCache()
        {
            if (!Directory.Exists(GlobalCachePath))
                Directory.CreateDirectory(GlobalCachePath);

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