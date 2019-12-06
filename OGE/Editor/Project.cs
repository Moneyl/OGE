using System.Collections.Generic;
using OGE.Editor.Interfaces;
using OGE.Editor.Managers;

namespace OGE.Editor
{
    public class Project
    {
        private CacheManager _cache;

        public string Name { get; private set; }
        public string Author { get; private set; }
        public string Version { get; private set; }
        public string Description { get; private set; }

        public string ProjectFolderPath { get; private set; }
        public Dictionary<CacheFile, List<IReversibleAction>> Changes { get; }

        public Project(string projectFolderPath, string projectName)
        {
            ProjectFolderPath = projectFolderPath;
            Name = projectName;
            _cache = new CacheManager($"{ProjectFolderPath}\\Cache\\");
            Changes = new Dictionary<CacheFile, List<IReversibleAction>>();
        }

        public void GenerateModinfoFromChanges(string outputPath)
        {

        }

        public void Save(string outputPath)
        {

        }

        public void Load(string inputPath)
        {

        }

        public void AddFileEdit(CacheFile targetFile, IReversibleAction editAction)
        {

        }

        public void RemoveFileEdit(CacheFile targetFile, IReversibleAction editAction)
        {

        }

        public void ResetFile(CacheFile targetFile)
        {

        }
    }
}
