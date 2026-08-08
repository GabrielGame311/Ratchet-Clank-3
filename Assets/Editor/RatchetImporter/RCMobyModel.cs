using System;
using System.Collections.Generic;
using UnityEngine;

namespace RatchetImport
{
    /// <summary>A run of indices drawn with one texture. Maps 1:1 to a Unity submesh.</summary>
    public sealed class RCTextureConfig
    {
        public int textureId;
        public int start; // first index
        public int size;  // index count (always a multiple of 3)
        public int mode;
    }

    public sealed class RCBone
    {
        public int parent;

        /// <summary>Inverse bind matrix in game space (Z-up), translation in game units.</summary>
        public Matrix4x4 inverseBind;

        /// <summary>Rest translation relative to the parent bone, in game units.</summary>
        public Vector3 localTranslation;
    }

    /// <summary>
    /// One keyframe. Rotations are stored for every bone; translations and scales
    /// are sparse and fall back to the bone's rest values when absent.
    /// </summary>
    public sealed class RCFrame
    {
        public float speed;
        public Quaternion[] rotations;
        public Vector3[] translations;
        public bool[] hasTranslation;
        public Vector3[] scales;
        public bool[] hasScale;
    }

    public sealed class RCAnimation
    {
        /// <summary>Playback rate; when zero, each frame carries its own.</summary>
        public float speed;

        public readonly List<RCFrame> frames = new List<RCFrame>();
    }

    /// <summary>
    /// A moby: the game's term for a dynamic, usually animated object
    /// (characters, enemies, weapons, crates, pickups...).
    ///
    /// All geometry stays in the game's native coordinate system (Z-up,
    /// right-handed) and native units. Conversion to Unity happens in
    /// <see cref="RCMobyBuilder"/> so the parser stays a faithful view of the file.
    /// </summary>
    public sealed class RCMobyModel
    {
        private const int HeaderSize = 0x48;
        private const int MeshHeaderSize = 0x20;
        private const int VertexSize = 0x28;
        private const int MetalVertexSize = 0x20;
        private const int TextureConfigSize = 0x10;

        public short id;
        public float size = 1f;
        public int boneCount;

        public Vector3[] positions = new Vector3[0];
        public Vector3[] normals = new Vector3[0];
        public Vector2[] uvs = new Vector2[0];

        /// <summary>Four bone indices per vertex, laid out consecutively.</summary>
        public byte[] boneIndices = new byte[0];

        /// <summary>Four raw 0-255 weights per vertex, matching <see cref="boneIndices"/>.</summary>
        public byte[] boneWeights = new byte[0];

        public ushort[] indices = new ushort[0];
        public readonly List<RCTextureConfig> textureConfigs = new List<RCTextureConfig>();

        // The "metal" pass: chrome-shaded surfaces the game renders with an
        // environment map instead of a diffuse texture. Separate vertices (no UVs),
        // separate indices, indexed from zero against its own vertex buffer.
        public Vector3[] metalPositions = new Vector3[0];
        public Vector3[] metalNormals = new Vector3[0];
        public byte[] metalBoneIndices = new byte[0];
        public byte[] metalBoneWeights = new byte[0];
        public ushort[] metalIndices = new ushort[0];
        public readonly List<RCTextureConfig> metalTextureConfigs = new List<RCTextureConfig>();

        public int MetalVertexCount { get { return metalPositions.Length; } }

        public RCBone[] bones = new RCBone[0];
        public readonly List<RCAnimation> animations = new List<RCAnimation>();

        /// <summary>Problems found while parsing; surfaced in the importer UI.</summary>
        public readonly List<string> warnings = new List<string>();

        public int VertexCount { get { return positions.Length; } }
        public int TriangleCount { get { return indices.Length / 3; } }
        public bool HasSkeleton { get { return boneCount > 0 && bones.Length == boneCount; } }

        /// <summary>
        /// Model header (0x48 bytes, all pointers relative to <paramref name="offset"/>):
        ///   0x00 mesh header      0x08 bone count       0x09 low-poly bone count
        ///   0x0C animation count  0x0D sound count      0x10 collision
        ///   0x14 bone matrices    0x18 bone data        0x1C attachments
        ///   0x24 size (float)     0x28 sounds           0x2C bangles (units of 0x10)
        /// followed by one int32 animation offset per animation.
        /// </summary>
        public static RCMobyModel Parse(byte[] data, int offset, short modelId)
        {
            var model = new RCMobyModel();
            model.id = modelId;

            if (offset == 0)
                return model;

            if (!RCBinary.InRange(data, offset, HeaderSize))
                throw new ArgumentOutOfRangeException("offset", "Moby header at 0x" + offset.ToString("X") + " is outside the file.");

            int meshPointer = RCBinary.I32(data, offset + 0x00);
            model.boneCount = data[offset + 0x08];
            int lowPolyBoneCount = data[offset + 0x09];
            if (model.boneCount == 0)
                model.boneCount = lowPolyBoneCount;

            int animationCount = data[offset + 0x0C];
            int boneMatrixPointer = RCBinary.I32(data, offset + 0x14);
            int boneDataPointer = RCBinary.I32(data, offset + 0x18);
            model.size = RCBinary.F32(data, offset + 0x24);

            if (model.size <= 0f || float.IsNaN(model.size))
                model.size = 1f;

            if (meshPointer > 0)
                model.ReadMesh(data, offset, offset + meshPointer);

            if (model.boneCount > 0 && boneMatrixPointer > 0 && boneDataPointer > 0)
                model.ReadSkeleton(data, offset, boneMatrixPointer, boneDataPointer);
            else
                model.boneCount = 0;

            if (animationCount > 0 && model.boneCount > 0)
                model.ReadAnimations(data, offset, animationCount);

            return model;
        }

        /// <summary>
        /// Mesh header (0x20 bytes):
        ///   0x00 texture config count   0x04 metal texture config count
        ///   0x08 texture configs        0x0C metal texture configs
        ///   0x10 vertices               0x14 indices
        ///   0x18 vertex count (u16)     0x1A metal vertex count (u16)
        /// Metal (chrome-shaded) geometry is a separate pass the game renders with a
        /// reflection shader; it is skipped here.
        /// </summary>
        private void ReadMesh(byte[] data, int baseOffset, int meshHeader)
        {
            if (!RCBinary.InRange(data, meshHeader, MeshHeaderSize))
            {
                warnings.Add("Mesh header is outside the file; geometry skipped.");
                return;
            }

            int texConfigCount = RCBinary.I32(data, meshHeader + 0x00);
            int metalTexConfigCount = RCBinary.I32(data, meshHeader + 0x04);
            int texConfigPointer = RCBinary.I32(data, meshHeader + 0x08);
            int metalTexConfigPointer = RCBinary.I32(data, meshHeader + 0x0C);
            int vertexPointer = RCBinary.I32(data, meshHeader + 0x10);
            int indexPointer = RCBinary.I32(data, meshHeader + 0x14);
            int vertexCount = RCBinary.U16(data, meshHeader + 0x18);
            int metalVertexCount = RCBinary.U16(data, meshHeader + 0x1A);

            if (texConfigPointer > 0 && texConfigCount > 0)
                ReadTextureConfigs(data, baseOffset + texConfigPointer, texConfigCount, textureConfigs);

            if (metalTexConfigPointer > 0 && metalTexConfigCount > 0)
                ReadTextureConfigs(data, baseOffset + metalTexConfigPointer, metalTexConfigCount, metalTextureConfigs);

            int indexCount = 0;
            for (int i = 0; i < textureConfigs.Count; i++)
                indexCount += textureConfigs[i].size;

            int metalIndexCount = 0;
            for (int i = 0; i < metalTextureConfigs.Count; i++)
                metalIndexCount += metalTextureConfigs[i].size;

            if (vertexCount > 0 && vertexPointer > 0)
                ReadVertices(data, baseOffset + vertexPointer, vertexCount);

            if (indexCount > 0 && indexPointer > 0)
                indices = ReadIndices(data, baseOffset + indexPointer, indexCount);

            // Metal vertices follow the textured ones; metal indices follow the
            // textured ones, and are numbered from zero against the metal vertices.
            if (metalVertexCount > 0 && vertexPointer > 0)
                ReadMetalVertices(data, baseOffset + vertexPointer + vertexCount * VertexSize, metalVertexCount);

            if (metalIndexCount > 0 && indexPointer > 0)
                metalIndices = ReadIndices(data, baseOffset + indexPointer + indexCount * 2, metalIndexCount);
        }

        /// <summary>Texture config entry (0x10): texture id, first index, index count, mode flags.</summary>
        private void ReadTextureConfigs(byte[] data, int pointer, int count, List<RCTextureConfig> target)
        {
            if (!RCBinary.InRange(data, pointer, count * TextureConfigSize))
            {
                warnings.Add("Texture config block is outside the file.");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                int o = pointer + i * TextureConfigSize;
                target.Add(new RCTextureConfig
                {
                    textureId = RCBinary.I32(data, o + 0x00),
                    start = RCBinary.I32(data, o + 0x04),
                    size = RCBinary.I32(data, o + 0x08),
                    mode = RCBinary.I32(data, o + 0x0C)
                });
            }
        }

        /// <summary>
        /// Vertex (0x28): position float3, normal float3, uv float2,
        /// then four 0-255 bone weights and four bone indices.
        /// </summary>
        private void ReadVertices(byte[] data, int pointer, int count)
        {
            if (!RCBinary.InRange(data, pointer, count * VertexSize))
            {
                warnings.Add("Vertex block is outside the file.");
                return;
            }

            positions = new Vector3[count];
            normals = new Vector3[count];
            uvs = new Vector2[count];
            boneWeights = new byte[count * 4];
            boneIndices = new byte[count * 4];

            for (int i = 0; i < count; i++)
            {
                int o = pointer + i * VertexSize;

                positions[i] = new Vector3(
                    RCBinary.F32(data, o + 0x00),
                    RCBinary.F32(data, o + 0x04),
                    RCBinary.F32(data, o + 0x08));

                normals[i] = new Vector3(
                    RCBinary.F32(data, o + 0x0C),
                    RCBinary.F32(data, o + 0x10),
                    RCBinary.F32(data, o + 0x14));

                uvs[i] = new Vector2(
                    RCBinary.F32(data, o + 0x18),
                    RCBinary.F32(data, o + 0x1C));

                for (int k = 0; k < 4; k++)
                {
                    boneWeights[i * 4 + k] = data[o + 0x20 + k];
                    boneIndices[i * 4 + k] = data[o + 0x24 + k];
                }
            }
        }

        /// <summary>Metal vertex (0x20): position float3, normal float3, weights, bone ids. No UVs.</summary>
        private void ReadMetalVertices(byte[] data, int pointer, int count)
        {
            if (!RCBinary.InRange(data, pointer, count * MetalVertexSize))
            {
                warnings.Add("Chrome vertex block is outside the file.");
                return;
            }

            metalPositions = new Vector3[count];
            metalNormals = new Vector3[count];
            metalBoneWeights = new byte[count * 4];
            metalBoneIndices = new byte[count * 4];

            for (int i = 0; i < count; i++)
            {
                int o = pointer + i * MetalVertexSize;

                metalPositions[i] = new Vector3(
                    RCBinary.F32(data, o + 0x00),
                    RCBinary.F32(data, o + 0x04),
                    RCBinary.F32(data, o + 0x08));

                metalNormals[i] = new Vector3(
                    RCBinary.F32(data, o + 0x0C),
                    RCBinary.F32(data, o + 0x10),
                    RCBinary.F32(data, o + 0x14));

                for (int k = 0; k < 4; k++)
                {
                    metalBoneWeights[i * 4 + k] = data[o + 0x18 + k];
                    metalBoneIndices[i * 4 + k] = data[o + 0x1C + k];
                }
            }
        }

        private ushort[] ReadIndices(byte[] data, int pointer, int count)
        {
            if (!RCBinary.InRange(data, pointer, count * 2))
            {
                warnings.Add("Index block is outside the file.");
                return new ushort[0];
            }

            var result = new ushort[count];
            for (int i = 0; i < count; i++)
                result[i] = RCBinary.U16(data, pointer + i * 2);

            return result;
        }

        /// <summary>
        /// Bone matrix (0x40 bytes): a 3x4 matrix whose 3x3 part is the bind rotation
        /// and scale, plus the cumulative (model-space) offset at 0x30 and the parent
        /// index -- stored pre-multiplied by the 0x40 element size -- at 0x3E.
        ///
        /// Bone data (0x10 bytes): the bone's rest translation relative to its parent
        /// at 0x00, and the parent index again at 0x0E.
        ///
        /// All translations are stored scaled by 1024.
        /// </summary>
        private void ReadSkeleton(byte[] data, int baseOffset, int boneMatrixPointer, int boneDataPointer)
        {
            int matrixBlock = baseOffset + boneMatrixPointer;
            int dataBlock = baseOffset + boneDataPointer;

            // Deadlocked packs bone matrices into 0x30 bytes instead of 0x40. Some
            // Deadlocked levels still carry RaC3-format models, so infer the stride
            // from the gap between the two blocks rather than trusting the game id.
            int stride = ((boneDataPointer - boneMatrixPointer) / boneCount == 0x30) ? 0x30 : 0x40;

            if (!RCBinary.InRange(data, matrixBlock, boneCount * stride) ||
                !RCBinary.InRange(data, dataBlock, boneCount * 0x10))
            {
                warnings.Add("Skeleton block is outside the file; model imported without bones.");
                boneCount = 0;
                return;
            }

            bones = new RCBone[boneCount];

            for (int i = 0; i < boneCount; i++)
            {
                int m = matrixBlock + i * stride;
                int d = dataBlock + i * 0x10;

                var bone = new RCBone();

                var inverseBind = Matrix4x4.identity;

                if (stride == 0x40)
                {
                    // The stored 3x3 is the bind rotation, so its transpose is the
                    // inverse bind rotation. The cumulative offset is the inverse
                    // bind translation.
                    for (int row = 0; row < 3; row++)
                    {
                        for (int col = 0; col < 3; col++)
                            inverseBind[row, col] = RCBinary.F32(data, m + (col * 4 + row) * 4);
                    }

                    inverseBind[0, 3] = RCBinary.F32(data, m + 0x30) / 1024f;
                    inverseBind[1, 3] = RCBinary.F32(data, m + 0x34) / 1024f;
                    inverseBind[2, 3] = RCBinary.F32(data, m + 0x38) / 1024f;
                }
                else
                {
                    // Deadlocked stores the inverse bind matrix directly.
                    for (int row = 0; row < 3; row++)
                    {
                        for (int col = 0; col < 4; col++)
                            inverseBind[row, col] = RCBinary.F32(data, m + (row * 4 + col) * 4);
                    }

                    inverseBind[0, 3] /= 1024f;
                    inverseBind[1, 3] /= 1024f;
                    inverseBind[2, 3] /= 1024f;
                }

                bone.inverseBind = inverseBind;

                bone.localTranslation = new Vector3(
                    RCBinary.F32(data, d + 0x00) / 1024f,
                    RCBinary.F32(data, d + 0x04) / 1024f,
                    RCBinary.F32(data, d + 0x08) / 1024f);

                int parent = RCBinary.I16(data, d + 0x0E) / 0x40;
                bone.parent = (i == 0 || parent < 0 || parent >= boneCount || parent == i) ? -1 : parent;

                bones[i] = bone;
            }

            // A bone may only be parented to an earlier bone; anything else would be a
            // cycle and would hang the hierarchy walk.
            for (int i = 0; i < boneCount; i++)
            {
                if (bones[i].parent >= i)
                {
                    warnings.Add("Bone " + i + " has a forward parent reference (" + bones[i].parent + "); re-parented to the root.");
                    bones[i].parent = (i == 0) ? -1 : 0;
                }
            }
        }

        /// <summary>
        /// Animation header (0x1C bytes, pointers relative to the model base):
        ///   0x10 frame count   0x12 sound count   0x18 speed (float)
        /// followed by one int32 frame offset per frame.
        /// </summary>
        private void ReadAnimations(byte[] data, int baseOffset, int animationCount)
        {
            int table = baseOffset + HeaderSize;
            if (!RCBinary.InRange(data, table, animationCount * 4))
            {
                warnings.Add("Animation offset table is outside the file.");
                return;
            }

            for (int i = 0; i < animationCount; i++)
            {
                int animOffset = RCBinary.I32(data, table + i * 4);
                var animation = new RCAnimation();

                if (animOffset > 0 && RCBinary.InRange(data, baseOffset + animOffset, 0x1C))
                {
                    int header = baseOffset + animOffset;
                    int frameCount = data[header + 0x10];
                    animation.speed = RCBinary.F32(data, header + 0x18);

                    int frameTable = header + 0x1C;
                    if (RCBinary.InRange(data, frameTable, frameCount * 4))
                    {
                        for (int f = 0; f < frameCount; f++)
                        {
                            int frameOffset = RCBinary.I32(data, frameTable + f * 4);
                            RCFrame frame = ReadFrame(data, baseOffset + frameOffset);
                            if (frame != null)
                                animation.frames.Add(frame);
                        }
                    }
                    else
                    {
                        warnings.Add("Animation " + i + " has a frame table outside the file.");
                    }
                }

                animations.Add(animation);
            }
        }

        /// <summary>
        /// Frame header (0x10 bytes):
        ///   0x00 speed (float)      0x04 frame index (u16)   0x06 payload length in 0x10 blocks
        ///   0x08 scale offset       0x0A scale count
        ///   0x0C translation offset 0x0E translation count
        /// The payload starts with one packed quaternion per bone (four int16 scaled by
        /// 32767, W negated), then the sparse scale and translation lists. Each sparse
        /// entry is three int16 plus a bone index byte and a flag byte; scales are
        /// scaled by 4096 and translations by 1024.
        /// </summary>
        private RCFrame ReadFrame(byte[] data, int offset)
        {
            if (!RCBinary.InRange(data, offset, 0x10))
            {
                warnings.Add("A frame header lies outside the file; frame dropped.");
                return null;
            }

            var frame = new RCFrame();
            frame.speed = RCBinary.F32(data, offset + 0x00);

            int payloadBlocks = RCBinary.U16(data, offset + 0x06);
            int scaleOffset = RCBinary.U16(data, offset + 0x08);
            int scaleCount = RCBinary.U16(data, offset + 0x0A);
            int translationOffset = RCBinary.U16(data, offset + 0x0C);
            int translationCount = RCBinary.U16(data, offset + 0x0E);

            int payload = offset + 0x10;
            int payloadLength = payloadBlocks * 0x10;

            if (!RCBinary.InRange(data, payload, payloadLength))
            {
                warnings.Add("A frame payload lies outside the file; frame dropped.");
                return null;
            }

            frame.rotations = new Quaternion[boneCount];
            frame.translations = new Vector3[boneCount];
            frame.hasTranslation = new bool[boneCount];
            frame.scales = new Vector3[boneCount];
            frame.hasScale = new bool[boneCount];

            for (int i = 0; i < boneCount; i++)
            {
                if ((i + 1) * 8 > payloadLength)
                {
                    frame.rotations[i] = Quaternion.identity;
                    continue;
                }

                int o = payload + i * 8;
                frame.rotations[i] = new Quaternion(
                    RCBinary.I16(data, o + 0x00) / 32767f,
                    RCBinary.I16(data, o + 0x02) / 32767f,
                    RCBinary.I16(data, o + 0x04) / 32767f,
                    -RCBinary.I16(data, o + 0x06) / 32767f);
            }

            for (int i = 0; i < scaleCount; i++)
            {
                int o = scaleOffset + i * 8;
                if (o + 8 > payloadLength)
                    break;

                int bone = data[payload + o + 0x06];
                if (bone >= boneCount)
                    continue;

                var scale = new Vector3(
                    RCBinary.I16(data, payload + o + 0x00) / 4096f,
                    RCBinary.I16(data, payload + o + 0x02) / 4096f,
                    RCBinary.I16(data, payload + o + 0x04) / 4096f);

                // A bone can appear more than once; the entries accumulate.
                frame.scales[bone] = frame.hasScale[bone] ? frame.scales[bone] + scale : scale;
                frame.hasScale[bone] = true;
            }

            for (int i = 0; i < translationCount; i++)
            {
                int o = translationOffset + i * 8;
                if (o + 8 > payloadLength)
                    break;

                int bone = data[payload + o + 0x06];
                if (bone >= boneCount)
                    continue;

                var translation = new Vector3(
                    RCBinary.I16(data, payload + o + 0x00) / 1024f,
                    RCBinary.I16(data, payload + o + 0x02) / 1024f,
                    RCBinary.I16(data, payload + o + 0x04) / 1024f);

                frame.translations[bone] = frame.hasTranslation[bone] ? frame.translations[bone] + translation : translation;
                frame.hasTranslation[bone] = true;
            }

            return frame;
        }
    }
}
