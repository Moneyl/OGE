
using System.Collections.Generic;
using System.IO;

namespace OGE.Helpers
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

        public static IReadOnlyList<string> PackfileExtensions => _packfileExtensions;
        public static IReadOnlyList<string> XmlExtensions => _xmlExtensions;
        public static IReadOnlyList<string> TextExtensions => _textExtensions;

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
    }
}