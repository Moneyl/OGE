
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OGE.Utility;
using OGE.Utility.Helpers;
using OGE.ViewModels.FileExplorer;
using RfgTools.Formats.Packfiles;

namespace OGE.Editor.Managers
{
    public class CacheManager
    {
        private string _workingDirectory;
        private Dictionary<string, List<CacheFile>> _files = new Dictionary<string, List<CacheFile>>();
        private List<Packfile> _workingDirectoryPackfiles = new List<Packfile>(); //Depth 0 packfiles
        //Key is the parent file name, value is the packfile info. These are depth > 0 packfiles
        private Dictionary<string, Packfile> _embeddedPackfiles = new Dictionary<string, Packfile>();

        //Todo: Maybe have subfolders for different working directories in EditorCache
        private string CachePath { get; }
        public IReadOnlyList<Packfile> WorkingDirectoryPackfiles => _workingDirectoryPackfiles;
        public string WorkingDirectory
        {
            get => _workingDirectory;
            set
            {
                _workingDirectory = value;
                UpdateWorkingDirectoryData();
            }
        }

        public CacheManager(string cachePath)
        {
            CachePath = cachePath;
            Directory.CreateDirectory(CachePath);
            ScanEditorCache();
        }

        private void UpdateWorkingDirectoryData()
        {
            if(!Directory.Exists(_workingDirectory))
                return;

            Directory.CreateDirectory(_workingDirectory);
            _workingDirectoryPackfiles.Clear();
            var directoryFiles = Directory.GetFiles(WorkingDirectory);

            foreach (var filePath in directoryFiles)
            {
                if (!PathHelpers.IsPackfilePath(filePath))
                    continue;

                var packfile = new Packfile(false);
                packfile.ReadMetadata(filePath);
                packfile.ParseAsmFiles($"{CachePath}{packfile.Filename}\\");

                _workingDirectoryPackfiles.Add(packfile);
            }
        }

        public void ScanEditorCache()
        {
            if (!Directory.Exists(CachePath))
                Directory.CreateDirectory(CachePath);

            var cacheFolders = Directory.GetDirectories(CachePath);
            foreach (var cacheFolder in cacheFolders)
            {
                string parentFileName = Path.GetFileName(cacheFolder);
                var files = Directory.GetFiles(cacheFolder);
                if (files.Length <= 0)
                    continue;

                _files[parentFileName] = new List<CacheFile>();
                var fileList = _files[parentFileName];
                foreach (var file in files)
                {
                    fileList.Add(new CacheFile(Path.GetFileName(file), parentFileName));
                }
            }
        }

        public bool TryGetFile(string targetFilename, FileExplorerItemViewModel parent, out Stream stream, bool extractIfNotCached = false)
        {
            stream = Stream.Null;
            string parentFolderPathOverride = $"{CachePath}{parent.Key}\\";

            //Handle depth 1 files (direct subfile of working dir packfile)
            if (parent.IsTopLevelPackfile) 
            {
                //Extract target file from parent
                if (ExtractFileIfNotCached(targetFilename, parent))
                    stream = GetCachedFileStream(targetFilename, parent.Filename);

                return stream != Stream.Null;
            }
            //Handle uncached depth > 1 files
            if (!IsFileCached(targetFilename, parent.Key, parentFolderPathOverride)) 
            {
                //If parent isn't top level pack file, must first extract parent from it's parent (parent.Parent)
                if (parent.Parent == null)
                    return false;

                //Ensure parent is extracted from it's parent
                ExtractFileIfNotCached(parent.Filename, parent.Parent);

                //Try to extract target file from parent
                if (!ExtractFileIfNotCached(targetFilename, parent))
                    return false;
            }

            stream = GetCachedFileStream(targetFilename, parent.Key, parentFolderPathOverride);
            return stream != Stream.Null;
        }

        /// <summary>
        /// Extracts a one level deep file from it's parent.
        /// Does not work for files that are more than one level deep!
        /// </summary>
        /// <param name="targetFilename">The name of the target file to be extracted.</param>
        /// <param name="parent">The parent file.</param>
        /// <returns>Returns a bool that is true if it was successful and false otherwise.</returns>
        public bool ExtractFileIfNotCached(string targetFilename, FileExplorerItemViewModel parent)
        {
            string parentFolderPathOverride = !parent.IsTopLevelPackfile
                ? $"{CachePath}{parent.Key}\\"
                : null;

            //If file isn't cached, extract and return success value of that
            return IsFileCached(targetFilename, parent.Key) || ExtractAndCacheFile(targetFilename, parent);
        }

        public bool IsFileCached(string filename, string parentKey, string parentFolderPathOverride = null)
        {
            if (_files.TryGetValue(parentKey, out List<CacheFile> files))
            {
                foreach (var subFile in files)
                {
                    if (!string.Equals(subFile.Filename, filename, StringComparison.CurrentCultureIgnoreCase))
                        continue;
                    if (!subFile.FileExists(parentFolderPathOverride))
                        break;

                    return true;
                }
            }
            return false;
        }

        private Stream GetCachedFileStream(string filename, string parentKey, string parentFolderPathOverride = null)
        {
            if (_files.TryGetValue(parentKey, out List<CacheFile> files))
            {
                foreach (var subFile in files)
                {
                    if (!string.Equals(subFile.Filename, filename, StringComparison.CurrentCultureIgnoreCase))
                        continue;
                    if (!subFile.TryOpenOrGet(out Stream fileStream, parentFolderPathOverride))
                        break;

                    return fileStream;
                }
            }
            return Stream.Null;
        }

        //Todo: Make this support files that are two layers deep
        public bool ExtractAndCacheFile(string targetFilename, FileExplorerItemViewModel parent)
        {
            if (parent.IsEmbeddedPackfile && parent.Parent == null)
                return false;

            //Form output paths
            string packfileOutputPath = $"{CachePath}{parent.Key}\\";
            string targetOutputPath = $"{packfileOutputPath}\\{targetFilename}";
            //Ensure output directory exists
            Directory.CreateDirectory(packfileOutputPath);
            //Get parent subfiles list, create if it doesn't exist.
            var fileRefs = _files.GetOrCreate(parent.Key);
            //Get parent packfile
            var packfile = parent.IsEmbeddedPackfile
                ? _embeddedPackfiles.First(item => item.Value.Filename == parent.Filename).Value
                : _workingDirectoryPackfiles.First(item => item.Filename == parent.Filename);

            if (packfile.TryExtractSingleFile(targetFilename, targetOutputPath))
            {
                fileRefs.Add(new CacheFile(targetFilename, parent.Filename));
            }
            else
            {
                //Failed to extract single file, so extract whole vpp
                //Todo: Ask user if they want to extract the whole vpp
                WindowLogger.Log($"Failed to extract single file \"{targetFilename}\" from \"{parent.FilePath}\". Extracting entire packfile.");
                packfile.ExtractFileData(packfileOutputPath);
                foreach (var subfileName in packfile.Filenames)
                {
                    fileRefs.Add(new CacheFile(subfileName, parent.Filename));
                }
            }

            if (PathHelpers.IsPackfilePath(targetFilename))
            {
                string targetKey = $"{parent.Filename}--{targetFilename}";
                string targetCache = $"{CachePath}{targetKey}\\";
                Directory.CreateDirectory(targetCache);

                var targetPackfile = new Packfile(false);
                targetPackfile.ReadMetadata(targetOutputPath);
                targetPackfile.ParseAsmFiles(targetCache);
                _embeddedPackfiles[targetKey] = targetPackfile;
            }
            return true;
        }
    }
}
