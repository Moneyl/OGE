using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
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
                var editedFile = new XElement("File", 
                    new XAttribute("Name", cacheFile.Key.Filename),
                    new XAttribute("ParentName", cacheFile.Key.ParentName));

                foreach (var change in cacheFile.Value)
                {
                    var changeNode = new XElement("Change", new XAttribute("Type", change.GetType().ToString()));
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
            using var settingsFileStream = new FileStream(inputPath, FileMode.Open);
            var document = XDocument.Load(settingsFileStream);

            var root = document.Root;
            if (root == null)
                throw new XmlException("Error! Project file has no root node!");

            var projectElement = root.GetRequiredElement("Project");
            Author = projectElement.GetRequiredValue("Author");
            Version = projectElement.GetRequiredValue("Version");
            Description = projectElement.GetRequiredValue("Description");

            //Todo: Load changes and validate cache
            var changesNode = projectElement.GetRequiredElement("Changes");
            foreach (var file in changesNode.Elements("File"))
            {
                //Todo: Check if file is present in project cache and validate that cache and oge_proj match
                string filename = file.GetRequiredAttributeValue("Name");
                string parentName = file.GetRequiredAttributeValue("ParentName");
                if (string.IsNullOrEmpty(parentName))
                    parentName = null;
                if (!_cache.TryGetCacheFile(filename, parentName, out CacheFile changeTarget))
                    continue;

                foreach (var change in file.Elements("Change"))
                {
                    string typeString = change.GetRequiredAttributeValue("Type");
                    if(!TryCreateTrackedAction(typeString, out ITrackedAction action))
                        continue;

                    action.ReadFromProjectFile(change);
                    Changes[changeTarget].Add(action);
                }
            }
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

        private bool TryCreateTrackedAction(string typeString, out ITrackedAction action)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            action = null;

            foreach (Type type in assembly.GetTypes())
            {
                if (!typeof(ITrackedAction).IsAssignableFrom(type)) 
                    continue;
                if (type.ToString() != typeString) 
                    continue;

                action = Activator.CreateInstance(type) as ITrackedAction;
                return true;
            }
            return false;
        }
    }
}
