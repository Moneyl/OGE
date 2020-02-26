using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms.Design;
using OGE.Utility;
using OGE.Utility.Helpers;

namespace OGE.Editor.Managers
{
    /// <summary>
    /// Provides way to access and cache the subfiles of RFG packfiles (vpp_pc and str2_pc).
    /// The goal of this is to avoid extracting a subfile more than once.
    /// </summary>
    public class CacheManager
    {
        private string _workingDirectory;
        private List<CacheFile> _cacheFiles = new List<CacheFile>();

        public IReadOnlyList<CacheFile> CacheFiles => _cacheFiles.AsReadOnly();
        public string WorkingDirectory
        {
            get => _workingDirectory;
            set
            {
                if(string.IsNullOrEmpty(value))
                    return;

                _workingDirectory = value;
                UpdateWorkingDirectoryData();
            }
        }
        private string CachePath { get; }

        public CacheManager(string cachePath)
        {
            CachePath = cachePath;
            Directory.CreateDirectory(CachePath);
            ScanEditorCache();
        }

        private void UpdateWorkingDirectoryData()
        {
            Directory.CreateDirectory(_workingDirectory);

            var directoryFiles = Directory.GetFiles(WorkingDirectory);
            foreach (var filePath in directoryFiles)
            {
                if (!PathHelpers.IsPackfilePath(filePath))
                    continue;

                //Try to find file in cache, add if not visible.
                string filename = Path.GetFileName(filePath);
                if (TryGetCacheFile(filename, null, out var target))
                {
                    //Found in cache, update FilePath and read format data
                    target.FilePath = filePath;
                    target.ReadFormatData();
                }
                else
                {
                    //Else, add to cache and read format data
                    var cacheFile = new CacheFile(filename, null, CachePath, filePath)
                    {
                        Depth = 0, FileType = RfgFileTypes.Packfile
                    };
                    cacheFile.ReadFormatData();
                    _cacheFiles.Add(cacheFile);
                }
            }

            //Sort alphabetically for easier file explorer navigation
            _cacheFiles.Sort((file1, file2) => string.Compare(file1.Filename, file2.Filename, StringComparison.Ordinal));
        }

        private void ScanEditorCache()
        {
            if (!Directory.Exists(CachePath))
                Directory.CreateDirectory(CachePath);

            var cacheFolders = Directory.GetDirectories(CachePath);
            //First check depth 0 packfiles cached children
            foreach (var cacheFolder in cacheFolders)
            {
                string parentFileName = Path.GetFileName(cacheFolder);
                var files = Directory.GetFiles(cacheFolder);
                if (IsEmbeddedPackfileKey(parentFileName))
                    continue;
                if(files.Length <= 0)
                    continue;

                var parentFile = new CacheFile(parentFileName, null, CachePath)
                {
                    FileType = RfgFileTypes.Packfile, Depth = 0
                };
                _cacheFiles.Add(parentFile);

                //Read folder sub-files, add to cache
                foreach (var filePath in files)
                {
                    var cacheFile = new CacheFile(Path.GetFileName(filePath), parentFile.Filename, CachePath, filePath)
                    {
                        FileType = PathHelpers.IsPackfilePath(filePath) ? RfgFileTypes.Container : RfgFileTypes.Primitive,
                        Depth = 1,
                        Parent = parentFile
                    };
                    _cacheFiles.Add(cacheFile);

                    if (cacheFile.CanHaveSubfiles()) 
                        cacheFile.ReadFormatData();
                }
            }

            //Then check depth 1 packfiles, have "--" in key. In other words IsEmbeddedPackfileKey() == true
            foreach (var cacheFolder in cacheFolders)
            {
                string embeddedFileKey = Path.GetFileName(cacheFolder);
                var files = Directory.GetFiles(cacheFolder);
                if (!IsEmbeddedPackfileKey(embeddedFileKey))
                    continue;
                if (!TrySplitEmbeddedPackfileKey(embeddedFileKey, out string parentName, out string childName))
                    continue;
                if (files.Length <= 0)
                    continue;

                //Try to find parent
                if(!TryGetCacheFile(childName, parentName, out CacheFile parentFile))
                    continue;

                string filePath = $"{CachePath}\\{parentName}\\{childName}";
                var cacheFile = new CacheFile(childName, parentName, CachePath, filePath)
                {
                    FileType = RfgFileTypes.Container, Depth = 1, Parent = parentFile
                };
                _cacheFiles.Add(cacheFile);

                //Read folder sub-files, add to cache
                foreach (var subFilePath in files)
                {
                    _cacheFiles.Add(new CacheFile(Path.GetFileName(subFilePath), cacheFile.Filename, CachePath, subFilePath)
                    {
                        FileType = RfgFileTypes.Primitive,
                        Depth = 2
                    });
                }
            }
        }

        public bool TryGetFile(string targetName, string parentName, out Stream stream)
        {
            stream = Stream.Null;
            //Try to get parent CacheFile
            if (!TryGetCacheFile(parentName, null, out CacheFile parent))
                return false;

            //Call main function
            return TryGetFile(targetName, parent, out stream);
        }

        public bool TryGetFile(string targetFilename, CacheFile parent, out Stream stream)
        {
            //Ensure parent is extracted
            if(parent.Depth > 0)
                ExtractFileIfNotCached(parent.Filename, parent.Parent);

            //Extract target
            ExtractFileIfNotCached(targetFilename, parent);
            stream = GetCachedFileStream(targetFilename, parent.Filename);

            return stream != Stream.Null;
        }

        /// <summary>
        /// Extracts a file from it's parent.
        /// Parent should already be extracted when this is called.
        /// </summary>
        /// <param name="targetFilename">The name of the target file to be extracted.</param>
        /// <param name="parent">The parent CacheFile.</param>
        /// <returns>Returns if the file was successfully extracted.</returns>
        private bool ExtractFileIfNotCached(string targetFilename, CacheFile parent)
        {
            //If file isn't cached, extract and return success value of that
            return IsFileCached(targetFilename, parent.Filename) || ExtractAndCacheFile(targetFilename, parent);
        }

        public bool IsFileCached(string targetName, string parentName = null)
        {
            foreach (var cacheFile in _cacheFiles)
            {
                if (!string.Equals(cacheFile.Filename, targetName, StringComparison.CurrentCultureIgnoreCase))
                    continue;
                if (!string.Equals(cacheFile.ParentName, parentName, StringComparison.CurrentCultureIgnoreCase))
                    continue;

                return true;
            }
            return false;
        }

        private Stream GetCachedFileStream(string targetName, string parentName)
        {
            foreach (var cacheFile in _cacheFiles)
            {
                if (!string.Equals(cacheFile.Filename, targetName, StringComparison.CurrentCultureIgnoreCase))
                    continue;
                if (!string.Equals(cacheFile.ParentName, parentName, StringComparison.CurrentCultureIgnoreCase))
                    continue;
                if (!cacheFile.TryOpenOrGet(out Stream fileStream))
                    break;

                return fileStream;
            }
            return Stream.Null;
        }

        public bool TryGetCacheFile(string targetName, string parentName, out CacheFile target, bool extractIfNotCached = false)
        {
            target = null;
            CacheFile parent = null;

            foreach (var cacheFile in _cacheFiles)
            {
                //Find parent in cases where no files with the target parent exist yet
                if(string.Equals(cacheFile.Filename, parentName, StringComparison.CurrentCultureIgnoreCase))
                    if (parent == null)
                        parent = cacheFile;
                //Match parent and target name to existing CacheFiles
                if (!string.Equals(cacheFile.ParentName, parentName, StringComparison.CurrentCultureIgnoreCase))
                    continue;
                if (!string.Equals(cacheFile.Filename, targetName, StringComparison.CurrentCultureIgnoreCase))
                    continue;

                //Set target and fix it's parent value if necessary
                target = cacheFile;
                if (target.Parent == null)
                    target.Parent = parent;

                return true;
            }

            if (extractIfNotCached && parent != null && ExtractAndCacheFile(targetName, parent))
                return TryGetCacheFile(targetName, parentName, out target);

            return false;
        }

        private bool ExtractAndCacheFile(string targetFilename, CacheFile parent)
        {
            var packfile = parent?.PackfileData;
            if (packfile == null)
                return false;

            CacheFile targetFile = null;
            //Form output paths
            string packfileOutputPath = $"{CachePath}{parent.Key}\\";
            string targetOutputPath = $"{packfileOutputPath}\\{targetFilename}";
            //Ensure output directory exists
            Directory.CreateDirectory(packfileOutputPath);

            //First try to extract single file
            if (packfile.TryExtractSingleFile(targetFilename, targetOutputPath))
            {
                targetFile = new CacheFile(targetFilename, parent.Filename, CachePath, targetOutputPath)
                {
                    FileType = PathHelpers.IsPackfilePath(targetOutputPath) ? RfgFileTypes.Container : RfgFileTypes.Primitive,
                    Parent = parent, Depth = parent.Depth + 1
                };
                _cacheFiles.Add(targetFile);
            }
            else //If that fails, extract all files from parent
            {
                WindowLogger.Log($"Failed to extract single file \"{targetFilename}\" from \"{parent.FilePath}\". Extracting entire packfile.");
                packfile.ExtractFileData(packfileOutputPath);
                foreach (var subfile in packfile.DirectoryEntries)
                {
                    var cacheFile = new CacheFile(subfile.FileName, parent.Filename, CachePath, $"{packfileOutputPath}\\{subfile.FileName}")
                    {
                        FileType = PathHelpers.IsPackfilePath(targetOutputPath) ? RfgFileTypes.Container : RfgFileTypes.Primitive,
                        Parent = parent, Depth = parent.Depth + 1
                    };
                    _cacheFiles.Add(cacheFile);

                    if (cacheFile.Filename == targetFilename)
                        targetFile = cacheFile;
                }
            }

            if (targetFile == null)
                return false;
            if (targetFile.CanHaveSubfiles())
            {
                Directory.CreateDirectory($"{CachePath}{targetFile.Key}\\");
                targetFile.ReadFormatData();
            }
            return true;
        }

        private bool IsEmbeddedPackfileKey(string key)
        {
            return key.Contains("--");
        }

        private bool TrySplitEmbeddedPackfileKey(string key, out string parentName, out string childName)
        {
            parentName = null;
            childName = null;

            if (!IsEmbeddedPackfileKey(key))
                return false;

            var result = key.Split("--");
            if (result.Length > 2)
                return false;

            parentName = result[0];
            childName = result[1];
            return true;
        }

        public CacheFile CopyToCache(CacheFile target)
        {
            //Make CacheFile copy
            var targetCopy = new CacheFile(target.Filename, target.ParentName, CachePath)
            {
                Depth = target.Depth
            };
            Directory.CreateDirectory($"{CachePath}\\{target.Key}\\");

            //For depth 0 files assume they have no parent, don't copy file, just make directory
            if (target.Depth != 0)
            {
                if (target.Parent != null)
                {
                    targetCopy.Parent = CopyToCache(target.Parent);
                    Directory.CreateDirectory($"{CachePath}\\{target.Parent.Key}\\");
                }

                File.Copy(target.FilePath, targetCopy.FilePath);
            }

            //Add copy to file list
            _cacheFiles.Add(targetCopy);
            return targetCopy;
        }
    }
}
