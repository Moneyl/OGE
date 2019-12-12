using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using OGE.Editor.Interfaces;
using OGE.Editor.Managers;
using RfgTools.Helpers;

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
            //Xml file representation in memory
            var xml = new XDocument();

            //Add root node. Required for xml
            var root = new XElement("root");
            xml.Add(root);
            var projectElement = new XElement("Project", new XAttribute("Name", Name));
            root.Add(projectElement);

            projectElement.Add(new XElement("Author", Author));
            projectElement.Add(new XElement("Version", Version));
            projectElement.Add(new XElement("Description", Description));

            var changesElement = new XElement("Changes");
            foreach (var cacheFile in Changes)
            {
                //Todo: Write node for each file and write CacheFile name/info
                var editedFile = new XElement("File", new XAttribute("Name", cacheFile.Key.Filename));
                foreach (var change in cacheFile.Value)
                {
                    var changeNode = new XElement("Change");
                    change.WriteToProjectFile(changeNode);
                    editedFile.Add(changeNode);
                }
                changesElement.Add(editedFile);
            }
            root.Add(changesElement);

            using var settingsFileStream = new FileStream(outputPath ?? $"{ProjectFolderPath}\\{Name}.oge_proj", FileMode.Create);
            xml.Save(settingsFileStream);
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
