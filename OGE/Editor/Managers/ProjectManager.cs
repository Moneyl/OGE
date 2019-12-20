using System.Collections.Generic;
using System.IO;

namespace OGE.Editor.Managers
{
    //Todo: Keep undo/redo stack and track changes
    //Todo: Generate modinfo.xml from changes
    public static class ProjectManager
    {
        private static CacheManager _cache;

        public static IReadOnlyList<CacheFile> EditorCacheFiles => _cache.CacheFiles;
        public static string GlobalCachePath { get; } = @".\EditorCache\";
        public static string WorkingDirectory
        {
            get => _cache.WorkingDirectory;
            set => _cache.WorkingDirectory = value;
        }
        public static Project CurrentProject { get; private set; }

        static ProjectManager()
        {

        }

        public static void Init(string initialWorkingDirectory)
        {
            _cache = new CacheManager(GlobalCachePath);
            WorkingDirectory = initialWorkingDirectory;
        }

        public static bool TryGetCacheFile(string targetName, string parentName, out CacheFile target, bool extractIfNotCached = false)
        {
            return _cache.TryGetCacheFile(targetName, parentName, out target, extractIfNotCached);
        }

        public static bool TryGetFile(string targetFilename, string parentName, out Stream stream)
        {
            return _cache.TryGetFile(targetFilename, parentName, out stream);
        }

        public static bool TryGetFile(string targetFilename, CacheFile parent, out Stream stream)
        {
            return _cache.TryGetFile(targetFilename, parent, out stream);
        }

        public static bool IsFileCached(string targetName, string parentName)
        {
            return _cache.IsFileCached(targetName, parentName);
        }

        public static void OpenProject(string projectFilePath)
        {
            CurrentProject = new Project(projectFilePath);
            CurrentProject.Load(projectFilePath);
        }

        public static void CloseCurrentProject()
        {
            //Todo: Ask if user wants to save unsaved changes
            CurrentProject.Save();
        }

        public static void SaveCurrentProject()
        {
            CurrentProject.Save();
        }

        public static void CreateNewProject(string projectFolderPath, string projectName, string author = null, string description = null, string version = null)
        {
            CurrentProject = new Project(projectFolderPath)
            {
                Name = projectName,
                Author = author,
                Description = description,
                Version = version
            };
        }

        public static bool CopyFileToProjectCache(string targetName, string parentName)
        {
            //Get parent and call main overload
            return _cache.TryGetCacheFile(targetName, parentName, out CacheFile parent, true) 
                   && CopyFileToProjectCache(targetName, parent);
        }

        public static bool CopyFileToProjectCache(string targetName, CacheFile parent)
        {
            //Check if in editor cache and check if there's a valid project currently loaded
            //If so, check if already in project cache
                    //If so, exit
                    //If not, copy to project cache
                //Else, exit
                
            return false;
        }
    }
}