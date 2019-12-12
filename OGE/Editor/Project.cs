using System.Collections.Generic;
using OGE.Editor.Interfaces;
using OGE.Editor.Managers;

namespace OGE.Editor
{
    public class Project
    {
        private CacheManager _cache;

        public string Name { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }

        public string ProjectFolderPath { get; private set; }
        public Dictionary<CacheFile, List<ITrackedAction>> Changes { get; }

        public Project(string projectFolderPath)
        {
            ProjectFolderPath = projectFolderPath;
            _cache = new CacheManager($"{ProjectFolderPath}\\Cache\\");
            Changes = new Dictionary<CacheFile, List<ITrackedAction>>();
        }

        public void GenerateModinfoFromChanges(string outputPath)
        {

        }

        public void Save(string outputPath = null)
        {

        }

        public void Load(string inputPath)
        {

        }

        public void AddFileEdit(CacheFile targetFile, ITrackedAction editAction)
        {

        }

        public void RemoveFileEdit(CacheFile targetFile, ITrackedAction editAction)
        {

        }

        public void ResetFile(CacheFile targetFile)
        {

        }
    }
}
