using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using RfgTools.Helpers;

namespace OGE.Editor.Managers
{
    public static class SettingsManager
    {
        //Settings
        public static string DataFolderPath { get; private set; }
        public static bool TextureViewerDarkBackground = false;

        //Other values
        public static string SettingsFilePath { get; } = "./Settings.xml";

        public static void Init()
        {
            //See if settings file exists
            //If not, generate default one, ask user for any needed values
            //If it does, load it.

            if(!SettingsFileExists())
                GenerateDefaultSettingsFile();
            else
                LoadSettingsFile();

            if (!Directory.Exists(DataFolderPath))
            {
                RequestDataFolderPath();
                SaveSettingsFile();
            }
        }

        private static bool SettingsFileExists()
        {
            return File.Exists(SettingsFilePath);
        }

        private static void GenerateDefaultSettingsFile()
        {
            //Xml file representation in memory
            var xml = new XDocument();

            //Add root node. Required for xml
            var root = new XElement("root");
            xml.Add(root);
            root.Add(new XElement("Settings"));

            var settings = root.GetRequiredElement("Settings");
            settings.Add(new XElement("DataFolderPath", "")); 
            settings.Add(new XElement("UseTextureViewerDarkBackground", TextureViewerDarkBackground));

            using var settingsFileStream = new FileStream(SettingsFilePath, FileMode.Create);
            xml.Save(settingsFileStream);
        }

        private static void LoadSettingsFile()
        {
            using var settingsFileStream = new FileStream(SettingsFilePath, FileMode.Open);
            var document = XDocument.Load(settingsFileStream);

            var root = document.Root;
            if (root == null)
                throw new XmlException("Error! Settings file has no root node!");

            var settings = root.GetRequiredElement("Settings");
            DataFolderPath = settings.GetOptionalValue("DataFolderPath", "");
            TextureViewerDarkBackground = settings.GetOptionalValue(
                "UseTextureViewerDarkBackground", "false").ToBool();
        }

        private static void SaveSettingsFile()
        {
            //Xml file representation in memory
            var xml = new XDocument();

            //Add root node. Required for xml
            var root = new XElement("root");
            xml.Add(root);
            root.Add(new XElement("Settings"));

            var settings = root.GetRequiredElement("Settings");
            settings.Add(new XElement("DataFolderPath", DataFolderPath));
            settings.Add(new XElement("UseTextureViewerDarkBackground", TextureViewerDarkBackground));

            using var settingsFileStream = new FileStream(SettingsFilePath, FileMode.Create);
            xml.Save(settingsFileStream);
        }

        private static void RequestDataFolderPath()
        {
            MessageBox.Show("Please select your RFGR data folder. This folder contains files such " +
                            "as \"misc.vpp_pc\" and \"anims.vpp_pc\".", "Data folder location needed");
            var openFolderDialog = new FolderBrowserDialog();
            openFolderDialog.ShowDialog();
            DataFolderPath = openFolderDialog.SelectedPath;
        }
    }
}
