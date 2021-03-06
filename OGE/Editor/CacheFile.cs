﻿using System;
using System.IO;
using OGE.Editor.Managers;
using OGE.Utility.Helpers;
using RfgTools.Formats.Asm;
using RfgTools.Formats.Packfiles;
using RfgTools.Formats.Textures;

namespace OGE.Editor
{
    public class CacheFile
    {
        private Stream _fileStream = Stream.Null;

        public string Filename { get; private set; }
        public string FilePath { get; set; }
        public string CachePath { get; set; }
        public string ParentName { get; private set; }
        public CacheFile Parent { get; set; } = null;
        public bool IsOpen { get; private set; } = false;
        public RfgFileTypes FileType { get; set; } = RfgFileTypes.None;
        public bool InEditorCache { get; set; } = false;
        public bool InProjectCache { get; set; } = false;
        public uint Depth { get; set; } = 0;

        //Todo: Figure out a less dumb way to store format data
        public Packfile PackfileData = null;
        public PegFile PegData = null;
        public AsmFile AsmData = null;

        public string Key
        {
            get
            {
                //Special key syntax for embedded packfiles
                if (Parent != null && Depth > 0 && CanHaveSubfiles())
                    return $"{Parent.Filename}--{Filename}";
                
                //Key for all other files is just their filename
                return Filename;
            }
        }

        public CacheFile(string filename, string parentName, string cachePath, string filePath = null)
        {
            Filename = filename;
            ParentName = parentName;
            CachePath = cachePath;
            FilePath = filePath ?? $"{CachePath}{ParentName}\\{Filename}";
            UpdateFileType();
        }

        public bool CanHaveSubfiles()
        {
            return FileType == RfgFileTypes.Packfile || FileType == RfgFileTypes.Container;
        }

        private void UpdateFileType()
        {
            string extension = Path.GetExtension(Filename);
            FileType = extension switch
            {
                ".vpp_pc" => RfgFileTypes.Packfile,
                ".str2_pc" => RfgFileTypes.Container,
                _ => RfgFileTypes.Primitive
            };
        }

        public void ReadFormatData()
        {
            switch (FileType)
            {
                case RfgFileTypes.None:
                    return;
                case RfgFileTypes.Packfile:
                    PackfileData = new Packfile(false);
                    PackfileData.ReadMetadata(FilePath);
                    PackfileData.DirectoryEntries.Sort((entry1, entry2) => string.Compare(entry1.FileName, entry2.FileName, StringComparison.Ordinal));
                    PackfileData.ParseAsmFiles($"{CachePath}{Filename}\\");
                    break;
                case RfgFileTypes.Container:
                    PackfileData = new Packfile(false);
                    PackfileData.ReadMetadata(FilePath);
                    break;
                case RfgFileTypes.Primitive:
                    string extension = Path.GetExtension(Filename);
                    if (extension == ".cpeg_pc" || extension == ".cvbm_pc")
                    {
                        if(!PathHelpers.TryGetGpuFileNameFromCpuFile(FilePath, out string gpuFileName))
                            return;

                        string basePath = Path.GetDirectoryName(FilePath);
                        string gpuFilePath = $"{basePath}\\{gpuFileName}";

                        //Todo: Change this to check editor or project cache depending on CacheFile location
                        //Ensure gpu file is extracted
                        if(!ProjectManager.TryGetCacheFile(gpuFileName, ParentName, out _, true))
                            return;

                        PegData = new PegFile();
                        PegData.Read(FilePath, gpuFilePath);
                    }
                    //Todo: Add checks for primitive type, and primitive handling code
                    break;
            }
        }

        public bool FileExists(string parentFolderPathOverride = null)
        {
            string filePath = $"{CachePath}\\{Key}\\{Filename}";
            return File.Exists(FilePath ?? filePath);
        }

        public bool TryOpenOrGet(out Stream stream)
        {
            if (IsOpen || TryOpenFile())
            {
                stream = _fileStream;
                return true;
            }

            stream = Stream.Null;
            return false;
        }

        private bool TryOpenFile()
        {
            _fileStream.Close();
            _fileStream = Stream.Null;
            string filePath = FilePath ?? $"{CachePath}\\{Key}\\{Filename}";

            //Check if file exists in EditorCache
            if (!File.Exists(filePath))
                return false;

            _fileStream = new FileStream(filePath, FileMode.Open);
            return true;
        }
    }
}
