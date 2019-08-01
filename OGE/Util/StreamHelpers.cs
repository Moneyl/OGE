using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGE.Util
{
    public static class StreamHelpers
    {
        public static string ReadNullTerminatedString(this BinaryReader stream)
        {
            var String = new StringBuilder();
            do
            {
                String.Append(stream.ReadChar()); //Since the character isn't a null byte, add it to the string
            }
            while (stream.PeekChar() != 0); //Read bytes until a null byte (string terminator) is reached
            
            stream.ReadByte(); //Read past the null terminator
            return String.ToString();
        }

        public static string ReadFixedLengthString(this BinaryReader stream, uint stringLength)
        {
            var String = new StringBuilder();
            for (int i = 0; i < stringLength; i++)
            {
                String.Append(stream.ReadChar());
            }

            return String.ToString();
        }

        public static void WriteNullTerminatedString(this BinaryWriter stream, string output)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(output);
            stream.Write(bytes, 0, bytes.Length);
            stream.Write((byte)0);
        }
    }
}
