using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RatchetImport
{
    /// <summary>
    /// Textures, materials and their alpha flags, shared by every preview and
    /// thumbnail built from one loaded level so the texture table is decoded once.
    /// Owned by the window, which destroys the contents when the level changes.
    /// </summary>
    internal sealed class RCPreviewCache
    {
        public readonly Dictionary<int, Texture2D> textures = new Dictionary<int, Texture2D>();
        public readonly Dictionary<int, bool> textureHasAlpha = new Dictionary<int, bool>();
        public readonly Dictionary<int, Material> materials = new Dictionary<int, Material>();

        public void DestroyMaterials()
        {
            Destroy(materials);
        }

        public void DestroyAll()
        {
            Destroy(materials);
            Destroy(textures);
            textureHasAlpha.Clear();
        }

        private static void Destroy<T>(Dictionary<int, T> cache) where T : UnityEngine.Object
        {
            foreach (T value in cache.Values)
            {
                if (value != null)
                    UnityEngine.Object.DestroyImmediate(value);
            }

            cache.Clear();
        }
    }

    /// <summary>
    /// Renders a moby into an editor window without writing anything to the project.
    ///
    /// The mesh, skeleton and coordinate conversion all come from
    /// <see cref="RCMobyFactory"/> -- the same code the importer uses -- so what you
    /// see here is what lands in the project. Animation is applied by sampling the
    /// parsed frames directly onto the bone transforms rather than going through an
    /// AnimationClip, which keeps the preview independent of the editor's animation
    /// mode.
    /// </summary>
    internal sealed class RCMobyPreview : IDisposable
    {
        private PreviewRenderUtility previewUtility;
        private GameObject root;
        private Mesh mesh;

        /// <summary>
        /// Preview scenes do not run the skinning update, so a SkinnedMeshRenderer
        /// there keeps drawing its bind pose no matter what the bones do. The rig is
        /// therefore kept as a disabled renderer used only as a BakeMesh source, and
        /// what actually draws is this baked snapshot, refreshed on every pose change.
        /// </summary>
        private SkinnedMeshRenderer rig;
        private Mesh bakedMesh;
        private MeshFilter bakedFilter;

        private RCMobyModel model;
        private float scale;

        private Transform[] boneTransforms;
        private Vector3[] bindPositions;
        private Quaternion[] bindRotations;
        private Vector3[] bindScales;

        private float[][] frameTimes;

        /// <summary>Animations that actually have frames, as indices into the model's list.</summary>
        private int[] playableAnimations;

        public Bounds bounds;
        public float yaw = 140f;
        public float pitch = 12f;
        public float zoom = 1f;

        /// <summary>Index into <see cref="playableAnimations"/>, or -1 for the bind pose.</summary>
        public int animationSlot = -1;

        public float time;
        public bool playing = true;

        /// <summary>Diagnostic: draw both sides of every triangle. If a model only looks
        /// right with this on, the problem is winding, not shading.</summary>
        public bool showBackfaces;

        private Material[] materials = new Material[0];

        public readonly List<string> warnings = new List<string>();

        public int AnimationCount
        {
            get { return playableAnimations != null ? playableAnimations.Length : 0; }
        }

        public bool HasSkeleton
        {
            get { return model != null && model.HasSkeleton; }
        }

        /// <summary>Popup labels, slot 0 being the bind pose. Built once.</summary>
        public string[] animationNames = { "Bind pose" };

        public float Length(int slot)
        {
            if (slot < 0 || slot >= AnimationCount)
                return 0f;

            float[] times = frameTimes[slot];
            return (times.Length > 0) ? times[times.Length - 1] : 0f;
        }

        public float CurrentLength
        {
            get { return Length(animationSlot); }
        }

        public static RCMobyPreview Create(RCEngineFile engine, RCMobyModel model, Stream vram, float importScale,
            bool withTextures, bool cutoutTransparency, bool includeChrome, RCPreviewCache cache)
        {
            var preview = new RCMobyPreview();
            preview.model = model;
            preview.scale = model.size * importScale;

            var submeshTextureIds = new List<int>();
            preview.mesh = RCMobyFactory.BuildMesh(model, preview.scale, includeChrome, submeshTextureIds, preview.warnings);
            preview.mesh.name = "PreviewMesh";
            preview.mesh.hideFlags = HideFlags.HideAndDontSave;
            preview.bounds = preview.mesh.bounds;

            var materials = new Material[submeshTextureIds.Count];
            for (int i = 0; i < submeshTextureIds.Count; i++)
            {
                materials[i] = GetSharedMaterial(engine, submeshTextureIds[i], vram, withTextures, cutoutTransparency,
                    cache, preview.warnings);
            }

            preview.materials = materials;

            preview.root = new GameObject("MobyPreview");
            preview.root.hideFlags = HideFlags.HideAndDontSave;

            if (model.HasSkeleton)
            {
                string[] bonePaths;
                RCMobyFactory.BuildSkeleton(model, preview.root.transform, preview.scale, out preview.boneTransforms, out bonePaths);

                int boneCount = preview.boneTransforms.Length;
                preview.bindPositions = new Vector3[boneCount];
                preview.bindRotations = new Quaternion[boneCount];
                preview.bindScales = new Vector3[boneCount];

                for (int i = 0; i < boneCount; i++)
                {
                    preview.boneTransforms[i].gameObject.hideFlags = HideFlags.HideAndDontSave;
                    preview.bindPositions[i] = preview.boneTransforms[i].localPosition;
                    preview.bindRotations[i] = preview.boneTransforms[i].localRotation;
                    preview.bindScales[i] = preview.boneTransforms[i].localScale;
                }

                preview.rig = preview.root.AddComponent<SkinnedMeshRenderer>();
                preview.rig.sharedMesh = preview.mesh;
                preview.rig.sharedMaterials = materials;
                preview.rig.bones = preview.boneTransforms;
                preview.rig.rootBone = preview.boneTransforms[0];
                preview.rig.updateWhenOffscreen = true;

                // Only a bake source; the visible geometry is the baked snapshot below.
                preview.rig.enabled = false;

                preview.bakedMesh = new Mesh();
                preview.bakedMesh.name = "PreviewBaked";
                preview.bakedMesh.hideFlags = HideFlags.HideAndDontSave;
                preview.bakedMesh.MarkDynamic();

                var baked = new GameObject("Baked");
                baked.hideFlags = HideFlags.HideAndDontSave;
                baked.transform.SetParent(preview.root.transform, false);

                preview.bakedFilter = baked.AddComponent<MeshFilter>();
                preview.bakedFilter.sharedMesh = preview.bakedMesh;
                baked.AddComponent<MeshRenderer>().sharedMaterials = materials;
            }
            else
            {
                preview.root.AddComponent<MeshFilter>().sharedMesh = preview.mesh;
                preview.root.AddComponent<MeshRenderer>().sharedMaterials = materials;
            }

            var playable = new List<int>();
            var times = new List<float[]>();
            for (int i = 0; i < model.animations.Count; i++)
            {
                if (model.animations[i].frames.Count == 0)
                    continue;

                playable.Add(i);
                times.Add(RCMobyFactory.FrameTimes(model.animations[i]));
            }

            preview.playableAnimations = playable.ToArray();
            preview.frameTimes = times.ToArray();
            preview.animationSlot = (preview.playableAnimations.Length > 0) ? 0 : -1;

            preview.animationNames = new string[preview.playableAnimations.Length + 1];
            preview.animationNames[0] = "Bind pose";
            for (int i = 0; i < preview.playableAnimations.Length; i++)
            {
                RCAnimation animation = model.animations[preview.playableAnimations[i]];
                preview.animationNames[i + 1] = string.Format("Anim {0:D2}   ({1} frames, {2:0.00}s)",
                    preview.playableAnimations[i], animation.frames.Count, preview.Length(i));
            }

            preview.previewUtility = new PreviewRenderUtility();
            preview.previewUtility.camera.fieldOfView = 30f;
            preview.previewUtility.camera.clearFlags = CameraClearFlags.SolidColor;
            preview.previewUtility.camera.backgroundColor = new Color(0.16f, 0.17f, 0.19f, 1f);
            preview.previewUtility.ambientColor = new Color(0.35f, 0.36f, 0.40f, 1f);

            preview.previewUtility.lights[0].intensity = 1.3f;
            preview.previewUtility.lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0f);
            preview.previewUtility.lights[1].intensity = 0.7f;
            preview.previewUtility.lights[1].transform.rotation = Quaternion.Euler(-20f, -130f, 0f);

            preview.previewUtility.AddSingleGO(preview.root);
            preview.ApplyPose();
            return preview;
        }

        /// <summary>
        /// Materials and textures are shared across every preview built from the same
        /// level, so browsing the list does not re-decode the texture table each time.
        /// The caches are owned by the window and outlive individual previews.
        /// </summary>
        private static Material GetSharedMaterial(RCEngineFile engine, int textureId, Stream vram, bool withTextures,
            bool cutoutTransparency, RCPreviewCache cache, List<string> warnings)
        {
            Material cached;
            if (cache.materials.TryGetValue(textureId, out cached) && cached != null)
                return cached;

            if (textureId == RCMobyFactory.ChromeTextureId)
            {
                Material chrome = RCMobyFactory.CreateChromeMaterial("preview_chrome");
                chrome.hideFlags = HideFlags.HideAndDontSave;
                cache.materials[textureId] = chrome;
                return chrome;
            }

            Texture2D texture = null;
            bool hasAlpha = false;

            if (withTextures && textureId >= 0)
            {
                if (cache.textures.TryGetValue(textureId, out texture) && texture != null)
                {
                    cache.textureHasAlpha.TryGetValue(textureId, out hasAlpha);
                }
                else
                {
                    texture = RCMobyFactory.DecodeTexture(engine, textureId, vram, out hasAlpha, warnings);
                    if (texture != null)
                    {
                        texture.hideFlags = HideFlags.HideAndDontSave;
                        texture.wrapMode = TextureWrapMode.Repeat;
                    }

                    cache.textures[textureId] = texture;
                    cache.textureHasAlpha[textureId] = hasAlpha;
                }
            }

            Material material = RCMobyFactory.CreateMaterial("preview_" + textureId, texture, hasAlpha, cutoutTransparency);
            material.hideFlags = HideFlags.HideAndDontSave;
            cache.materials[textureId] = material;
            return material;
        }

        // -------------------------------------------------------------- posing

        public void ApplyPose()
        {
            if (!HasSkeleton || boneTransforms == null)
                return;

            if (animationSlot < 0 || animationSlot >= AnimationCount)
            {
                for (int i = 0; i < boneTransforms.Length; i++)
                {
                    boneTransforms[i].localPosition = bindPositions[i];
                    boneTransforms[i].localRotation = bindRotations[i];
                    boneTransforms[i].localScale = bindScales[i];
                }

                Bake();
                return;
            }

            RCAnimation animation = model.animations[playableAnimations[animationSlot]];
            float[] times = frameTimes[animationSlot];
            int frameCount = animation.frames.Count;

            int a = 0;
            int b = 0;
            float u = 0f;

            if (frameCount > 1)
            {
                float length = times[times.Length - 1];
                float t = (length > 0f) ? Mathf.Repeat(time, length) : 0f;

                while (a < frameCount - 2 && times[a + 1] <= t)
                    a++;

                b = a + 1;
                float span = times[b] - times[a];
                u = (span > 0f) ? Mathf.Clamp01((t - times[a]) / span) : 0f;
            }

            RCFrame frameA = animation.frames[a];
            RCFrame frameB = animation.frames[b];

            for (int i = 0; i < boneTransforms.Length; i++)
            {
                Vector3 positionA, scaleA, positionB, scaleB;
                Quaternion rotationA, rotationB;

                RCMobyFactory.SampleBone(model, frameA, i, scale, out positionA, out rotationA, out scaleA);
                RCMobyFactory.SampleBone(model, frameB, i, scale, out positionB, out rotationB, out scaleB);

                boneTransforms[i].localPosition = Vector3.Lerp(positionA, positionB, u);
                boneTransforms[i].localRotation = Quaternion.Slerp(rotationA, rotationB, u);
                boneTransforms[i].localScale = Vector3.Lerp(scaleA, scaleB, u);
            }

            Bake();
        }

        /// <summary>Snapshots the current skinned pose into the mesh that actually draws.</summary>
        private void Bake()
        {
            if (rig == null || bakedMesh == null)
                return;

            rig.BakeMesh(bakedMesh);

            // BakeMesh leaves the bounds stale, which can cull the model out of frame.
            // The camera keeps framing the bind pose so it does not chase the
            // animation around.
            bakedMesh.RecalculateBounds();

            if (bakedFilter != null)
                bakedFilter.sharedMesh = bakedMesh;
        }

        // ------------------------------------------------------------ rendering

        private void SetupCamera(Rect rect)
        {
            Camera camera = previewUtility.camera;

            float radius = Mathf.Max(bounds.extents.magnitude, 0.001f);
            float distance = radius * 3.6f / Mathf.Max(zoom, 0.05f);

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            camera.transform.rotation = rotation;
            camera.transform.position = bounds.center - rotation * Vector3.forward * distance;
            camera.nearClipPlane = Mathf.Max(distance * 0.01f, 0.0001f);
            camera.farClipPlane = distance * 20f;
        }

        public void Draw(Rect rect, GUIStyle background)
        {
            // Rects are only meaningful on Repaint; drawing on a Layout pass would
            // render into a zero-sized target.
            if (previewUtility == null || Event.current.type != EventType.Repaint
                || rect.width < 4f || rect.height < 4f)
                return;

            // Cull mode lives on the material, and the materials are shared, so it is
            // reasserted here rather than tracked per preview.
            float cull = showBackfaces ? 0f : 2f;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null && materials[i].HasProperty("_Cull"))
                    materials[i].SetFloat("_Cull", cull);
            }

            previewUtility.BeginPreview(rect, background);
            SetupCamera(rect);

            // allowScriptableRenderPipeline: the project renders through URP, so the
            // preview must too or the materials come out unlit.
            previewUtility.Render(true, false);

            Texture rendered = previewUtility.EndPreview();
            GUI.DrawTexture(rect, rendered, ScaleMode.StretchToFill, false);
        }

        /// <summary>Renders a square thumbnail the caller then owns.</summary>
        public Texture2D RenderThumbnail(int size)
        {
            if (previewUtility == null)
                return null;

            var rect = new Rect(0f, 0f, size, size);
            previewUtility.BeginStaticPreview(rect);
            SetupCamera(rect);
            previewUtility.Render(true, false);
            return previewUtility.EndStaticPreview();
        }

        public void ResetView()
        {
            yaw = 140f;
            pitch = 12f;
            zoom = 1f;
        }

        public void Dispose()
        {
            if (previewUtility != null)
            {
                previewUtility.Cleanup();
                previewUtility = null;
            }

            // Cleanup tears down the preview scene along with the objects in it, but
            // the mesh was created outside that scene and must go explicitly.
            if (mesh != null)
            {
                UnityEngine.Object.DestroyImmediate(mesh);
                mesh = null;
            }

            if (bakedMesh != null)
            {
                UnityEngine.Object.DestroyImmediate(bakedMesh);
                bakedMesh = null;
            }

            if (root != null)
            {
                UnityEngine.Object.DestroyImmediate(root);
                root = null;
            }

            boneTransforms = null;
            model = null;
        }
    }
}
