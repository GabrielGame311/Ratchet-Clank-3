using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace RatchetImport
{
    public sealed class RCImportSettings
    {
        public string outputFolder = "Assets/RatchetImports";
        public float importScale = 1f;
        public bool importAnimations = true;
        public bool importTextures = true;
        public bool createAnimatorController = true;

        /// <summary>Enable alpha clipping on materials whose texture has transparency.</summary>
        public bool cutoutTransparency = true;

        /// <summary>
        /// Include the chrome pass. These surfaces have no UVs and are shaded from an
        /// environment map in game, so they come in as a plain reflective material.
        /// Turn off if they turn out to overlay the textured geometry on some models.
        /// </summary>
        public bool importChrome = true;
    }

    public sealed class RCImportResult
    {
        public string prefabPath;
        public int vertexCount;
        public int triangleCount;
        public int boneCount;
        public int animationCount;
        public readonly List<string> warnings = new List<string>();
    }

    /// <summary>
    /// Writes a parsed moby into the project as assets: a mesh, materials and
    /// textures, one AnimationClip per animation, an AnimatorController and a prefab
    /// wiring it all together. The conversion itself lives in <see cref="RCMobyFactory"/>.
    /// </summary>
    public static class RCMobyBuilder
    {
        /// <summary>
        /// Moby classes are referred to in hex throughout the modding community, so
        /// the id is formatted that way here and everywhere it surfaces -- window,
        /// asset names, folder names and log messages alike.
        /// </summary>
        public static string MobyName(short modelId)
        {
            return "Moby_0x" + ((ushort) modelId).ToString("X4");
        }

        /// <summary>Creates and returns the project folder that holds one level's imports.</summary>
        public static string LevelFolder(RCEngineFile engine, string outputFolder)
        {
            string levelName = SafeName(Path.GetFileName(Path.GetDirectoryName(engine.enginePath)));
            if (string.IsNullOrEmpty(levelName))
                levelName = "Level";

            return CreateFolders(outputFolder, levelName);
        }

        public static RCImportResult Import(RCEngineFile engine, RCMobyEntry entry, RCMobyModel model,
            RCImportSettings settings, Stream vram, Dictionary<int, Material> materialCache)
        {
            var result = new RCImportResult();
            result.warnings.AddRange(model.warnings);

            if (model.VertexCount == 0 || model.indices.Length == 0)
                throw new InvalidOperationException(MobyName(entry.modelId) + " has no geometry.");

            string name = MobyName(entry.modelId);
            string levelFolder = LevelFolder(engine, settings.outputFolder);
            string mobyFolder = CreateFolders(levelFolder, name);

            float scale = model.size * settings.importScale;

            // The mesh decides how many submeshes survive, so materials are built from
            // its output rather than from the raw texture config list.
            var submeshTextureIds = new List<int>();
            Mesh mesh = RCMobyFactory.BuildMesh(model, scale, settings.importChrome, submeshTextureIds, result.warnings);
            mesh.name = name + "_Mesh";
            AssetDatabase.CreateAsset(mesh, Path.Combine(mobyFolder, name + "_Mesh.asset").Replace('\\', '/'));

            var materials = new Material[submeshTextureIds.Count];
            for (int i = 0; i < submeshTextureIds.Count; i++)
                materials[i] = GetOrCreateMaterial(engine, submeshTextureIds[i], settings, vram, levelFolder, materialCache, result);

            var root = new GameObject(name);
            Transform[] boneTransforms = null;
            string[] bonePaths = null;

            try
            {
                if (model.HasSkeleton)
                {
                    RCMobyFactory.BuildSkeleton(model, root.transform, scale, out boneTransforms, out bonePaths);

                    var skinned = root.AddComponent<SkinnedMeshRenderer>();
                    skinned.sharedMesh = mesh;
                    skinned.sharedMaterials = materials;
                    skinned.bones = boneTransforms;
                    skinned.rootBone = boneTransforms[0];
                    skinned.updateWhenOffscreen = true;
                }
                else
                {
                    root.AddComponent<MeshFilter>().sharedMesh = mesh;
                    root.AddComponent<MeshRenderer>().sharedMaterials = materials;
                }

                if (settings.importAnimations && model.HasSkeleton && model.animations.Count > 0)
                {
                    string animFolder = CreateFolders(mobyFolder, "Animations");
                    AnimationClip[] clips = BuildClips(model, bonePaths, scale, animFolder, name);
                    result.animationCount = clips.Length;

                    if (settings.createAnimatorController && clips.Length > 0)
                    {
                        string controllerPath = Path.Combine(mobyFolder, name + ".controller").Replace('\\', '/');
                        AnimatorController controller = BuildController(controllerPath, clips);
                        root.AddComponent<Animator>().runtimeAnimatorController = controller;
                    }
                }

                string prefabPath = Path.Combine(mobyFolder, name + ".prefab").Replace('\\', '/');
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

                result.prefabPath = prefabPath;
                result.vertexCount = model.VertexCount;
                result.triangleCount = model.TriangleCount;
                result.boneCount = model.HasSkeleton ? model.boneCount : 0;
                return result;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        // ----------------------------------------------------------- animation

        private static AnimationClip[] BuildClips(RCMobyModel model, string[] bonePaths, float scale, string folder, string mobyName)
        {
            var clips = new List<AnimationClip>();

            for (int a = 0; a < model.animations.Count; a++)
            {
                RCAnimation animation = model.animations[a];
                if (animation.frames.Count == 0)
                    continue;

                AnimationClip clip = RCMobyFactory.BuildClip(model, animation, bonePaths, scale);
                clip.name = mobyName + "_Anim_" + a.ToString("D2");

                AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
                clipSettings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, clipSettings);

                string path = Path.Combine(folder, clip.name + ".anim").Replace('\\', '/');
                AssetDatabase.CreateAsset(clip, path);
                clips.Add(clip);
            }

            return clips.ToArray();
        }

        private static AnimatorController BuildController(string path, AnimationClip[] clips)
        {
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

            for (int i = 0; i < clips.Length; i++)
            {
                AnimatorState state = stateMachine.AddState(clips[i].name);
                state.motion = clips[i];

                if (i == 0)
                    stateMachine.defaultState = state;
            }

            return controller;
        }

        // ------------------------------------------------------ materials

        private static Material GetOrCreateMaterial(RCEngineFile engine, int textureId, RCImportSettings settings,
            Stream vram, string levelFolder, Dictionary<int, Material> materialCache, RCImportResult result)
        {
            Material cached;
            if (materialCache.TryGetValue(textureId, out cached) && cached != null)
                return cached;

            string materialFolder = CreateFolders(levelFolder, "Materials");
            string materialName;

            if (textureId == RCMobyFactory.ChromeTextureId)
                materialName = "chrome";
            else if (textureId >= 0)
                materialName = "tex_" + textureId.ToString("D4");
            else
                materialName = "untextured";

            string materialPath = Path.Combine(materialFolder, materialName + ".mat").Replace('\\', '/');

            var existing = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (existing != null)
            {
                materialCache[textureId] = existing;
                return existing;
            }

            Material material;

            if (textureId == RCMobyFactory.ChromeTextureId)
            {
                material = RCMobyFactory.CreateChromeMaterial(materialName);
            }
            else
            {
                bool hasAlpha = false;
                Texture2D texture = null;

                if (settings.importTextures && textureId >= 0)
                    texture = ImportTexture(engine, textureId, vram, levelFolder, result.warnings, out hasAlpha);

                material = RCMobyFactory.CreateMaterial(materialName, texture, hasAlpha, settings.cutoutTransparency);
            }

            AssetDatabase.CreateAsset(material, materialPath);
            materialCache[textureId] = material;
            return material;
        }

        /// <summary>
        /// Writes one level texture into the project as a PNG and returns the imported
        /// asset, reusing an existing file when there is one.
        /// </summary>
        public static Texture2D ImportTexture(RCEngineFile engine, int textureId, Stream vram,
            string levelFolder, List<string> warnings, out bool hasAlpha)
        {
            hasAlpha = false;

            if (textureId < 0 || textureId >= engine.textures.Count)
            {
                warnings.Add("Texture id " + textureId + " is outside the level texture table.");
                return null;
            }

            string textureFolder = CreateFolders(levelFolder, "Textures");
            string texturePath = Path.Combine(textureFolder, "tex_" + textureId.ToString("D4") + ".png").Replace('\\', '/');

            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (existing != null)
            {
                hasAlpha = HasTransparency(existing);
                return existing;
            }

            if (vram == null)
            {
                warnings.Add("vram.ps3 was not found next to engine.ps3; textures were skipped.");
                return null;
            }

            Texture2D texture = RCMobyFactory.DecodeTexture(engine, textureId, vram, out hasAlpha, warnings);
            if (texture == null)
                return null;

            // Asset paths are project-relative; go through the data path so the write
            // does not depend on the editor's working directory.
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            File.WriteAllBytes(Path.Combine(projectRoot, texturePath), texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceSynchronousImport);

            var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer != null)
            {
                importer.alphaIsTransparency = hasAlpha;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        }

        private static bool HasTransparency(Texture2D texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            return importer != null && importer.alphaIsTransparency;
        }

        // ---------------------------------------------------------- utilities

        /// <summary>Creates <paramref name="child"/> under <paramref name="parent"/>, making missing ancestors as needed.</summary>
        public static string CreateFolders(string parent, string child)
        {
            if (string.IsNullOrEmpty(parent) || !parent.Replace('\\', '/').StartsWith("Assets"))
                parent = "Assets/RatchetImports";

            string full = parent.Replace('\\', '/');
            if (!string.IsNullOrEmpty(child))
                full = full.TrimEnd('/') + "/" + child;

            if (AssetDatabase.IsValidFolder(full))
                return full;

            string[] parts = full.Split('/');
            string current = parts[0]; // "Assets"

            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }

            return current;
        }

        public static string SafeName(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var invalid = Path.GetInvalidFileNameChars();
            var chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (Array.IndexOf(invalid, chars[i]) >= 0)
                    chars[i] = '_';
            }

            return new string(chars);
        }
    }
}
