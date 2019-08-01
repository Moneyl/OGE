using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGE.Formats.Asm
{
    public class AsmFile
    {
        public uint Signature { get; set; }
        public ushort Version { get; set; }
        public ushort ContainerCount { get; set; }

        public List<AsmContainer> Containers;

        //Values used internally by tools start here
        public string AsmPath { get; }
        public string AsmFileName { get; }

        public AsmFile(string asmPath)
        {
            AsmPath = asmPath;
            AsmFileName = Path.GetFileName(asmPath);

            Containers = new List<AsmContainer>();
        }

        public void Read()
        {
            using (var stream = new BinaryReader(new FileStream(AsmPath, FileMode.Open)))
            {
                Signature = stream.ReadUInt32();
                if (Signature != 3203399405)
                {
                    throw new Exception($"Invalid file signature detected in {AsmFileName}! Expected 3203399405, read {Signature}. This is usually a sign of file corruption. Make sure that your packfile extractor is not improperly extracting files.");
                }

                Version = stream.ReadUInt16();
                if (Version != 5)
                {
                    throw new Exception($"Unsupported asm_pc format version detected in {AsmFileName}! Expected version 5, found version {Version}.");
                }

                ContainerCount = stream.ReadUInt16();
                for (int i = 0; i < ContainerCount; i++)
                {
                    var container = new AsmContainer();
                    container.Read(stream);
                    Containers.Add(container);
                }
            }
        }

        public void Write()
        {
            using (var stream = new BinaryWriter(new FileStream(AsmPath, FileMode.Truncate)))
            {
                stream.Write(Signature);
                stream.Write(Version);
                stream.Write(ContainerCount);

                foreach (var container in Containers)
                {
                    container.Write(stream);
                }
            }
        }
    }
}
