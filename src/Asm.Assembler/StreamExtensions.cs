using System.IO;

namespace Asm.Assembler
{    
    public static class StreamExtensions
    {
        public static void Emit8(this Stream s, byte v)
        {
            s.WriteByte(v);
        }

        public static void Emit16(this Stream s, short v)
        {
            s.WriteByte((byte)(v & 255));
            s.WriteByte((byte)((v >> 8) & 255));
        }

        public static void Emit16(this Stream s, int v)
        {
            s.WriteByte((byte)(v & 255));
            s.WriteByte((byte)((v >> 8) & 255));
        }
    }
}