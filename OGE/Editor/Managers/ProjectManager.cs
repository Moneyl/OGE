using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using OGE.Editor.Interfaces;

namespace OGE.Editor.Managers
{
    //Todo: Keep undo/redo stack and track changes
    //Todo: Generate modinfo.xml from changes
    /// <summary>
    /// Class for managing projects and providing way to get files from cache without worrying about,
    /// if they're in the global cache or project cache.
    /// </summary>
    /// <remarks>API is largely up in the air and experimental as the needs of the caching system aren't fully know yet</remarks>
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

        //Todo: Update these TryGet functions to also support project cache
        public static bool TryGetCacheFile(string targetName, string parentName, out CacheFile target, bool extractIfNotCached = false)
        {
            return CurrentProject.TryGetCacheFile(targetName, parentName, out target, false) 
                   || _cache.TryGetCacheFile(targetName, parentName, out target, extractIfNotCached);
        }

        public static bool TryGetFile(string targetFilename, string parentName, out Stream stream)
        {
            return CurrentProject.TryGetFile(targetFilename, parentName, out stream)
                   || _cache.TryGetFile(targetFilename, parentName, out stream);
        }

        public static bool IsFileCached(string targetName, string parentName)
        {
            return CurrentProject.IsFileCached(targetName, parentName)
                   || _cache.IsFileCached(targetName, parentName);
        }

        public static void OpenProject(string projectFilePath)
        {
            CurrentProject = new Project($"{Path.GetDirectoryName(projectFilePath)}\\");
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

        public static bool CopyFileToProjectCache(string targetName, string parentName, out CacheFile projectCacheFile)
        {
            projectCacheFile = null;
            //Get parent and call main overload
            return _cache.TryGetCacheFile(targetName, parentName, out CacheFile parent, true) 
                   && CopyFileToProjectCache(targetName, parent, out projectCacheFile);
        }

        public static bool CopyFileToProjectCache(string targetName, CacheFile parent, out CacheFile projectCacheFile)
        {
            projectCacheFile = null;
            //Confirm file exists in global cache, extract if not. If fail to get from global cache, return false
            if (CurrentProject == null || !_cache.TryGetCacheFile(targetName, parent.Filename, out var targetFile, true))
                return false;

            //If file is in project cache, get it, if not, copy to project cache and get copy
            if(!CurrentProject.TryGetCacheFile(targetName, parent.Filename, out projectCacheFile))
                projectCacheFile = CurrentProject.CopyToCache(targetFile);
            
            return true;
        }

        public static void AddChange(CacheFile editedFile, ITrackedAction change)
        {

        }
    }
}