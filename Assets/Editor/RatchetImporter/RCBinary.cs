using System.Runtime.InteropServices;

namespace RatchetImport
{
    /// <summary>
    /// Big-endian primitive readers.
    ///
    /// The HD-collection asset files (engine.ps3 / vram.ps3) were authored for a
    /// PowerPC target, so every multi-byte field is stored most significant byte
    /// first. x86 is little-endian, hence the manual assembly below.
    /// </summary>
    internal static class RCBinary
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct FloatBits
        {
            [FieldOffset(0)] public int i;
            [FieldOffset(0)] public float f;
        }

        public static int I32(byte[] b, int o)
        {
            return (b[o] << 24) | (b[o + 1] << 16) | (b[o + 2] << 8) | b[o + 3];
        }

        public static uint U32(byte[] b, int o)
        {
            return unchecked((uint) I32(b, o));
        }

        public static short I16(byte[] b, int o)
        {
            return (short) ((b[o] << 8) | b[o + 1]);
        }

        public static ushort U16(byte[] b, int o)
        {
            return (ushort) ((b[o] << 8) | b[o + 1]);
        }

        public static float F32(byte[] b, int o)
        {
            FloatBits bits = default;
            bits.i = I32(b, o);
            return bits.f;
        }

        /// <summary>True when [offset, offset + length) lies inside the buffer.</summary>
        public static bool InRange(byte[] b, int offset, int length)
        {
            return offset >= 0 && length >= 0 && (long) offset + length <= b.Length;
        }
    }
}
