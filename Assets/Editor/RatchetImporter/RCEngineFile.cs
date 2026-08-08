using System;
using System.Collections.Generic;
using System.IO;

namespace RatchetImport
{
    public enum RCGame
    {
        Unknown = 0,
        RaC1 = 1,
        RaC2 = 2,
        RaC3 = 3,
        Deadlocked = 4
    }

    /// <summary>One entry of the level texture table. Pixel data lives in vram.ps3.</summary>
    public sealed class RCTextureHeader
    {
        public int index;
        public int vramPointer;
        public byte mipMapCount;
        public byte compressionFormat; // 0x86 = BC1/DXT1, 0x87 = BC2/DXT3, 0x88 = BC3/DXT5
        public short width;
        public short height;

        /// <summary>Byte length of this texture's slice of vram.ps3 (includes mip chain).</summary>
        public int vramLength;
    }

    /// <summary>One slot of the level's moby model table.</summary>
    public sealed class RCMobyEntry
    {
        public short modelId;
        public int offset;

        public bool HasModel
        {
            get { return offset != 0; }
        }
    }

    /// <summary>
    /// Reads a level's engine.ps3 container: the moby (character/prop) model table
    /// and the level texture table.
    ///
    /// engine.ps3 opens with a table of section pointers. Only the fields this
    /// importer needs are decoded:
    ///   0x00  moby model table
    ///   0x54  texture table
    ///   0x58  texture count
    ///   0xA0  game magic
    /// </summary>
    public sealed class RCEngineFile
    {
        public const int TextureHeaderSize = 0x24;

        public string enginePath;
        public string vramPath;
        public byte[] data;
        public RCGame game;

        /// <summary>Raw header magic, kept so unrecognised builds can be reported.</summary>
        public uint magic;

        public int mobyModelPointer;
        public int texturePointer;
        public int textureCount;

        public readonly List<RCMobyEntry> mobyEntries = new List<RCMobyEntry>();
        public readonly List<RCTextureHeader> textures = new List<RCTextureHeader>();

        public static RCEngineFile Load(string enginePath)
        {
            if (!File.Exists(enginePath))
                throw new FileNotFoundException("engine file not found", enginePath);

            var engine = new RCEngineFile();
            engine.enginePath = enginePath;
            engine.data = File.ReadAllBytes(enginePath);

            if (engine.data.Length < 0xA4)
                throw new InvalidDataException("File is too small to be an engine container.");

            engine.magic = RCBinary.U32(engine.data, 0xA0);
            engine.game = DetectGame(engine.data);
            engine.mobyModelPointer = RCBinary.I32(engine.data, 0x00);
            engine.texturePointer = RCBinary.I32(engine.data, 0x54);
            engine.textureCount = RCBinary.I32(engine.data, 0x58);

            // vram.ps3 sits next to engine.ps3 in the same level folder.
            string dir = Path.GetDirectoryName(enginePath);
            string candidate = dir == null ? null : Path.Combine(dir, "vram.ps3");
            engine.vramPath = (candidate != null && File.Exists(candidate)) ? candidate : null;

            engine.ReadMobyTable();
            engine.ReadTextureTable();
            return engine;
        }

        private static RCGame DetectGame(byte[] data)
        {
            uint magic = RCBinary.U32(data, 0xA0);
            switch (magic)
            {
                case 0x00000001: return RCGame.RaC1;
                case 0xEAA90001: return RCGame.RaC2;
                case 0xEAA60001: return RCGame.RaC3;
                case 0x008E008D: return RCGame.Deadlocked;
                default: return RCGame.Unknown;
            }
        }

        /// <summary>
        /// Moby table: an int32 count followed by count 8-byte slots
        /// (int16 padding, int16 model id, int32 offset into engine.ps3).
        /// </summary>
        private void ReadMobyTable()
        {
            if (!RCBinary.InRange(data, mobyModelPointer, 4))
                throw new InvalidDataException("Moby model table pointer is out of range.");

            int count = RCBinary.I32(data, mobyModelPointer);
            if (count < 0 || !RCBinary.InRange(data, mobyModelPointer + 4, count * 0x08))
                throw new InvalidDataException("Moby model table is out of range (count " + count + ").");

            for (int i = 0; i < count; i++)
            {
                int slot = mobyModelPointer + 4 + i * 0x08;
                mobyEntries.Add(new RCMobyEntry
                {
                    modelId = RCBinary.I16(data, slot + 0x02),
                    offset = RCBinary.I32(data, slot + 0x04)
                });
            }
        }

        private void ReadTextureTable()
        {
            if (textureCount <= 0)
                return;

            if (!RCBinary.InRange(data, texturePointer, textureCount * TextureHeaderSize))
                return;

            for (int i = 0; i < textureCount; i++)
            {
                int o = texturePointer + i * TextureHeaderSize;
                textures.Add(new RCTextureHeader
                {
                    index = i,
                    vramPointer = RCBinary.I32(data, o + 0x00),
                    mipMapCount = data[o + 0x05],
                    compressionFormat = data[o + 0x06],
                    width = RCBinary.I16(data, o + 0x18),
                    height = RCBinary.I16(data, o + 0x1A)
                });
            }

            // A texture's payload runs until the next texture's vram offset;
            // the last one runs to the end of vram.ps3.
            long vramLength = (vramPath != null) ? new FileInfo(vramPath).Length : 0;
            for (int i = 0; i < textures.Count; i++)
            {
                long end = (i < textures.Count - 1) ? textures[i + 1].vramPointer : vramLength;
                textures[i].vramLength = (int) Math.Max(0, end - textures[i].vramPointer);
            }
        }

        public RCMobyModel ReadMoby(RCMobyEntry entry)
        {
            return RCMobyModel.Parse(data, entry.offset, entry.modelId);
        }
    }
}
