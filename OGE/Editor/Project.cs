using OGE.Editor.Managers;

namespace OGE.Editor
{
    public class Project
    {
        private CacheManager _cache;

        public string ProjectFolderPath { get; private set; }
        public string Name { get; private set; }
        //List of edits
        //List of edited files
        //Settings
        //Metadata - Version, Author, Name, etc

        //Funcs:
            //Add edit
            //Remove edit
            //Add/Remove edited file
            //Save
            //Load
            //Generate modinfo from edits

        public Project(string projectFolderPath, string projectName)
        {
            ProjectFolderPath = projectFolderPath;
            Name = projectName;
            _cache = new CacheManager($"{ProjectFolderPath}\\Cache\\");
        }
    }
}
