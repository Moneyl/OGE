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
    //Todo: Track open files, extracted files, and edited files (last one is per project, others should be cached cross-project)
    //Todo: Keep undo/redo stack and track changes
    //Todo: Generate modinfo.xml from changes
    public static class ProjectManager
    {
        private static string _workingDirectory;
        private static Dictionary<string, List<FileRef>> _files = new Dictionary<string, List<FileRef>>(); //Todo: Populate _files
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
            stream = Stream.Null;
            if (target.Parent == null)
                return false;

            var parent = target.Parent;
            string parentFileName = Path.GetFileName(parent.FilePath);
            string parentFilePath = parent.FilePath;
            string filename = Path.GetFileName(target.FilePath);

            //If parent isn't top level packfile, must first extract it from it's parent.
            if (!parent.IsTopLevelPackfile)
            {
                if (parent.Parent == null)
                    return false;

                string topParentFilePath = parent.Parent.FilePath;
                string topParentFileName = Path.GetFileName(parent.Parent.FilePath);
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
                //if (!ExtractFileIfNotCached(parentFileName, topParentFilePath, true))
                //    return false;

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

        public static void ExtractAndCacheFile(string parentFilePath, string filename, bool targetIsPackfile = false, bool parentIsEmbeddedPackfile = false, string topLevelParentPath = null)
        {
            //Cases
            // - First level file -- can single extract
            // - First level file -- can't single extract
            // - Second level file -- can single extract -> Might need to extract parent first
            // - Second level file -- can't single extract -> Might need to extract parent first

            //Handle level 1 files
            if (parentIsEmbeddedPackfile && topLevelParentPath == null)
                return;
            //{
                //Check if parent in Files dictionary, add it if not
                //Get parent files list
                //Get parent Packfile instance

                //Check if can single file extract
                    //If it can, extract and return
                    //If it can't, do full extract and return

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

            //var packfile = _workingDirectoryPackfiles.First(item => item.Filename == parentFileName);
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
            //}
            //else //Handle level 2 files
            //{
            //    //Check if parent is in Files dictionary
            //        //If not, check if parents parent is in dictionary
            //            //Handle extracting single file of parent, or fully extracting parents parent, ideally by calling this func again
            //            //Add parent to _embeddedPackfiles and parse metadata and asm_pc (Level two files shouldn't have asm_pc's, but do it anyways for consistency)
            //        //If it is, get it and it's Packfile instance
            //            //Try extract single file from parent, if not, fully extract parent

            //    string topLevelParentName = Path.GetFileName(topLevelParentPath);
            //    string parentFileName = Path.GetFileName(parentFilePath);
            //    string parentKey = $"{topLevelParentName}--{parentFileName}";
            //    string outputPath = $"{GlobalCachePath}{parentKey}\\{filename}";
            //    string packfileOutputPath = $"{GlobalCachePath}{parentKey}\\";

            //    //Ensure output directory exists
            //    Directory.CreateDirectory(packfileOutputPath);

            //    //If parent file not cached, need to extract it from it's parent.
            //    if (!Files.ContainsKey(parentKey))
            //    {
            //        ExtractAndCacheFile(topLevelParentPath, parentFileName, true);
            //    }
            //    Files[parentKey] = new List<FileRef>();
            //    var fileRefs = Files[parentKey];

            //    var packfile = _embeddedPackfiles.First(item => item.Value.Filename == parentFileName).Value;
            //    if (packfile.CanExtractSingleFile() && packfile.TryExtractSingleFile(filename, outputPath))
            //    {
            //        fileRefs.Add(new FileRef(filename, parentKey));
            //    }
            //    else
            //    {
            //        //Failed to extract single file, so extract whole vpp
            //        //Todo: Ask user if they want to extract the whole vpp
            //        WindowLogger.Log($"Failed to extract single file \"{filename}\" from \"{parentFilePath}\". Extracting entire packfile.");
            //        packfile.ExtractFileData(packfileOutputPath);
            //        foreach (var subfileName in packfile.Filenames)
            //        {
            //            fileRefs.Add(new FileRef(subfileName, parentKey));
            //        }
            //    }
            //}
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
                
                //Todo: Consider pre-extracting, or pre-parsing str2s so they're contents are known
                //Todo: Alternatively, pre-parse asm_pc files and get str2 contents from them
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