using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RatchetImport
{
    /// <summary>
    /// Turns parsed moby data into live Unity objects: meshes, bone hierarchies,
    /// animation clips, textures and materials. Nothing here touches the
    /// AssetDatabase, so both the importer and the preview window run the exact same
    /// conversion -- if a model looks right in the preview it will import that way.
    /// </summary>
    internal static class RCMobyFactory
    {
        // ------------------------------------------------- coordinate conversion

        /// <summary>
        /// The game is Z-up and right-handed; Unity is Y-up and left-handed. Swapping
        /// Y and Z converts between them. The swap flips handedness, which is exactly
        /// what makes the geometry read correctly in Unity -- at the cost of also
        /// flipping triangle winding, which is undone when the index buffer is built.
        /// </summary>
        public static Vector3 SwapYZ(Vector3 v)
        {
            return new Vector3(v.x, v.z, v.y);
        }

        /// <summary>
        /// A rotation seen through an orientation-reversing basis change becomes the
        /// same rotation about the swapped axis, in the opposite direction.
        /// </summary>
        public static Quaternion SwapYZ(Quaternion q)
        {
            return new Quaternion(-q.x, -q.z, -q.y, q.w);
        }

        /// <summary>
        /// Conjugates a transform by the axis swap. Because the swap matrix is a
        /// symmetric permutation, this reduces to permuting rows and columns.
        /// </summary>
        public static Matrix4x4 SwapYZ(Matrix4x4 m)
        {
            var result = new Matrix4x4();
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                    result[row, col] = m[Swap(row), Swap(col)];
            }
            return result;
        }

        private static int Swap(int i)
        {
            if (i == 1) return 2;
            if (i == 2) return 1;
            return i;
        }

        public static Matrix4x4 ScaleTranslation(Matrix4x4 m, float scale)
        {
            m[0, 3] *= scale;
            m[1, 3] *= scale;
            m[2, 3] *= scale;
            return m;
        }

        public static Quaternion Normalize(Quaternion q)
        {
            float length = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
            if (length < 1e-6f)
                return Quaternion.identity;

            return new Quaternion(q.x / length, q.y / length, q.z / length, q.w / length);
        }

        // ---------------------------------------------------------------- mesh

        /// <summary>Marks a submesh as belonging to the chrome pass rather than a level texture.</summary>
        public const int ChromeTextureId = -2;

        public static Mesh BuildMesh(RCMobyModel model, float scale, List<int> submeshTextureIds, List<string> warnings)
        {
            return BuildMesh(model, scale, true, submeshTextureIds, warnings);
        }

        public static Mesh BuildMesh(RCMobyModel model, float scale, bool includeChrome,
            List<int> submeshTextureIds, List<string> warnings)
        {
            int texturedCount = model.VertexCount;
            int chromeCount = includeChrome ? model.MetalVertexCount : 0;
            int vertexCount = texturedCount + chromeCount;

            var mesh = new Mesh();
            mesh.indexFormat = (vertexCount > 65535)
                ? UnityEngine.Rendering.IndexFormat.UInt32
                : UnityEngine.Rendering.IndexFormat.UInt16;

            var vertices = new Vector3[vertexCount];
            var normals = new Vector3[vertexCount];
            var uvs = new Vector2[vertexCount];
            bool normalsUsable = false;

            for (int i = 0; i < texturedCount; i++)
            {
                vertices[i] = SwapYZ(model.positions[i]) * scale;

                Vector3 normal = SwapYZ(model.normals[i]);
                if (normal.sqrMagnitude > 1e-8f)
                {
                    normals[i] = normal.normalized;
                    normalsUsable = true;
                }

                // The game samples textures with V running downwards.
                uvs[i] = new Vector2(model.uvs[i].x, 1f - model.uvs[i].y);
            }

            // Chrome vertices carry no UVs -- in game they are shaded from an
            // environment map -- so they get a flat coordinate and a chrome material.
            for (int i = 0; i < chromeCount; i++)
            {
                int v = texturedCount + i;
                vertices[v] = SwapYZ(model.metalPositions[i]) * scale;

                Vector3 normal = SwapYZ(model.metalNormals[i]);
                if (normal.sqrMagnitude > 1e-8f)
                {
                    normals[v] = normal.normalized;
                    normalsUsable = true;
                }

                uvs[v] = Vector2.zero;
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;

            if (model.HasSkeleton)
                mesh.boneWeights = BuildBoneWeights(model, texturedCount, chromeCount, warnings);

            var submeshes = new List<int[]>();

            for (int c = 0; c < model.textureConfigs.Count; c++)
            {
                RCTextureConfig config = model.textureConfigs[c];
                int[] triangles = BuildSubmeshIndices(model.indices, config, 0, texturedCount,
                    vertices, normals, normalsUsable, warnings);

                if (triangles.Length == 0)
                    continue;

                submeshes.Add(triangles);
                submeshTextureIds.Add(config.textureId);
            }

            if (submeshes.Count == 0 && model.indices.Length > 0)
            {
                // No usable texture configs: draw the whole index buffer as one submesh.
                var config = new RCTextureConfig { textureId = -1, start = 0, size = model.indices.Length };
                int[] triangles = BuildSubmeshIndices(model.indices, config, 0, texturedCount,
                    vertices, normals, normalsUsable, warnings);

                if (triangles.Length > 0)
                {
                    submeshes.Add(triangles);
                    submeshTextureIds.Add(-1);
                }
            }

            for (int c = 0; c < (chromeCount > 0 ? model.metalTextureConfigs.Count : 0); c++)
            {
                RCTextureConfig config = model.metalTextureConfigs[c];
                int[] triangles = BuildSubmeshIndices(model.metalIndices, config, texturedCount, chromeCount,
                    vertices, normals, normalsUsable, warnings);

                if (triangles.Length == 0)
                    continue;

                submeshes.Add(triangles);
                submeshTextureIds.Add(ChromeTextureId);
            }

            mesh.subMeshCount = submeshes.Count;
            for (int i = 0; i < submeshes.Count; i++)
                mesh.SetTriangles(submeshes[i], i, false);

            if (normalsUsable)
                mesh.normals = normals;
            else
                mesh.RecalculateNormals();

            if (model.HasSkeleton)
            {
                var bindposes = new Matrix4x4[model.boneCount];
                for (int i = 0; i < model.boneCount; i++)
                    bindposes[i] = SwapYZ(ScaleTranslation(model.bones[i].inverseBind, scale));
                mesh.bindposes = bindposes;
            }

            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        /// <summary>
        /// Rebuilds an index run as triangles, choosing the winding of each triangle
        /// independently.
        ///
        /// Two things fight here. The Y/Z swap mirrors the model, which flips winding
        /// wholesale; and the game's own index buffer is not wound consistently to
        /// begin with, because on the original hardware facing came from the vertex
        /// normals rather than from vertex order. Picking a single global direction
        /// therefore leaves a scattering of inside-out triangles. Instead each
        /// triangle is compared against its own vertex normals, which are unambiguous.
        /// </summary>
        private static int[] BuildSubmeshIndices(ushort[] indices, RCTextureConfig config, int vertexOffset,
            int vertexCount, Vector3[] vertices, Vector3[] normals, bool normalsUsable, List<string> warnings)
        {
            int start = config.start;
            int count = config.size;

            if (start < 0 || count <= 0 || start + count > indices.Length)
            {
                warnings.Add("Texture config " + config.textureId + " covers indices outside the index buffer; skipped.");
                return new int[0];
            }

            var triangles = new List<int>(count);
            int dropped = 0;

            for (int i = 0; i + 2 < count; i += 3)
            {
                int a = indices[start + i];
                int b = indices[start + i + 1];
                int c = indices[start + i + 2];

                if (a >= vertexCount || b >= vertexCount || c >= vertexCount)
                {
                    dropped++;
                    continue;
                }

                a += vertexOffset;
                b += vertexOffset;
                c += vertexOffset;

                // Unity treats cross(b - a, c - a) as the front-facing normal.
                bool reverse = true;

                if (normalsUsable)
                {
                    Vector3 geometric = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]);
                    float agreement = Vector3.Dot(geometric, normals[a] + normals[b] + normals[c]);

                    // A zero here means a degenerate triangle or unusable normals;
                    // fall back to the plain mirror flip.
                    if (agreement != 0f)
                        reverse = agreement < 0f;
                }

                triangles.Add(a);

                if (reverse)
                {
                    triangles.Add(c);
                    triangles.Add(b);
                }
                else
                {
                    triangles.Add(b);
                    triangles.Add(c);
                }
            }

            if (dropped > 0)
                warnings.Add(dropped + " triangle(s) referenced vertices past the end of the buffer and were dropped.");

            return triangles.ToArray();
        }

        private static BoneWeight[] BuildBoneWeights(RCMobyModel model, int texturedCount, int chromeCount, List<string> warnings)
        {
            int vertexCount = texturedCount + chromeCount;
            var weights = new BoneWeight[vertexCount];
            var indices = new int[4];
            var values = new float[4];
            int clamped = 0;

            for (int v = 0; v < vertexCount; v++)
            {
                bool chrome = v >= texturedCount;
                byte[] sourceIndices = chrome ? model.metalBoneIndices : model.boneIndices;
                byte[] sourceWeights = chrome ? model.metalBoneWeights : model.boneWeights;
                int index = chrome ? v - texturedCount : v;

                float total = 0f;

                for (int k = 0; k < 4; k++)
                {
                    int bone = sourceIndices[index * 4 + k];
                    if (bone >= model.boneCount)
                    {
                        bone = 0;
                        clamped++;
                    }

                    indices[k] = bone;
                    values[k] = sourceWeights[index * 4 + k];
                    total += values[k];
                }

                if (total <= 0f)
                {
                    // Rigid vertex: bind it fully to its first bone.
                    values[0] = 1f;
                    values[1] = values[2] = values[3] = 0f;
                }
                else
                {
                    for (int k = 0; k < 4; k++)
                        values[k] /= total;
                }

                // Unity expects the influences in descending weight order.
                SortDescending(indices, values);

                weights[v] = new BoneWeight
                {
                    boneIndex0 = indices[0], weight0 = values[0],
                    boneIndex1 = indices[1], weight1 = values[1],
                    boneIndex2 = indices[2], weight2 = values[2],
                    boneIndex3 = indices[3], weight3 = values[3]
                };
            }

            if (clamped > 0)
                warnings.Add(clamped + " vertex influence(s) referenced a bone past the end of the skeleton and were bound to the root.");

            return weights;
        }

        private static void SortDescending(int[] indices, float[] values)
        {
            for (int i = 1; i < 4; i++)
            {
                for (int j = i; j > 0 && values[j] > values[j - 1]; j--)
                {
                    float fv = values[j]; values[j] = values[j - 1]; values[j - 1] = fv;
                    int iv = indices[j]; indices[j] = indices[j - 1]; indices[j - 1] = iv;
                }
            }
        }

        // ------------------------------------------------------------ skeleton

        /// <summary>
        /// Creates the bone hierarchy in its bind pose, so an unanimated model still
        /// matches the mesh. Animation overwrites these values wholesale.
        /// </summary>
        public static void BuildSkeleton(RCMobyModel model, Transform root, float scale,
            out Transform[] boneTransforms, out string[] bonePaths)
        {
            int boneCount = model.boneCount;
            boneTransforms = new Transform[boneCount];
            bonePaths = new string[boneCount];

            var inverseBind = new Matrix4x4[boneCount];
            for (int i = 0; i < boneCount; i++)
                inverseBind[i] = SwapYZ(ScaleTranslation(model.bones[i].inverseBind, scale));

            for (int i = 0; i < boneCount; i++)
            {
                var go = new GameObject("Bone_" + i.ToString("D2"));
                Transform parent = (model.bones[i].parent >= 0) ? boneTransforms[model.bones[i].parent] : root;
                go.transform.SetParent(parent, false);

                // local = inverse(bind_parent) * bind_self, and inverse(bind) is what
                // the file stores, so the parent's matrix can be used directly.
                Matrix4x4 bind = inverseBind[i].inverse;
                Matrix4x4 local = (model.bones[i].parent >= 0) ? inverseBind[model.bones[i].parent] * bind : bind;

                go.transform.localPosition = local.GetColumn(3);
                go.transform.localRotation = local.rotation;
                go.transform.localScale = local.lossyScale;

                boneTransforms[i] = go.transform;
                bonePaths[i] = (model.bones[i].parent >= 0)
                    ? bonePaths[model.bones[i].parent] + "/" + go.name
                    : go.name;
            }
        }

        // ----------------------------------------------------------- animation

        /// <summary>
        /// Per-frame timestamps for an animation. The header speed wins when set;
        /// otherwise each frame carries its own. Both are a fraction of 60 fps.
        /// </summary>
        public static float[] FrameTimes(RCAnimation animation)
        {
            var times = new float[animation.frames.Count];
            float t = 0f;

            for (int f = 0; f < animation.frames.Count; f++)
            {
                times[f] = t;

                float speed = (animation.speed != 0f) ? animation.speed : animation.frames[f].speed;
                if (speed <= 0f || float.IsNaN(speed))
                    speed = 1f;

                t += 1f / (speed * 60f);
            }

            return times;
        }

        /// <summary>Local transform of one bone on one frame, already in Unity space.</summary>
        public static void SampleBone(RCMobyModel model, RCFrame frame, int bone, float scale,
            out Vector3 position, out Quaternion rotation, out Vector3 boneScale)
        {
            Vector3 translation = frame.hasTranslation[bone] ? frame.translations[bone] : model.bones[bone].localTranslation;
            position = SwapYZ(translation) * scale;
            rotation = SwapYZ(Normalize(frame.rotations[bone]));
            boneScale = frame.hasScale[bone] ? SwapYZ(frame.scales[bone]) : Vector3.one;
        }

        public static AnimationClip BuildClip(RCMobyModel model, RCAnimation animation, string[] bonePaths, float scale)
        {
            int frameCount = animation.frames.Count;
            int boneCount = model.boneCount;

            float[] times = FrameTimes(animation);

            var clip = new AnimationClip();
            clip.frameRate = 60f;

            var px = new float[frameCount];
            var py = new float[frameCount];
            var pz = new float[frameCount];
            var rx = new float[frameCount];
            var ry = new float[frameCount];
            var rz = new float[frameCount];
            var rw = new float[frameCount];
            var sx = new float[frameCount];
            var sy = new float[frameCount];
            var sz = new float[frameCount];

            for (int b = 0; b < boneCount; b++)
            {
                string path = bonePaths[b];
                var previous = Quaternion.identity;

                for (int f = 0; f < frameCount; f++)
                {
                    Vector3 position, boneScale;
                    Quaternion rotation;
                    SampleBone(model, animation.frames[f], b, scale, out position, out rotation, out boneScale);

                    px[f] = position.x;
                    py[f] = position.y;
                    pz[f] = position.z;

                    // Keep the quaternion path continuous so component-wise
                    // interpolation does not take the long way round.
                    if (f > 0 && Quaternion.Dot(previous, rotation) < 0f)
                        rotation = new Quaternion(-rotation.x, -rotation.y, -rotation.z, -rotation.w);
                    previous = rotation;

                    rx[f] = rotation.x;
                    ry[f] = rotation.y;
                    rz[f] = rotation.z;
                    rw[f] = rotation.w;

                    sx[f] = boneScale.x;
                    sy[f] = boneScale.y;
                    sz[f] = boneScale.z;
                }

                SetCurve(clip, path, "m_LocalPosition.x", times, px);
                SetCurve(clip, path, "m_LocalPosition.y", times, py);
                SetCurve(clip, path, "m_LocalPosition.z", times, pz);

                SetCurve(clip, path, "m_LocalRotation.x", times, rx);
                SetCurve(clip, path, "m_LocalRotation.y", times, ry);
                SetCurve(clip, path, "m_LocalRotation.z", times, rz);
                SetCurve(clip, path, "m_LocalRotation.w", times, rw);

                SetCurve(clip, path, "m_LocalScale.x", times, sx);
                SetCurve(clip, path, "m_LocalScale.y", times, sy);
                SetCurve(clip, path, "m_LocalScale.z", times, sz);
            }

            clip.EnsureQuaternionContinuity();
            return clip;
        }

        private static void SetCurve(AnimationClip clip, string path, string property, float[] times, float[] values)
        {
            bool constant = true;
            for (int i = 1; i < values.Length; i++)
            {
                if (Mathf.Abs(values[i] - values[0]) > 1e-6f)
                {
                    constant = false;
                    break;
                }
            }

            if (constant)
            {
                clip.SetCurve(path, typeof(Transform), property, AnimationCurve.Constant(times[0], times[times.Length - 1], values[0]));
                return;
            }

            var keys = new Keyframe[times.Length];
            for (int i = 0; i < times.Length; i++)
            {
                // The game holds each pose and blends linearly to the next, so linear
                // tangents reproduce playback far better than Unity's smoothing default.
                float inTangent = 0f;
                float outTangent = 0f;

                if (i > 0 && times[i] > times[i - 1])
                    inTangent = (values[i] - values[i - 1]) / (times[i] - times[i - 1]);

                if (i < times.Length - 1 && times[i + 1] > times[i])
                    outTangent = (values[i + 1] - values[i]) / (times[i + 1] - times[i]);

                keys[i] = new Keyframe(times[i], values[i], inTangent, outTangent);
            }

            clip.SetCurve(path, typeof(Transform), property, new AnimationCurve(keys));
        }

        // ------------------------------------------------- textures & materials

        /// <summary>Decodes one level texture into a fresh in-memory Texture2D, or null.</summary>
        public static Texture2D DecodeTexture(RCEngineFile engine, int textureId, Stream vram,
            out bool hasAlpha, List<string> warnings)
        {
            hasAlpha = false;

            if (vram == null || textureId < 0 || textureId >= engine.textures.Count)
                return null;

            RCTextureHeader header = engine.textures[textureId];

            if (!RCTextureDecoder.IsSupported(header.compressionFormat))
            {
                if (warnings != null)
                {
                    warnings.Add("Texture " + textureId + " uses unsupported compression 0x" +
                                 header.compressionFormat.ToString("X2") + "; skipped.");
                }
                return null;
            }

            byte[] payload = RCTextureDecoder.ReadPayload(vram, header);
            Color32[] pixels = RCTextureDecoder.Decode(payload, header.width, header.height, header.compressionFormat);

            if (pixels == null)
            {
                if (warnings != null)
                    warnings.Add("Texture " + textureId + " could not be decoded (truncated payload?).");
                return null;
            }

            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a < 250)
                {
                    hasAlpha = true;
                    break;
                }
            }

            var texture = new Texture2D(header.width, header.height, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        public static Shader FindShader()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");

            return shader;
        }

        /// <summary>
        /// Stand-in for the chrome pass. In game these surfaces are shaded from an
        /// environment map and carry no UVs, so a plain reflective material is the
        /// closest honest approximation.
        /// </summary>
        public static Material CreateChromeMaterial(string name)
        {
            var material = new Material(FindShader());
            material.name = name;

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", new Color(0.62f, 0.64f, 0.68f, 1f));
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", new Color(0.62f, 0.64f, 0.68f, 1f));

            if (material.HasProperty("_Metallic"))
                material.SetFloat("_Metallic", 1f);
            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", 0.85f);
            if (material.HasProperty("_Glossiness"))
                material.SetFloat("_Glossiness", 0.85f);

            return material;
        }

        public static Material CreateMaterial(string name, Texture texture, bool hasAlpha, bool cutout)
        {
            var material = new Material(FindShader());
            material.name = name;

            if (texture != null)
            {
                if (material.HasProperty("_BaseMap"))
                    material.SetTexture("_BaseMap", texture);
                if (material.HasProperty("_MainTex"))
                    material.SetTexture("_MainTex", texture);
            }

            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", 0.15f);
            if (material.HasProperty("_Glossiness"))
                material.SetFloat("_Glossiness", 0.15f);

            if (hasAlpha && cutout)
                EnableAlphaClipping(material);

            return material;
        }

        public static void EnableAlphaClipping(Material material)
        {
            if (material.HasProperty("_AlphaClip"))
            {
                // URP Lit
                material.SetFloat("_AlphaClip", 1f);
                material.EnableKeyword("_ALPHATEST_ON");
                if (material.HasProperty("_Cutoff"))
                    material.SetFloat("_Cutoff", 0.5f);
                material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.AlphaTest;
            }
            else if (material.HasProperty("_Mode"))
            {
                // Built-in Standard
                material.SetFloat("_Mode", 1f);
                material.EnableKeyword("_ALPHATEST_ON");
                material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.AlphaTest;
            }
        }
    }
}
