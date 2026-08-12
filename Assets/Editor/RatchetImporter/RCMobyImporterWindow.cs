using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RatchetImport
{
    /// <summary>
    /// Browses the moby table of a level's engine.ps3, previews the models with their
    /// animations, and imports the selected ones into the project.
    /// </summary>
    public sealed class RCMobyImporterWindow : EditorWindow
    {
        private const string EnginePathKey = "RatchetImport.EnginePath";
        private const string OutputFolderKey = "RatchetImport.OutputFolder";
        private const string ImportScaleKey = "RatchetImport.ImportScale";

        private const float ThumbnailSize = 40f;
        private const float PlainRowHeight = 20f;
        private const float ThumbnailRowHeight = ThumbnailSize + 4f;

        private sealed class MobyRow
        {
            public RCMobyEntry entry;
            public int vertexCount;
            public int triangleCount;
            public int chromeTriangleCount;
            public int boneCount;
            public int animationCount;
            public string error;
            public bool selected;
        }

        private string enginePath = "";
        private RCEngineFile engine;
        private FileStream vram;
        private readonly List<MobyRow> rows = new List<MobyRow>();
        private readonly List<MobyRow> filtered = new List<MobyRow>();

        private readonly RCImportSettings settings = new RCImportSettings();

        private string filter = "";
        private bool onlySkinned;
        private bool onlyAnimated;
        private bool showThumbnails = true;
        private Vector2 listScroll;
        private float listWidth = 330f;
        private string status = "";
        private MessageType statusType = MessageType.Info;

        private static readonly string[] TabNames = { "Mobies", "Textures" };
        private int tab;
        private readonly RCTextureBrowser textureBrowser = new RCTextureBrowser();

        // Previewing
        private MobyRow previewRow;
        private RCMobyPreview preview;
        private double lastUpdate;
        private float playbackSpeed = 1f;
        private bool showBackfaces;
        private Vector2 settingsScroll;

        // Shared across every preview and thumbnail built from the loaded level.
        private readonly RCPreviewCache previewCache = new RCPreviewCache();
        private readonly Dictionary<short, Texture2D> thumbnails = new Dictionary<short, Texture2D>();
        private readonly List<MobyRow> thumbnailQueue = new List<MobyRow>();

        [MenuItem("Tools/Ratchet & Clank 3/Moby Importer")]
        public static void Open()
        {
            var window = GetWindow<RCMobyImporterWindow>("Moby Importer");
            window.minSize = new Vector2(860f, 520f);
            window.Show();
        }

        private void OnEnable()
        {
            enginePath = EditorPrefs.GetString(EnginePathKey, "");
            settings.outputFolder = EditorPrefs.GetString(OutputFolderKey, settings.outputFolder);
            settings.importScale = EditorPrefs.GetFloat(ImportScaleKey, settings.importScale);
            lastUpdate = EditorApplication.timeSinceStartup;
        }

        private void OnDisable()
        {
            ReleaseLevel();
        }

        private void Update()
        {
            double now = EditorApplication.timeSinceStartup;
            float delta = (float) (now - lastUpdate);
            lastUpdate = now;

            if (preview != null && preview.playing && preview.animationSlot >= 0 && preview.CurrentLength > 0f)
            {
                preview.time += delta * playbackSpeed;
                preview.ApplyPose();
                Repaint();
            }
            else if (thumbnailQueue.Count > 0)
            {
                Repaint();
            }
        }

        private void OnGUI()
        {
            DrawSource();

            if (engine == null)
            {
                EditorGUILayout.HelpBox(
                    "Pick the engine.ps3 of an extracted level folder. vram.ps3 must sit next to it for textures to load.",
                    MessageType.Info);
                DrawStatus();
                return;
            }

            tab = GUILayout.Toolbar(tab, TabNames);

            if (tab == 1)
            {
                textureBrowser.Draw(engine, vram, settings, Repaint);
                DrawSettings();

                if (!string.IsNullOrEmpty(textureBrowser.status))
                    EditorGUILayout.HelpBox(textureBrowser.status, textureBrowser.statusType);

                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(listWidth)))
                {
                    DrawFilters();

                    // Above the list, not below it: the list expands to fill whatever
                    // room is left, so anything after it can be pushed off the bottom
                    // of the window where it cannot be reached.
                    DrawActions();
                    DrawList();
                }

                DrawSplitter();

                using (new EditorGUILayout.VerticalScope())
                {
                    DrawPreview();
                    DrawSettings();
                }
            }

            DrawStatus();
            ProcessThumbnailQueue();
        }

        // --------------------------------------------------------------- source

        private void DrawSource()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("engine.ps3", GUILayout.Width(70f));
                enginePath = EditorGUILayout.TextField(enginePath);

                if (GUILayout.Button("Browse", GUILayout.Width(64f)))
                {
                    string picked = EditorUtility.OpenFilePanel("Select engine.ps3", GuessStartFolder(), "ps3");
                    if (!string.IsNullOrEmpty(picked))
                    {
                        enginePath = picked;
                        GUI.FocusControl(null);
                        LoadEngine();
                    }
                }

                using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(enginePath)))
                {
                    if (GUILayout.Button("Load", GUILayout.Width(52f)))
                        LoadEngine();
                }
            }

            if (engine != null)
            {
                string vramState = (vram != null) ? "vram.ps3 loaded" : "vram.ps3 MISSING - no textures";
                EditorGUILayout.LabelField(string.Format("{0}   |   {1} mobies   |   {2} textures   |   {3}",
                    engine.game, rows.Count, engine.textures.Count, vramState), EditorStyles.miniLabel);
            }
        }

        // ----------------------------------------------------------------- list

        private void DrawFilters()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                filter = EditorGUILayout.TextField(filter, EditorStyles.toolbarSearchField);
                onlySkinned = GUILayout.Toggle(onlySkinned, "Rigged", EditorStyles.toolbarButton, GUILayout.Width(52f));
                onlyAnimated = GUILayout.Toggle(onlyAnimated, "Anim", EditorStyles.toolbarButton, GUILayout.Width(42f));

                bool wantThumbnails = GUILayout.Toggle(showThumbnails, "Thumbs", EditorStyles.toolbarButton, GUILayout.Width(52f));
                if (wantThumbnails != showThumbnails)
                {
                    showThumbnails = wantThumbnails;
                    thumbnailQueue.Clear();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("All", EditorStyles.miniButtonLeft))
                    SetSelection(true, true);
                if (GUILayout.Button("None", EditorStyles.miniButtonMid))
                    SetSelection(false, true);
                if (GUILayout.Button("Clear all", EditorStyles.miniButtonRight))
                    SetSelection(false, false);
            }
        }

        private void DrawList()
        {
            RebuildFilter();

            float rowHeight = showThumbnails ? ThumbnailRowHeight : PlainRowHeight;
            Rect area = GUILayoutUtility.GetRect(listWidth, 10000f, 60f, 10000f);
            GUI.Box(area, GUIContent.none, EditorStyles.helpBox);

            var content = new Rect(0f, 0f, area.width - 18f, filtered.Count * rowHeight);
            listScroll = GUI.BeginScrollView(area, listScroll, content);

            // Only the rows on screen are drawn; a level can hold hundreds of mobies.
            int first = Mathf.Max(0, Mathf.FloorToInt(listScroll.y / rowHeight) - 1);
            int last = Mathf.Min(filtered.Count - 1, Mathf.CeilToInt((listScroll.y + area.height) / rowHeight));

            for (int i = first; i <= last; i++)
                DrawRow(filtered[i], new Rect(0f, i * rowHeight, content.width, rowHeight), rowHeight);

            GUI.EndScrollView();

            if (filtered.Count == 0)
                EditorGUILayout.LabelField("Nothing matches the current filter.", EditorStyles.miniLabel);
        }

        private void DrawRow(MobyRow row, Rect rect, float rowHeight)
        {
            if (row == previewRow)
                EditorGUI.DrawRect(rect, new Color(0.24f, 0.37f, 0.59f, 0.55f));
            else if (row.selected)
                EditorGUI.DrawRect(rect, new Color(0.30f, 0.45f, 0.30f, 0.28f));

            var toggleRect = new Rect(rect.x + 3f, rect.y + (rowHeight - 16f) * 0.5f, 16f, 16f);
            row.selected = EditorGUI.Toggle(toggleRect, row.selected);

            float x = toggleRect.xMax + 4f;

            if (showThumbnails)
            {
                var thumbRect = new Rect(x, rect.y + 2f, ThumbnailSize, ThumbnailSize);
                Texture2D thumbnail;

                if (thumbnails.TryGetValue(row.entry.modelId, out thumbnail) && thumbnail != null)
                {
                    GUI.DrawTexture(thumbRect, thumbnail, ScaleMode.ScaleToFit);
                }
                else
                {
                    EditorGUI.DrawRect(thumbRect, new Color(0f, 0f, 0f, 0.25f));

                    if (row.error == null && !thumbnails.ContainsKey(row.entry.modelId) && !thumbnailQueue.Contains(row))
                        thumbnailQueue.Add(row);
                }

                x = thumbRect.xMax + 6f;
            }

            var labelRect = new Rect(x, rect.y + 1f, rect.width - x - 4f, 16f);
            EditorGUI.LabelField(labelRect, RCMobyBuilder.MobyName(row.entry.modelId), EditorStyles.boldLabel);

            var detailRect = new Rect(x, rect.y + (showThumbnails ? 17f : 1f), rect.width - x - 4f, 16f);
            string detail = (row.error != null)
                ? row.error
                : string.Format("{0} tris{1}  ·  {2} bones  ·  {3} anims", row.triangleCount,
                    row.chromeTriangleCount > 0 ? " +" + row.chromeTriangleCount + " chrome" : "",
                    row.boneCount, row.animationCount);

            if (showThumbnails || row.error != null)
                EditorGUI.LabelField(detailRect, detail, EditorStyles.miniLabel);
            else
                EditorGUI.LabelField(new Rect(x + 92f, rect.y + 1f, rect.width - x - 96f, 16f), detail, EditorStyles.miniLabel);

            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition)
                && !toggleRect.Contains(Event.current.mousePosition))
            {
                SetPreviewRow(row);
                Event.current.Use();
            }
        }

        private void RebuildFilter()
        {
            filtered.Clear();
            for (int i = 0; i < rows.Count; i++)
            {
                if (IsVisible(rows[i]))
                    filtered.Add(rows[i]);
            }
        }

        private bool IsVisible(MobyRow row)
        {
            if (onlySkinned && row.boneCount == 0)
                return false;

            if (onlyAnimated && row.animationCount == 0)
                return false;

            if (string.IsNullOrEmpty(filter))
                return true;

            // The name carries the hex id, so "1F3", "0x01f3" and "Moby_0x01F3" all
            // match. Decimal is still accepted for anyone who has an id in that form.
            return RCMobyBuilder.MobyName(row.entry.modelId).IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                   || row.entry.modelId.ToString().Contains(filter);
        }

        private void SetSelection(bool value, bool visibleOnly)
        {
            RebuildFilter();

            if (visibleOnly)
            {
                for (int i = 0; i < filtered.Count; i++)
                    filtered[i].selected = value;
            }
            else
            {
                for (int i = 0; i < rows.Count; i++)
                    rows[i].selected = value;
            }
        }

        private void DrawSplitter()
        {
            Rect rect = GUILayoutUtility.GetRect(4f, 4f, 60f, 10000f, GUILayout.Width(4f), GUILayout.ExpandHeight(true));
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.25f));
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);

            if (Event.current.type == EventType.MouseDrag && rect.Contains(Event.current.mousePosition))
            {
                listWidth = Mathf.Clamp(listWidth + Event.current.delta.x, 240f, position.width - 320f);
                Repaint();
                Event.current.Use();
            }
        }

        // -------------------------------------------------------------- preview

        private void DrawPreview()
        {
            if (previewRow == null)
            {
                Rect empty = GUILayoutUtility.GetRect(100f, 10000f, 120f, 10000f);
                GUI.Box(empty, "Select a moby to preview it", EditorStyles.helpBox);
                return;
            }

            DrawPreviewToolbar();

            Rect rect = GUILayoutUtility.GetRect(100f, 10000f, 120f, 10000f);

            if (preview == null)
            {
                GUI.Box(rect, "Could not build a preview for this moby.", EditorStyles.helpBox);
                return;
            }

            HandlePreviewInput(rect);
            preview.Draw(rect, GUIStyle.none);

            var overlay = new Rect(rect.x + 6f, rect.yMax - 20f, rect.width - 12f, 16f);
            GUI.Label(overlay, string.Format("{0}   ·   {1} tris   ·   {2} bones   ·   drag to orbit, scroll to zoom",
                RCMobyBuilder.MobyName(previewRow.entry.modelId), previewRow.triangleCount, previewRow.boneCount),
                EditorStyles.miniLabel);

            DrawScrubber();
        }

        private void DrawPreviewToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (preview != null && preview.AnimationCount > 0)
                {
                    // Slot 0 of the popup is the bind pose, so the animation slots are offset by one.
                    int popupIndex = preview.animationSlot + 1;
                    int newIndex = EditorGUILayout.Popup(popupIndex, preview.animationNames, EditorStyles.toolbarPopup, GUILayout.Width(230f));

                    if (newIndex != popupIndex)
                    {
                        preview.animationSlot = newIndex - 1;
                        preview.time = 0f;
                        preview.ApplyPose();
                    }

                    using (new EditorGUI.DisabledScope(preview.animationSlot < 0))
                    {
                        preview.playing = GUILayout.Toggle(preview.playing, preview.playing ? "Pause" : "Play",
                            EditorStyles.toolbarButton, GUILayout.Width(52f));

                        GUILayout.Label("Speed", EditorStyles.miniLabel, GUILayout.Width(40f));
                        playbackSpeed = GUILayout.HorizontalSlider(playbackSpeed, 0.1f, 2f, GUILayout.Width(70f));
                    }
                }
                else
                {
                    GUILayout.Label(preview != null && preview.HasSkeleton ? "No animations" : "Static model",
                        EditorStyles.miniLabel);
                }

                GUILayout.FlexibleSpace();

                // The model on screen is the one the user is thinking about, so give it
                // its own export rather than sending them off to find its checkbox.
                using (new EditorGUI.DisabledScope(previewRow == null || previewRow.error != null))
                {
                    Color previousBackground = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.40f, 0.72f, 0.42f);

                    if (GUILayout.Button("Export this moby", EditorStyles.toolbarButton, GUILayout.Width(112f)))
                        ExportRows(new List<MobyRow> { previewRow });

                    GUI.backgroundColor = previousBackground;
                }

                GUILayout.Space(8f);

                bool wantBackfaces = GUILayout.Toggle(showBackfaces, "Backfaces", EditorStyles.toolbarButton, GUILayout.Width(72f));
                if (wantBackfaces != showBackfaces)
                {
                    showBackfaces = wantBackfaces;
                    if (preview != null)
                        preview.showBackfaces = showBackfaces;
                }

                if (GUILayout.Button("Reset view", EditorStyles.toolbarButton, GUILayout.Width(78f)) && preview != null)
                    preview.ResetView();
            }
        }

        private void DrawScrubber()
        {
            if (preview == null || preview.animationSlot < 0)
                return;

            float length = preview.CurrentLength;
            if (length <= 0f)
                return;

            using (new EditorGUILayout.HorizontalScope())
            {
                float t = Mathf.Repeat(preview.time, length);
                float scrubbed = EditorGUILayout.Slider(t, 0f, length);

                if (!Mathf.Approximately(scrubbed, t))
                {
                    preview.time = scrubbed;
                    preview.playing = false;
                    preview.ApplyPose();
                }

                GUILayout.Label(string.Format("{0:0.00}s", length), EditorStyles.miniLabel, GUILayout.Width(48f));
            }
        }

        private void HandlePreviewInput(Rect rect)
        {
            Event e = Event.current;
            if (!rect.Contains(e.mousePosition))
                return;

            if (e.type == EventType.MouseDrag && e.button == 0)
            {
                preview.yaw -= e.delta.x * 0.6f;
                preview.pitch = Mathf.Clamp(preview.pitch + e.delta.y * 0.6f, -89f, 89f);
                Repaint();
                e.Use();
            }
            else if (e.type == EventType.ScrollWheel)
            {
                preview.zoom = Mathf.Clamp(preview.zoom * (1f - e.delta.y * 0.04f), 0.1f, 12f);
                Repaint();
                e.Use();
            }
        }

        private void SetPreviewRow(MobyRow row)
        {
            if (previewRow == row && preview != null)
                return;

            float yaw = 140f;
            float pitch = 12f;
            float zoom = 1f;

            if (preview != null)
            {
                // Keep the camera where the user put it when hopping between models.
                yaw = preview.yaw;
                pitch = preview.pitch;
                zoom = preview.zoom;
                preview.Dispose();
                preview = null;
            }

            previewRow = row;

            if (row == null || row.error != null)
                return;

            try
            {
                RCMobyModel model = engine.ReadMoby(row.entry);
                preview = RCMobyPreview.Create(engine, model, vram, settings.importScale, settings.importTextures,
                    settings.cutoutTransparency, settings.importChrome, previewCache);
                preview.showBackfaces = showBackfaces;
                preview.yaw = yaw;
                preview.pitch = pitch;
                preview.zoom = zoom;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Moby Importer] Preview failed for " + RCMobyBuilder.MobyName(row.entry.modelId) + ": " + e.Message);
                preview = null;
            }

            Repaint();
        }

        /// <summary>
        /// Builds at most one thumbnail per repaint so scrolling a large level stays
        /// responsive. Each one is a throwaway preview; the texture caches make the
        /// models after the first cheap.
        /// </summary>
        private void ProcessThumbnailQueue()
        {
            if (thumbnailQueue.Count == 0 || Event.current.type != EventType.Repaint)
                return;

            MobyRow row = thumbnailQueue[0];
            thumbnailQueue.RemoveAt(0);

            if (thumbnails.ContainsKey(row.entry.modelId))
                return;

            Texture2D thumbnail = null;

            try
            {
                RCMobyModel model = engine.ReadMoby(row.entry);
                using (RCMobyPreview thumbPreview = RCMobyPreview.Create(engine, model, vram, settings.importScale,
                    settings.importTextures, settings.cutoutTransparency, settings.importChrome, previewCache))
                {
                    thumbnail = thumbPreview.RenderThumbnail(64);
                    if (thumbnail != null)
                        thumbnail.hideFlags = HideFlags.HideAndDontSave;
                }
            }
            catch (Exception)
            {
                // A model that will not build gets a blank slot rather than retrying forever.
            }

            thumbnails[row.entry.modelId] = thumbnail;
            Repaint();
        }

        // -------------------------------------------------------------- settings

        private void DrawSettings()
        {
            // Fixed height with its own scrollbar. Left to size itself these rows
            // compete with the list and preview for space, and lose.
            EditorGUILayout.Space(2f);
            settingsScroll = EditorGUILayout.BeginScrollView(settingsScroll, GUILayout.Height(168f));
            DrawSettingsBody();
            EditorGUILayout.EndScrollView();
        }

        private void DrawSettingsBody()
        {
            EditorGUILayout.LabelField("Export settings", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                settings.outputFolder = EditorGUILayout.TextField("Output folder", settings.outputFolder);
                if (GUILayout.Button("Pick", GUILayout.Width(44f)))
                {
                    string picked = EditorUtility.OpenFolderPanel("Output folder", Application.dataPath, "");
                    if (!string.IsNullOrEmpty(picked) && picked.StartsWith(Application.dataPath))
                        settings.outputFolder = "Assets" + picked.Substring(Application.dataPath.Length).Replace('\\', '/');
                }
            }

            EditorGUI.BeginChangeCheck();
            settings.importScale = EditorGUILayout.FloatField(
                new GUIContent("Import scale", "Multiplier applied on top of the model's own size field."),
                settings.importScale);
            bool scaleChanged = EditorGUI.EndChangeCheck();

            settings.importAnimations = EditorGUILayout.Toggle("Import animations", settings.importAnimations);

            using (new EditorGUI.DisabledScope(!settings.importAnimations))
                settings.createAnimatorController = EditorGUILayout.Toggle("Create animator controller", settings.createAnimatorController);

            EditorGUI.BeginChangeCheck();
            settings.importTextures = EditorGUILayout.Toggle("Import textures", settings.importTextures);

            using (new EditorGUI.DisabledScope(!settings.importTextures))
                settings.cutoutTransparency = EditorGUILayout.Toggle("Alpha clip transparent textures", settings.cutoutTransparency);

            settings.importChrome = EditorGUILayout.Toggle(
                new GUIContent("Import chrome parts",
                    "The game's metal pass: extra geometry with no UVs, shaded from an environment map. Imported as a plain reflective material."),
                settings.importChrome);

            if (EditorGUI.EndChangeCheck() || scaleChanged)
                InvalidatePreviews();
        }

        /// <summary>Settings that change how a model looks invalidate everything already rendered.</summary>
        private void InvalidatePreviews()
        {
            MobyRow current = previewRow;

            // Drop the live preview before the materials it references go away.
            if (preview != null)
            {
                preview.Dispose();
                preview = null;
            }

            previewCache.DestroyMaterials();
            ClearThumbnails();
            thumbnailQueue.Clear();

            previewRow = null;
            SetPreviewRow(current);
        }

        private void DrawActions()
        {
            int selected = 0;
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].selected)
                    selected++;
            }

            // Kept enabled even with an empty selection. A greyed-out button reads as
            // a caption rather than a control, so it explains itself on click instead.
            Color previousBackground = GUI.backgroundColor;
            if (selected > 0)
                GUI.backgroundColor = new Color(0.40f, 0.72f, 0.42f);

            string label = (selected > 0)
                ? string.Format("Export {0} ticked moby(s)", selected)
                : "Export ticked mobies...";

            bool pressed = GUILayout.Button(label, GUILayout.Height(26f));
            GUI.backgroundColor = previousBackground;

            if (!pressed)
                return;

            if (selected == 0)
            {
                status = "Nothing is ticked. Use the checkbox on the left of each row, or the "
                         + "\"Export this moby\" button above the preview to export just the one you are looking at.";
                statusType = MessageType.Info;
                return;
            }

            ImportSelected();
        }

        private void DrawStatus()
        {
            if (!string.IsNullOrEmpty(status))
                EditorGUILayout.HelpBox(status, statusType);
        }

        // -------------------------------------------------------------- loading

        private string GuessStartFolder()
        {
            if (!string.IsNullOrEmpty(enginePath))
            {
                string dir = Path.GetDirectoryName(enginePath);
                if (dir != null && Directory.Exists(dir))
                    return dir;
            }

            return "";
        }

        private void ReleaseLevel()
        {
            if (preview != null)
            {
                preview.Dispose();
                preview = null;
            }

            previewRow = null;
            thumbnailQueue.Clear();
            ClearThumbnails();
            previewCache.DestroyAll();
            textureBrowser.Reset();

            if (vram != null)
            {
                vram.Dispose();
                vram = null;
            }
        }

        private void ClearThumbnails()
        {
            foreach (Texture2D thumbnail in thumbnails.Values)
            {
                if (thumbnail != null)
                    DestroyImmediate(thumbnail);
            }

            thumbnails.Clear();
        }

        private void LoadEngine()
        {
            ReleaseLevel();
            engine = null;
            rows.Clear();
            filtered.Clear();
            status = "";

            try
            {
                engine = RCEngineFile.Load(enginePath);
            }
            catch (Exception e)
            {
                status = "Could not read that file: " + e.Message;
                statusType = MessageType.Error;
                return;
            }

            EditorPrefs.SetString(EnginePathKey, enginePath);

            if (engine.vramPath != null)
            {
                try
                {
                    vram = new FileStream(engine.vramPath, FileMode.Open, FileAccess.Read);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[Moby Importer] Could not open vram.ps3: " + e.Message);
                }
            }

            if (engine.game == RCGame.Deadlocked)
            {
                status = "This looks like a Deadlocked level. Meshes and skeletons should load, but its animation format differs and is not supported.";
                statusType = MessageType.Warning;
            }
            else if (engine.game == RCGame.Unknown)
            {
                // Only picks the animation dialect, and the skeleton layout is detected
                // per model anyway, so an unrecognised build is not a problem in itself.
                Debug.Log("[Moby Importer] Unrecognised engine magic 0x" + engine.magic.ToString("X8")
                          + "; reading as Ratchet & Clank 1-3.");
            }

            try
            {
                for (int i = 0; i < engine.mobyEntries.Count; i++)
                {
                    RCMobyEntry entry = engine.mobyEntries[i];
                    if (!entry.HasModel)
                        continue;

                    if (EditorUtility.DisplayCancelableProgressBar("Reading moby table",
                            RCMobyBuilder.MobyName(entry.modelId), (float) i / engine.mobyEntries.Count))
                        break;

                    var row = new MobyRow { entry = entry };

                    try
                    {
                        RCMobyModel model = engine.ReadMoby(entry);
                        row.vertexCount = model.VertexCount;
                        row.triangleCount = model.TriangleCount;
                        row.chromeTriangleCount = model.metalIndices.Length / 3;
                        row.boneCount = model.HasSkeleton ? model.boneCount : 0;
                        row.animationCount = model.animations.Count;

                        if (model.VertexCount == 0)
                            row.error = "no geometry";
                    }
                    catch (Exception e)
                    {
                        row.error = "unreadable: " + e.Message;
                    }

                    rows.Add(row);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            if (string.IsNullOrEmpty(status))
            {
                status = string.Format("Loaded {0} moby models from {1}.", rows.Count,
                    Path.GetFileName(Path.GetDirectoryName(enginePath)));
                statusType = MessageType.Info;
            }
        }

        // ------------------------------------------------------------ importing

        private void ImportSelected()
        {
            var ticked = new List<MobyRow>();
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].selected)
                    ticked.Add(rows[i]);
            }

            ExportRows(ticked);
        }

        private void ExportRows(List<MobyRow> requested)
        {
            EditorPrefs.SetString(OutputFolderKey, settings.outputFolder);
            EditorPrefs.SetFloat(ImportScaleKey, settings.importScale);

            var queue = new List<MobyRow>();
            for (int i = 0; i < requested.Count; i++)
            {
                if (requested[i] != null && requested[i].error == null)
                    queue.Add(requested[i]);
            }

            if (queue.Count == 0)
            {
                status = "Nothing to export - the chosen mobies are unreadable or have no geometry.";
                statusType = MessageType.Warning;
                return;
            }

            var materialCache = new Dictionary<int, Material>();
            var failures = new List<string>();
            var warnings = new List<string>();
            int imported = 0;
            string lastPrefab = null;

            try
            {
                for (int i = 0; i < queue.Count; i++)
                {
                    MobyRow row = queue[i];
                    string name = RCMobyBuilder.MobyName(row.entry.modelId);

                    if (EditorUtility.DisplayCancelableProgressBar("Importing mobies", name, (float) i / queue.Count))
                        break;

                    try
                    {
                        RCMobyModel model = engine.ReadMoby(row.entry);
                        RCImportResult result = RCMobyBuilder.Import(engine, row.entry, model, settings,
                            settings.importTextures ? vram : null, materialCache);

                        imported++;
                        lastPrefab = result.prefabPath;

                        for (int w = 0; w < result.warnings.Count; w++)
                            warnings.Add(name + ": " + result.warnings[w]);
                    }
                    catch (Exception e)
                    {
                        failures.Add(name + ": " + e.Message);
                        Debug.LogError("[Moby Importer] " + name + " failed: " + e);
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

            if (lastPrefab != null)
            {
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(lastPrefab);
                if (asset != null)
                    EditorGUIUtility.PingObject(asset);
            }

            if (failures.Count > 0)
            {
                status = string.Format("Imported {0} of {1}. {2} failed - see the console.", imported, queue.Count, failures.Count);
                statusType = MessageType.Warning;
            }
            else
            {
                status = string.Format("Imported {0} moby(s) into {1}.{2}", imported, settings.outputFolder,
                    warnings.Count > 0 ? " " + warnings.Count + " warning(s) logged to the console." : "");
                statusType = MessageType.Info;
            }
        }
    }
}
