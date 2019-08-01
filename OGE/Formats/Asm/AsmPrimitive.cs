using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OGE.Util;

namespace OGE.Formats.Asm
{
    public class AsmPrimitive
    {
        public string Name { get; set; }
        public PrimitiveType Type { get; set; }
        public AllocatorType Allocator { get; set; }
        public byte Flags { get; set; } //Todo: Find the values of these flags
        public byte SplitExtIndex { get; set; } //Note: I don't know what exactly this value is. It's name was found via decompilation
        public uint HeaderSize { get; set; }
        public uint DataSize { get; set; }

        public AsmPrimitive()
        {

        }

        public void Read(BinaryReader stream)
        {
            uint nameLength = stream.ReadUInt16();
            Name = stream.ReadFixedLengthString(nameLength);
            Type = (PrimitiveType)stream.ReadByte();
            Allocator = (AllocatorType)stream.ReadByte();
            Flags = stream.ReadByte();
            SplitExtIndex = stream.ReadByte();
            HeaderSize = stream.ReadUInt32();
            DataSize = stream.ReadUInt32();
        }

        public void Write(BinaryWriter stream)
        {

        }
    }
}
