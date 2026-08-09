using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RatchetImport
{
    /// <summary>
    /// Grid browser for a level's texture table.
    ///
    /// This is the practical route to the game's effect art: the sprites the engine
    /// uses for flames, smoke, sparks and glows all live in this table, even though
    /// the particle systems that drive them do not live in the level files at all.
    /// Find the sheets here, export them, and build the Unity systems around them.
    ///
    /// Thumbnails are decoded lazily and downscaled before caching, so browsing a
    /// few hundred textures does not pin their full-resolution pixels in memory.
    /// </summary>
    internal sealed class RCTextureBrowser
    {
        private const int ThumbnailLimit = 128;
        private const int DecodesPerRepaint = 6;

        private enum FormatFilter
        {
            All = 0,
            BC1 = 1,
            BC2 = 2,
            BC3 = 3
        }

        private static readonly string[] FormatNames = { "All formats", "DXT1 (BC1)", "DXT3 (BC2)", "DXT5 (BC3)" };

        private Vector2 scroll;
        private float cellSize = 96f;
        private string filter = "";
        private FormatFilter formatFilter = FormatFilter.All;
        private bool smallOnly;
        private int inspected = -1;

        private readonly HashSet<int> selected = new HashSet<int>();
        private readonly Dictionary<int, Texture2D> thumbnails = new Dictionary<int, Texture2D>();
        private readonly List<int> pending = new List<int>();
        private readonly List<int> visible = new List<int>();

        private Texture2D checkerboard;

        public string status = "";
        public MessageType statusType = MessageType.Info;

        public void Draw(RCEngineFile engine, Stream vram, RCImportSettings settings, Action repaint)
        {
            if (engine.textures.Count == 0)
            {
                EditorGUILayout.HelpBox("This level has no texture table.", MessageType.Info);
                return;
            }

            DrawToolbar(engine, vram, settings);
            RebuildVisible(engine);
            DrawGrid(engine, vram);
            DrawInspector(engine);
            DecodePending(engine, vram, repaint);
        }

        private void DrawToolbar(RCEngineFile engine, Stream vram, RCImportSettings settings)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                filter = EditorGUILayout.TextField(filter, EditorStyles.toolbarSearchField, GUILayout.Width(140f));
                formatFilter = (FormatFilter) EditorGUILayout.Popup((int) formatFilter, FormatNames,
                    EditorStyles.toolbarPopup, GUILayout.Width(110f));

                smallOnly = GUILayout.Toggle(smallOnly, "Small (64px)", EditorStyles.toolbarButton, GUILayout.Width(88f));

                GUILayout.Space(8f);
                GUILayout.Label("Size", EditorStyles.miniLabel, GUILayout.Width(28f));
                cellSize = GUILayout.HorizontalSlider(cellSize, 48f, 192f, GUILayout.Width(70f));

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Select shown", EditorStyles.toolbarButton, GUILayout.Width(88f)))
                {
                    for (int i = 0; i < visible.Count; i++)
                        selected.Add(visible[i]);
                }

                if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(48f)))
                    selected.Clear();

                using (new EditorGUI.DisabledScope(selected.Count == 0 || vram == null))
                {
                    if (GUILayout.Button("Export " + selected.Count + " as PNG", EditorStyles.toolbarButton, GUILayout.Width(126f)))
                        Export(engine, vram, settings);
                }
            }
        }

        private void RebuildVisible(RCEngineFile engine)
        {
            visible.Clear();

            for (int i = 0; i < engine.textures.Count; i++)
            {
                RCTextureHeader header = engine.textures[i];

                if (smallOnly && (header.width > 64 || header.height > 64))
                    continue;

                if (formatFilter != FormatFilter.All)
                {
                    byte wanted = formatFilter == FormatFilter.BC1 ? RCTextureDecoder.FormatBC1
                        : formatFilter == FormatFilter.BC2 ? RCTextureDecoder.FormatBC2
                        : RCTextureDecoder.FormatBC3;

                    if (header.compressionFormat != wanted)
                        continue;
                }

                if (!string.IsNullOrEmpty(filter) && !i.ToString().Contains(filter))
                    continue;

                visible.Add(i);
            }
        }

        private void DrawGrid(RCEngineFile engine, Stream vram)
        {
            const float padding = 6f;
            float labelHeight = 14f;
            float cellWidth = cellSize + padding;
            float cellHeight = cellSize + labelHeight + padding;

            Rect area = GUILayoutUtility.GetRect(100f, 10000f, 80f, 10000f);
            GUI.Box(area, GUIContent.none, EditorStyles.helpBox);

            int columns = Mathf.Max(1, Mathf.FloorToInt((area.width - 18f) / cellWidth));
            int rowCount = Mathf.CeilToInt(visible.Count / (float) columns);

            var content = new Rect(0f, 0f, area.width - 18f, rowCount * cellHeight);
            scroll = GUI.BeginScrollView(area, scroll, content);

            int firstRow = Mathf.Max(0, Mathf.FloorToInt(scroll.y / cellHeight) - 1);
            int lastRow = Mathf.Min(rowCount - 1, Mathf.CeilToInt((scroll.y + area.height) / cellHeight));

            for (int row = firstRow; row <= lastRow; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    int index = row * columns + column;
                    if (index >= visible.Count)
                        break;

                    var cell = new Rect(column * cellWidth + padding * 0.5f, row * cellHeight + padding * 0.5f,
                        cellSize, cellSize + labelHeight);

                    DrawCell(engine, visible[index], cell, labelHeight);
                }
            }

            GUI.EndScrollView();
        }

        private void DrawCell(RCEngineFile engine, int textureId, Rect cell, float labelHeight)
        {
            var imageRect = new Rect(cell.x, cell.y, cell.width, cell.height - labelHeight);
            bool isSelected = selected.Contains(textureId);

            if (isSelected)
                EditorGUI.DrawRect(new Rect(cell.x - 2f, cell.y - 2f, cell.width + 4f, cell.height + 4f), new Color(0.3f, 0.55f, 0.9f, 0.85f));
            else if (textureId == inspected)
                EditorGUI.DrawRect(new Rect(cell.x - 2f, cell.y - 2f, cell.width + 4f, cell.height + 4f), new Color(1f, 1f, 1f, 0.22f));

            // A checkerboard behind each sprite makes the alpha readable, which is
            // what matters when hunting for effect art.
            DrawCheckerboard(imageRect);

            Texture2D thumbnail;
            if (thumbnails.TryGetValue(textureId, out thumbnail) && thumbnail != null)
            {
                GUI.DrawTexture(imageRect, thumbnail, ScaleMode.ScaleToFit, true);
            }
            else
            {
                if (!thumbnails.ContainsKey(textureId) && !pending.Contains(textureId))
                    pending.Add(textureId);

                EditorGUI.LabelField(imageRect, "...", EditorStyles.centeredGreyMiniLabel);
            }

            RCTextureHeader header = engine.textures[textureId];
            var labelRect = new Rect(cell.x, cell.yMax - labelHeight, cell.width, labelHeight);
            EditorGUI.LabelField(labelRect, string.Format("{0}  {1}x{2}", textureId, header.width, header.height),
                EditorStyles.miniLabel);

            if (Event.current.type == EventType.MouseDown && cell.Contains(Event.current.mousePosition))
            {
                inspected = textureId;

                if (isSelected)
                    selected.Remove(textureId);
                else
                    selected.Add(textureId);

                Event.current.Use();
            }
        }

        private void DrawInspector(RCEngineFile engine)
        {
            if (inspected < 0 || inspected >= engine.textures.Count)
            {
                EditorGUILayout.LabelField("Click a texture to inspect it. Clicking also toggles selection.",
                    EditorStyles.miniLabel);
                return;
            }

            RCTextureHeader header = engine.textures[inspected];
            string format = header.compressionFormat == RCTextureDecoder.FormatBC1 ? "DXT1 / BC1"
                : header.compressionFormat == RCTextureDecoder.FormatBC2 ? "DXT3 / BC2"
                : header.compressionFormat == RCTextureDecoder.FormatBC3 ? "DXT5 / BC3"
                : "unknown (0x" + header.compressionFormat.ToString("X2") + ")";

            EditorGUILayout.LabelField(string.Format(
                "Texture {0}   ·   {1}x{2}   ·   {3}   ·   {4} mip(s)   ·   {5} shown, {6} selected",
                inspected, header.width, header.height, format, header.mipMapCount, visible.Count, selected.Count),
                EditorStyles.miniLabel);
        }

        // ------------------------------------------------------------- decoding

        private void DecodePending(RCEngineFile engine, Stream vram, Action repaint)
        {
            if (pending.Count == 0 || Event.current.type != EventType.Repaint)
                return;

            int budget = Mathf.Min(DecodesPerRepaint, pending.Count);

            for (int i = 0; i < budget; i++)
            {
                int textureId = pending[0];
                pending.RemoveAt(0);

                if (thumbnails.ContainsKey(textureId))
                    continue;

                thumbnails[textureId] = DecodeThumbnail(engine, textureId, vram);
            }

            repaint();
        }

        private static Texture2D DecodeThumbnail(RCEngineFile engine, int textureId, Stream vram)
        {
            if (vram == null)
                return null;

            bool hasAlpha;
            Texture2D full = RCMobyFactory.DecodeTexture(engine, textureId, vram, out hasAlpha, null);
            if (full == null)
                return null;

            if (full.width <= ThumbnailLimit && full.height <= ThumbnailLimit)
            {
                full.hideFlags = HideFlags.HideAndDontSave;
                return full;
            }

            // Downscale and drop the full-resolution copy; only the thumbnail is kept.
            float ratio = Mathf.Min(ThumbnailLimit / (float) full.width, ThumbnailLimit / (float) full.height);
            int width = Mathf.Max(1, Mathf.RoundToInt(full.width * ratio));
            int height = Mathf.Max(1, Mathf.RoundToInt(full.height * ratio));

            var scaled = new Texture2D(width, height, TextureFormat.RGBA32, false);
            scaled.hideFlags = HideFlags.HideAndDontSave;

            var pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    pixels[y * width + x] = full.GetPixelBilinear((x + 0.5f) / width, (y + 0.5f) / height);
            }

            scaled.SetPixels(pixels);
            scaled.Apply();

            UnityEngine.Object.DestroyImmediate(full);
            return scaled;
        }

        // -------------------------------------------------------------- export

        private void Export(RCEngineFile engine, Stream vram, RCImportSettings settings)
        {
            string levelFolder = RCMobyBuilder.LevelFolder(engine, settings.outputFolder);
            var warnings = new List<string>();
            var ids = new List<int>(selected);
            ids.Sort();

            int exported = 0;
            string lastPath = null;

            try
            {
                for (int i = 0; i < ids.Count; i++)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Exporting textures",
                            "Texture " + ids[i], (float) i / ids.Count))
                        break;

                    bool hasAlpha;
                    Texture2D asset = RCMobyBuilder.ImportTexture(engine, ids[i], vram, levelFolder, warnings, out hasAlpha);

                    if (asset != null)
                    {
                        exported++;
                        lastPath = AssetDatabase.GetAssetPath(asset);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            for (int i = 0; i < warnings.Count; i++)
                Debug.LogWarning("[Moby Importer] " + warnings[i]);

            if (lastPath != null)
            {
                var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(lastPath);
                if (asset != null)
                    EditorGUIUtility.PingObject(asset);
            }

            status = string.Format("Exported {0} of {1} texture(s) to {2}/Textures.{3}",
                exported, ids.Count, levelFolder,
                warnings.Count > 0 ? " " + warnings.Count + " warning(s) in the console." : "");
            statusType = (exported == ids.Count) ? MessageType.Info : MessageType.Warning;
        }

        // ------------------------------------------------------------ utilities

        private void DrawCheckerboard(Rect rect)
        {
            if (checkerboard == null)
            {
                checkerboard = new Texture2D(16, 16, TextureFormat.RGBA32, false);
                checkerboard.hideFlags = HideFlags.HideAndDontSave;
                checkerboard.wrapMode = TextureWrapMode.Repeat;
                checkerboard.filterMode = FilterMode.Point;

                var light = new Color32(78, 78, 78, 255);
                var dark = new Color32(58, 58, 58, 255);
                var pixels = new Color32[16 * 16];

                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                        pixels[y * 16 + x] = ((x < 8) ^ (y < 8)) ? light : dark;
                }

                checkerboard.SetPixels32(pixels);
                checkerboard.Apply();
            }

            GUI.DrawTextureWithTexCoords(rect, checkerboard, new Rect(0f, 0f, rect.width / 16f, rect.height / 16f));
        }

        /// <summary>Drops every cached thumbnail; call when the loaded level changes.</summary>
        public void Reset()
        {
            foreach (Texture2D thumbnail in thumbnails.Values)
            {
                if (thumbnail != null)
                    UnityEngine.Object.DestroyImmediate(thumbnail);
            }

            thumbnails.Clear();
            pending.Clear();
            visible.Clear();
            selected.Clear();
            inspected = -1;
            status = "";

            if (checkerboard != null)
            {
                UnityEngine.Object.DestroyImmediate(checkerboard);
                checkerboard = null;
            }
        }
    }
}
