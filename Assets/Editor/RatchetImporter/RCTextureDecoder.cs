using System;
using System.IO;
using UnityEngine;

namespace RatchetImport
{
    /// <summary>
    /// Pulls texture payloads out of vram.ps3 and decodes them to straight RGBA32.
    ///
    /// The payloads are ordinary block-compressed data (BC1/BC2/BC3) in little-endian
    /// layout, unlike the surrounding big-endian containers. Only mip 0 is decoded;
    /// Unity regenerates the rest on import.
    /// </summary>
    internal static class RCTextureDecoder
    {
        public const byte FormatBC1 = 0x86;
        public const byte FormatBC2 = 0x87;
        public const byte FormatBC3 = 0x88;

        /// <summary>Reads one texture's slice of vram.ps3.</summary>
        public static byte[] ReadPayload(Stream vram, RCTextureHeader header)
        {
            if (header.vramLength <= 0 || header.vramPointer < 0 || header.vramPointer >= vram.Length)
                return null;

            int length = (int) Math.Min(header.vramLength, vram.Length - header.vramPointer);
            var buffer = new byte[length];

            vram.Seek(header.vramPointer, SeekOrigin.Begin);
            int read = 0;
            while (read < length)
            {
                int n = vram.Read(buffer, read, length - read);
                if (n <= 0)
                    break;
                read += n;
            }

            return read == length ? buffer : null;
        }

        public static bool IsSupported(byte compressionFormat)
        {
            return compressionFormat == FormatBC1 || compressionFormat == FormatBC2 || compressionFormat == FormatBC3;
        }

        /// <summary>
        /// Decodes mip 0 into a Color32 array in Unity's bottom-up row order.
        /// Returns null when the payload is too short or the format is unknown.
        /// </summary>
        public static Color32[] Decode(byte[] payload, int width, int height, byte compressionFormat)
        {
            if (payload == null || width <= 0 || height <= 0)
                return null;

            int blockBytes = (compressionFormat == FormatBC1) ? 8 : 16;
            int blocksX = (width + 3) / 4;
            int blocksY = (height + 3) / 4;

            if (payload.Length < blocksX * blocksY * blockBytes)
                return null;

            var pixels = new Color32[width * height];
            var block = new Color32[16];

            for (int by = 0; by < blocksY; by++)
            {
                for (int bx = 0; bx < blocksX; bx++)
                {
                    int o = (by * blocksX + bx) * blockBytes;

                    switch (compressionFormat)
                    {
                        case FormatBC1:
                            DecodeColorBlock(payload, o, block, true);
                            break;
                        case FormatBC2:
                            DecodeColorBlock(payload, o + 8, block, false);
                            ApplyBC2Alpha(payload, o, block);
                            break;
                        case FormatBC3:
                            DecodeColorBlock(payload, o + 8, block, false);
                            ApplyBC3Alpha(payload, o, block);
                            break;
                        default:
                            return null;
                    }

                    for (int py = 0; py < 4; py++)
                    {
                        int y = by * 4 + py;
                        if (y >= height)
                            break;

                        // Source row 0 is the top of the image; Unity's array starts at the bottom.
                        int row = (height - 1 - y) * width;

                        for (int px = 0; px < 4; px++)
                        {
                            int x = bx * 4 + px;
                            if (x >= width)
                                break;

                            pixels[row + x] = block[py * 4 + px];
                        }
                    }
                }
            }

            return pixels;
        }

        /// <summary>
        /// The 8-byte BC1 colour block shared by all three formats: two RGB565
        /// endpoints followed by sixteen 2-bit selectors.
        /// </summary>
        private static void DecodeColorBlock(byte[] data, int offset, Color32[] block, bool punchThroughAlpha)
        {
            int c0 = data[offset] | (data[offset + 1] << 8);
            int c1 = data[offset + 2] | (data[offset + 3] << 8);

            uint selectors = (uint) (data[offset + 4] | (data[offset + 5] << 8) | (data[offset + 6] << 16) | (data[offset + 7] << 24));

            Color32 p0 = FromRgb565(c0);
            Color32 p1 = FromRgb565(c1);

            Color32 p2, p3;

            // In BC1, endpoints in ascending order select the three-colour mode where
            // the fourth selector means "transparent". BC2/BC3 always use four colours.
            bool threeColorMode = punchThroughAlpha && c0 <= c1;

            if (threeColorMode)
            {
                p2 = new Color32((byte) ((p0.r + p1.r) / 2), (byte) ((p0.g + p1.g) / 2), (byte) ((p0.b + p1.b) / 2), 255);
                p3 = new Color32(0, 0, 0, 0);
            }
            else
            {
                p2 = new Color32((byte) ((2 * p0.r + p1.r) / 3), (byte) ((2 * p0.g + p1.g) / 3), (byte) ((2 * p0.b + p1.b) / 3), 255);
                p3 = new Color32((byte) ((p0.r + 2 * p1.r) / 3), (byte) ((p0.g + 2 * p1.g) / 3), (byte) ((p0.b + 2 * p1.b) / 3), 255);
            }

            for (int i = 0; i < 16; i++)
            {
                uint selector = (selectors >> (i * 2)) & 0x3;
                switch (selector)
                {
                    case 0: block[i] = p0; break;
                    case 1: block[i] = p1; break;
                    case 2: block[i] = p2; break;
                    default: block[i] = p3; break;
                }
            }
        }

        /// <summary>BC2: sixteen explicit 4-bit alpha values.</summary>
        private static void ApplyBC2Alpha(byte[] data, int offset, Color32[] block)
        {
            for (int i = 0; i < 16; i++)
            {
                int nibble = (data[offset + i / 2] >> ((i % 2) * 4)) & 0xF;
                block[i].a = (byte) (nibble * 17); // 0..15 -> 0..255
            }
        }

        /// <summary>BC3: two alpha endpoints plus sixteen 3-bit selectors.</summary>
        private static void ApplyBC3Alpha(byte[] data, int offset, Color32[] block)
        {
            int a0 = data[offset];
            int a1 = data[offset + 1];

            ulong selectors = 0;
            for (int i = 0; i < 6; i++)
                selectors |= (ulong) data[offset + 2 + i] << (i * 8);

            for (int i = 0; i < 16; i++)
            {
                int selector = (int) ((selectors >> (i * 3)) & 0x7);
                int a;

                if (selector == 0)
                {
                    a = a0;
                }
                else if (selector == 1)
                {
                    a = a1;
                }
                else if (a0 > a1)
                {
                    a = ((8 - selector) * a0 + (selector - 1) * a1) / 7;
                }
                else if (selector == 6)
                {
                    a = 0;
                }
                else if (selector == 7)
                {
                    a = 255;
                }
                else
                {
                    a = ((6 - selector) * a0 + (selector - 1) * a1) / 5;
                }

                block[i].a = (byte) a;
            }
        }

        private static Color32 FromRgb565(int color)
        {
            int r = (color >> 11) & 0x1F;
            int g = (color >> 5) & 0x3F;
            int b = color & 0x1F;

            // Replicate the high bits into the low ones so 0x1F maps to 0xFF.
            return new Color32(
                (byte) ((r << 3) | (r >> 2)),
                (byte) ((g << 2) | (g >> 4)),
                (byte) ((b << 3) | (b >> 2)),
                255);
        }
    }
}
