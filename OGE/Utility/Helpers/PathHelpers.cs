using System.Collections.Generic;
using System.IO;

namespace OGE.Utility.Helpers
{
    public static class PathHelpers
    {
        private static List<string> _packfileExtensions = new List<string>
            {".vpp_pc", ".str2_pc"};
        private static List<string> _xmlExtensions = new List<string>
            {".xml", ".xtbl", ".mtbl", ".gtodx", ".dtodx", ".vint_proj"};
        private static List<string> _textExtensions = new List<string>
        {
            ".xml", ".xtbl", ".mtbl", ".gtodx", ".dtodx", ".vint_proj",
            ".txt", ".lua"
        };
        private static List<string> _textureExtensions = new List<string>
        {
            ".cpeg_pc", ".cvbm_pc", ".gpeg_pc", ".gvbm_pc"
        };
        private static List<string> _textureHeaderExtensions = new List<string>
        {
            ".cpeg_pc", ".cvbm_pc", ".gpeg_pc", ".gvbm_pc"
        };
        private static List<string> _textureDataExtensions = new List<string>
        {
            ".cpeg_pc", ".cvbm_pc", ".gpeg_pc", ".gvbm_pc"
        };

        public static IReadOnlyList<string> PackfileExtensions => _packfileExtensions;
        public static IReadOnlyList<string> XmlExtensions => _xmlExtensions;
        public static IReadOnlyList<string> TextExtensions => _textExtensions;
        public static IReadOnlyList<string> TextureExtensions => _textureExtensions;
        public static IReadOnlyList<string> TextureHeaderExtensions => _textureHeaderExtensions;
        public static IReadOnlyList<string> TextureDataExtensions => _textureDataExtensions;


        public static bool IsPackfilePath(string packfilePath)
        {
            return IsPackfileExtension(Path.GetExtension(packfilePath));
        }
        public static bool IsPackfileExtension(string extension)
        {
            return _packfileExtensions.Contains(extension);
        }


        public static bool IsXmlPath(string filePath)
        {
            return IsXmlExtension(Path.GetExtension(filePath));
        }
        public static bool IsXmlExtension(string extension)
        {
            return _xmlExtensions.Contains(extension);
        }


        public static bool IsAsmPath(string filePath)
        {
            return IsAsmExtension(Path.GetExtension(filePath));
        }
        public static bool IsAsmExtension(string extension)
        {
            return extension == ".asm_pc";
        }


        public static bool IsTextPath(string filePath)
        {
            return IsTextExtension(Path.GetExtension(filePath));
        }
        public static bool IsTextExtension(string extension)
        {
            return _textExtensions.Contains(extension);
        }


        public static bool IsTexturePath(string filePath)
        {
            return IsTextureExtension(Path.GetExtension(filePath));
        }
        public static bool IsTextureExtension(string extension)
        {
            return _textureExtensions.Contains(extension);
        }


        public static bool IsTextureHeaderPath(string filePath)
        {
            return IsTextureHeaderExtension(Path.GetExtension(filePath));
        }
        public static bool IsTextureHeaderExtension(string extension)
        {
            return _textureHeaderExtensions.Contains(extension);
        }


        public static bool IsTextureDataPath(string filePath)
        {
            return IsTextureDataExtension(Path.GetExtension(filePath));
        }
        public static bool IsTextureDataExtension(string extension)
        {
            return _textureDataExtensions.Contains(extension);
        }


        public static bool TryGetCpuFileNameFromGpuFile(string gpuFileName, out string cpuFileName)
        {
            string extension = Path.GetExtension(gpuFileName);
            string name = Path.GetFileNameWithoutExtension(gpuFileName);
            switch (extension)
            {
                case ".gpeg_pc":
                    cpuFileName = $"{name}.cpeg_pc";
                    return true;
                case ".gvbm_pc":
                    cpuFileName = $"{name}.cvbm_pc";
                    return true;
                default:
                    cpuFileName = null;
                    return false;
            }
        }
        public static bool TryGetGpuFileNameFromCpuFile(string cpuFileName, out string gpuFileName)
        {
            string extension = Path.GetExtension(cpuFileName);
            string name = Path.GetFileNameWithoutExtension(cpuFileName);
            switch (extension)
            {
                case ".cpeg_pc":
                    gpuFileName = $"{name}.gpeg_pc";
                    return true;
                case ".cvbm_pc":
                    gpuFileName = $"{name}.gvbm_pc";
                    return true;
                default:
                    gpuFileName = null;
                    return false;
            }
        }
    }
}