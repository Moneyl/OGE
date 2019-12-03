using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OGE.Helpers;
using OGE.Utility;
using OGE.ViewModels.FileExplorer;
using RfgTools.Formats.Packfiles;

namespace OGE.Editor.Managers
{
    //Note: This class is a horrid mess and you shouldn't read it until I rewrite it
    //Todo: Keep undo/redo stack and track changes
    //Todo: Generate modinfo.xml from changes
    public static class ProjectManager
    {
        private static CacheManager _cache;
        public static string GlobalCachePath { get; } = @".\EditorCache\";
        public static IReadOnlyList<Packfile> WorkingDirectoryPackfiles => _cache.WorkingDirectoryPackfiles;
        public static string WorkingDirectory
        {
            get => _cache.WorkingDirectory;
            set => _cache.WorkingDirectory = value;
        }

        static ProjectManager()
        {

        }

        public static void Init()
        {
            _cache = new CacheManager(GlobalCachePath);
        }

        public static bool TryGetFile(string targetFilename, FileExplorerItemViewModel parent, out Stream stream, bool extractIfNotCached = false)
        {
            return _cache.TryGetFile(targetFilename, parent, out stream, extractIfNotCached);
        }

        public static bool IsFileCached(string filename, string parentKey, string parentFolderPathOverride = null)
        {
            return _cache.IsFileCached(filename, parentKey, parentFolderPathOverride);
        }
    }
}