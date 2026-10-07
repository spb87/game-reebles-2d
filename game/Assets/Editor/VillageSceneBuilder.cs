// Village scene generator: builds the Village scene and Player prefab from the
// promoted art in Assets/Art (Sprites/ + Backdrops/). A tiny procedural white
// sprite is still generated under Assets/Art/Greybox for the mobile UI controls.
// Run via:
//   Unity.exe -batchmode -nographics -projectPath game \
//     -executeMethod Reebles2D.Editor.VillageSceneBuilder.Build -quit

using System.IO;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Reebles2D.Editor
{
    /// <summary>
    /// Single entry point that regenerates the village: sprite import settings for
    /// Assets/Art assets, Assets/Prefabs/Player.prefab, and Assets/Scenes/Village.unity.
    /// Idempotent — re-running overwrites all outputs cleanly.
    /// </summary>
    public static class VillageSceneBuilder
    {
        private const string SpritesFolder = "Assets/Art/Sprites";
        private const string BackdropsFolder = "Assets/Art/Backdrops";
        private const string GreyboxFolder = "Assets/Art/Greybox";
        private const string PrefabFolder = "Assets/Prefabs";
        private const string SceneFolder = "Assets/Scenes";
        private const string ScenePath = SceneFolder + "/Village.unity";
        private const string PlayerPrefabPath = PrefabFolder + "/Player.prefab";
        private const string InputActionsPath = "Assets/Input/ReeblesInput.inputactions";

        private const int UiSpritePixels = 16;

        // All promoted sprites are 1024x1024. PPU chosen so the art lands at
        // sensible world sizes; transforms below fine-tune the final footprint.
        private const float BuildingPpu = 250f;   // ~4.1 units per building
        private const float FountainPpu = 400f;   // ~2.6 units
        private const float FencePpu = 256f;      // ~4 units per tile
        private const float LanternPpu = 512f;    // ~2 units
        private const float GroundPpu = 256f;     // backdrop is stretched to map bounds; PPU only sets import scale
        private const float ReeblePpu = 512f;     // ~341px sheet cell -> ~0.67 units
        private const float SingleReeblePpu = 768f; // ~803px single sprite -> ~1 unit

        // Colliders cover only the base of each sprite (doors/water/fence rails),
        // not the full artwork, so the player can walk in front of roofs.
        private const float FootprintWidthFraction = 0.7f;
        private const float FootprintHeightFraction = 0.3f;
        private const float FountainFootprintFraction = 0.35f;

        private static readonly Rect MapBounds = new Rect(-15f, -10f, 30f, 20f);
        private const float FenceThickness = 0.3f;
        private const float FenceSegmentHeight = 1f;

        private const float ControlEdgeOffset = 160f;
        private const float ControlAreaSize = 220f;
        private const float ControlVisualSize = 120f;
        private const float StickMovementRange = 60f;
        private static readonly Color ControlAreaColor = new Color(1f, 1f, 1f, 0.12f);
        private static readonly Color ControlVisualColor = new Color(1f, 1f, 1f, 0.45f);

        /// <summary>Regenerates the Village scene and Player prefab from promoted art.</summary>
        public static void Build()
        {
            EnsureFolder("Assets", "Art");
            EnsureFolder("Assets/Art", "Greybox");
            EnsureFolder("Assets", "Prefabs");
            EnsureFolder("Assets", "Scenes");

            Sprite groundSprite = EnsureSpriteImport(
                BackdropsFolder + "/village_ground.jpg", GroundPpu, alpha: false);
            Sprite bakerySprite = EnsureSpriteImport(
                SpritesFolder + "/building_bakery.png", BuildingPpu);
            Sprite smithySprite = EnsureSpriteImport(
                SpritesFolder + "/building_smithy.png", BuildingPpu);
            Sprite herbalistSprite = EnsureSpriteImport(
                SpritesFolder + "/building_herbalist.png", BuildingPpu);
            Sprite storeSprite = EnsureSpriteImport(
                SpritesFolder + "/building_store.png", BuildingPpu);
            Sprite innSprite = EnsureSpriteImport(
                SpritesFolder + "/building_inn.png", BuildingPpu);
            Sprite fountainSprite = EnsureSpriteImport(
                SpritesFolder + "/fountain.png", FountainPpu);
            Sprite fenceSprite = EnsureSpriteImport(
                SpritesFolder + "/fence.png", FencePpu);
            Sprite lanternSprite = EnsureSpriteImport(
                SpritesFolder + "/lantern.png", LanternPpu);
            Sprite reebleSprite;
            string singleReeblePath = SpritesFolder + "/reeble.png";
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(singleReeblePath) != null)
            {
                // Single on-model sprite from the REEB-142 style re-roll; the
                // sheet is still sliced below but left unused by the prefab.
                reebleSprite = EnsureSpriteImport(singleReeblePath, SingleReeblePpu);
                EnsureReebleSheetImport(SpritesFolder + "/reeble_sheet.png", ReeblePpu);
            }
            else
            {
                reebleSprite = EnsureReebleSheetImport(
                    SpritesFolder + "/reeble_sheet.png", ReeblePpu);
            }

            Sprite uiSprite = CreateUiRectSprite("UI_Rect");

            GameObject playerPrefab = BuildPlayerPrefab(reebleSprite);

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Village";

            // Single stretched backdrop — the art is a full-map painting, not a
            // tile, so Tiled drawMode would repeat the whole scene.
            CreateScaledSprite("Ground", groundSprite, MapBounds.center,
                new Vector2(MapBounds.width, MapBounds.height), sortingOrder: -10);

            CreateBuilding("Bakery", bakerySprite, new Vector2(-8f, 5f), new Vector2(4f, 4f));
            CreateBuilding("Smithy", smithySprite, new Vector2(-8f, -5f), new Vector2(4f, 4f));
            CreateBuilding("Herbalist", herbalistSprite, new Vector2(8f, 5f), new Vector2(4f, 4f));
            CreateBuilding("Store", storeSprite, new Vector2(8f, -5f), new Vector2(4f, 4f));
            CreateBuilding("Inn", innSprite, new Vector2(0f, 7f), new Vector2(6f, 4f));

            GameObject fountain = CreateScaledSprite("Fountain", fountainSprite,
                Vector3.zero, Vector2.one * 2.5f, sortingOrder: 0);
            CircleCollider2D fountainCollider = fountain.AddComponent<CircleCollider2D>();
            float fountainLocalRadius = fountainSprite.bounds.extents.x;
            fountainCollider.radius = fountainLocalRadius * FountainFootprintFraction;
            fountainCollider.offset = new Vector2(0f, -fountainLocalRadius * 0.4f);

            CreateLantern(lanternSprite, new Vector2(-6.6f, 3.2f));
            CreateLantern(lanternSprite, new Vector2(9.4f, 3.2f));

            BuildFence(fenceSprite);

            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
            player.transform.position = new Vector3(0f, -3f, 0f);

            GameObject boundsObject = new GameObject("CameraBounds");
            BoxCollider2D boundsCollider = boundsObject.AddComponent<BoxCollider2D>();
            boundsCollider.isTrigger = true;
            boundsCollider.size = new Vector2(MapBounds.width, MapBounds.height);
            boundsObject.transform.position = MapBounds.center;

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            GameObject vcamObject = new GameObject("PlayerCamera");
            CinemachineCamera vcam = vcamObject.AddComponent<CinemachineCamera>();
            vcam.Follow = player.transform;
            vcam.Lens.OrthographicSize = 5f;
            CinemachineConfiner2D confiner = vcamObject.AddComponent<CinemachineConfiner2D>();
            confiner.BoundingShape2D = boundsCollider;

            BuildMobileControls(uiSprite);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("VillageSceneBuilder: Village.unity generated at " + ScenePath);
        }

        private static void EnsureFolder(string parent, string name)
        {
            string path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        /// <summary>
        /// Applies sprite import settings to a promoted art asset and returns its Sprite.
        /// </summary>
        private static Sprite EnsureSpriteImport(string path, float pixelsPerUnit, bool alpha = true)
        {
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null)
            {
                throw new System.InvalidOperationException("No texture importer at " + path);
            }
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = alpha;
            importer.SaveAndReimport();

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new System.InvalidOperationException("Failed to load sprite: " + path);
            }
            return sprite;
        }

        /// <summary>
        /// Slices the 3x3 reeble sheet into nine sprites and returns the center
        /// (front-facing) cell for the player prefab. Cells are 1024/3 ≈ 341.33px —
        /// SpriteRect accepts fractional rects. Existing spriteIDs are reused by
        /// name so the prefab's slice reference stays stable across re-slices.
        /// </summary>
        private static Sprite EnsureReebleSheetImport(string path, float pixelsPerUnit)
        {
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null)
            {
                throw new System.InvalidOperationException("No texture importer at " + path);
            }
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;

            const int columns = 3;
            const int rows = 3;
            const int sheetPixels = 1024;
            float cellWidth = sheetPixels / (float)columns;
            float cellHeight = sheetPixels / (float)rows;
            SpriteDataProviderFactories factories = new SpriteDataProviderFactories();
            factories.Init();
            ISpriteEditorDataProvider dataProvider =
                factories.GetSpriteEditorDataProviderFromObject(importer)
                    as ISpriteEditorDataProvider;
            if (dataProvider == null)
            {
                throw new System.InvalidOperationException(
                    "No sprite data provider for " + path);
            }
            dataProvider.InitSpriteEditorDataProvider();

            // Reuse existing spriteIDs so the prefab's slice reference stays stable.
            System.Collections.Generic.Dictionary<string, GUID> existingIds =
                new System.Collections.Generic.Dictionary<string, GUID>();
            System.Collections.Generic.Dictionary<string, Rect> existingRects =
                new System.Collections.Generic.Dictionary<string, Rect>();
            try
            {
                foreach (SpriteRect existing in dataProvider.GetSpriteRects())
                {
                    existingIds[existing.name] = existing.spriteID;
                    existingRects[existing.name] = existing.rect;
                }
            }
            catch (System.ArgumentNullException)
            {
                // Texture has no spritesheet yet — nothing to reuse.
            }

            SpriteRect[] sheet = new SpriteRect[columns * rows];
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    int index = (rows - 1 - row) * columns + column;
                    string cellName = "reeble_" + index;
                    sheet[index] = new SpriteRect
                    {
                        name = cellName,
                        spriteID = existingIds.TryGetValue(cellName, out GUID id)
                            ? id : GUID.Generate(),
                        rect = new Rect(column * cellWidth, row * cellHeight, cellWidth, cellHeight),
                        pivot = new Vector2(0.5f, 0.5f),
                        alignment = SpriteAlignment.Center,
                    };
                }
            }
            bool matches = existingRects.Count == sheet.Length;
            if (matches)
            {
                foreach (SpriteRect cell in sheet)
                {
                    if (!existingRects.TryGetValue(cell.name, out Rect r)
                        || !Mathf.Approximately(r.x, cell.rect.x)
                        || !Mathf.Approximately(r.y, cell.rect.y)
                        || !Mathf.Approximately(r.width, cell.rect.width)
                        || !Mathf.Approximately(r.height, cell.rect.height))
                    {
                        matches = false;
                        break;
                    }
                }
            }
            if (!matches)
            {
                dataProvider.SetSpriteRects(sheet);
                dataProvider.Apply();
                importer.SaveAndReimport();
            }

            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Sprite sprite && sprite.name == "reeble_4")
                {
                    return sprite;
                }
            }
            throw new System.InvalidOperationException("Failed to load reeble_4 slice: " + path);
        }

        /// <summary>
        /// Regenerates the small white sprite used by the mobile UI controls. Kept
        /// procedural because tinted UI chrome has no promoted art.
        /// </summary>
        private static Sprite CreateUiRectSprite(string name)
        {
            string path = GreyboxFolder + "/" + name + ".png";
            Texture2D texture = new Texture2D(UiSpritePixels, UiSpritePixels,
                TextureFormat.RGBA32, false);
            Color[] pixels = new Color[UiSpritePixels * UiSpritePixels];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = UiSpritePixels;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new System.InvalidOperationException("Failed to load UI sprite: " + path);
            }
            return sprite;
        }

        private static GameObject CreateScaledSprite(string name, Sprite sprite,
            Vector3 position, Vector2 size, int sortingOrder)
        {
            GameObject obj = new GameObject(name);
            obj.transform.position = position;
            SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            Vector2 spriteSize = sprite.bounds.size;
            obj.transform.localScale = new Vector3(
                size.x / spriteSize.x, size.y / spriteSize.y, 1f);
            return obj;
        }

        private static void CreateBuilding(string name, Sprite sprite,
            Vector2 position, Vector2 size)
        {
            GameObject building = CreateScaledSprite(name, sprite, position, size, sortingOrder: 0);
            Vector2 spriteSize = sprite.bounds.size;
            BoxCollider2D collider = building.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(
                spriteSize.x * FootprintWidthFraction,
                spriteSize.y * FootprintHeightFraction);
            collider.offset = new Vector2(0f,
                -spriteSize.y * (0.5f - FootprintHeightFraction * 0.5f));
        }

        private static void CreateLantern(Sprite sprite, Vector2 position)
        {
            CreateScaledSprite("Lantern", sprite, position, Vector2.one * 1f, sortingOrder: 1);
        }

        /// <summary>
        /// Rings the map with discrete fence-segment sprites scaled to
        /// <see cref="FenceSegmentHeight"/> tall (horizontal art is reused rotated
        /// 90 degrees on the vertical edges). Collision stays on four thin
        /// strips so segment spacing cannot open gaps.
        /// </summary>
        private static void BuildFence(Sprite fenceSprite)
        {
            GameObject fenceRoot = new GameObject("Fence");
            float halfWidth = MapBounds.width * 0.5f;
            float halfHeight = MapBounds.height * 0.5f;

            Vector2 spriteSize = fenceSprite.bounds.size;
            float segmentScale = FenceSegmentHeight / spriteSize.y;
            float segmentWidth = spriteSize.x * segmentScale;

            LayFenceEdge(fenceRoot.transform, fenceSprite, "FenceNorth",
                new Vector2(-halfWidth, halfHeight), Vector2.right,
                MapBounds.width, segmentWidth, segmentScale, rotateVertical: false);
            LayFenceEdge(fenceRoot.transform, fenceSprite, "FenceSouth",
                new Vector2(-halfWidth, -halfHeight), Vector2.right,
                MapBounds.width, segmentWidth, segmentScale, rotateVertical: false);
            LayFenceEdge(fenceRoot.transform, fenceSprite, "FenceWest",
                new Vector2(-halfWidth, -halfHeight), Vector2.up,
                MapBounds.height, segmentWidth, segmentScale, rotateVertical: true);
            LayFenceEdge(fenceRoot.transform, fenceSprite, "FenceEast",
                new Vector2(halfWidth, -halfHeight), Vector2.up,
                MapBounds.height, segmentWidth, segmentScale, rotateVertical: true);

            CreateFenceCollider(fenceRoot.transform, "FenceNorthCollider",
                new Vector2(MapBounds.center.x, halfHeight),
                new Vector2(MapBounds.width + FenceThickness, FenceThickness));
            CreateFenceCollider(fenceRoot.transform, "FenceSouthCollider",
                new Vector2(MapBounds.center.x, -halfHeight),
                new Vector2(MapBounds.width + FenceThickness, FenceThickness));
            CreateFenceCollider(fenceRoot.transform, "FenceWestCollider",
                new Vector2(-halfWidth, MapBounds.center.y),
                new Vector2(FenceThickness, MapBounds.height + FenceThickness));
            CreateFenceCollider(fenceRoot.transform, "FenceEastCollider",
                new Vector2(halfWidth, MapBounds.center.y),
                new Vector2(FenceThickness, MapBounds.height + FenceThickness));
        }

        /// <summary>
        /// Places whole fence segments end to end from <paramref name="start"/>
        /// along <paramref name="direction"/>. Vertical runs reuse the same art
        /// rotated upright so the pickets stay vertical.
        /// </summary>
        private static void LayFenceEdge(Transform parent, Sprite fenceSprite,
            string name, Vector2 start, Vector2 direction, float length,
            float segmentWidth, float segmentScale, bool rotateVertical)
        {
            int count = Mathf.Max(1, Mathf.CeilToInt(length / segmentWidth));
            float step = length / count;
            float angle = rotateVertical ? 90f : 0f;
            for (int i = 0; i < count; i++)
            {
                GameObject segment = new GameObject(name + "_" + i);
                segment.transform.SetParent(parent);
                segment.transform.position = start + direction * (step * (i + 0.5f));
                segment.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                segment.transform.localScale = Vector3.one * segmentScale;
                SpriteRenderer renderer = segment.AddComponent<SpriteRenderer>();
                renderer.sprite = fenceSprite;
                renderer.sortingOrder = 0;
            }
        }

        private static void CreateFenceCollider(Transform parent, string name,
            Vector2 center, Vector2 size)
        {
            GameObject strip = new GameObject(name);
            strip.transform.SetParent(parent);
            strip.transform.position = center;
            BoxCollider2D collider = strip.AddComponent<BoxCollider2D>();
            collider.size = size;
        }

        private static void BuildMobileControls(Sprite sprite)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>();

            GameObject canvasObject = new GameObject("MobileControls");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject root = new GameObject("ControlsRoot");
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.SetParent(canvasObject.transform, false);
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            CreateStickArea(root.transform, sprite, "MoveStickArea",
                new Vector2(0f, 0f), new Vector2(ControlEdgeOffset, ControlEdgeOffset));
            CreateButtonArea(root.transform, sprite, "InteractButton",
                new Vector2(1f, 0f), new Vector2(-ControlEdgeOffset, ControlEdgeOffset));

            UI.MobileControlsHud hud = canvasObject.AddComponent<UI.MobileControlsHud>();
            SerializedObject serializedHud = new SerializedObject(hud);
            serializedHud.FindProperty("controlsRoot").objectReferenceValue = root;
            serializedHud.FindProperty("safeAreaTarget").objectReferenceValue = rootRect;
            serializedHud.ApplyModifiedPropertiesWithoutUndo();
        }

        private static RectTransform CreateControlArea(Transform parent, string name,
            Sprite sprite, Vector2 anchor, Vector2 anchoredPosition)
        {
            GameObject area = new GameObject(name);
            RectTransform rect = area.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = Vector2.one * ControlAreaSize;
            Image background = area.AddComponent<Image>();
            background.sprite = sprite;
            background.color = ControlAreaColor;
            return rect;
        }

        private static RectTransform CreateControlVisual(Transform parent, string name, Sprite sprite)
        {
            GameObject visual = new GameObject(name);
            RectTransform rect = visual.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.one * ControlVisualSize;
            Image image = visual.AddComponent<Image>();
            image.sprite = sprite;
            image.color = ControlVisualColor;
            return rect;
        }

        private static void CreateStickArea(Transform parent, Sprite sprite, string name,
            Vector2 anchor, Vector2 anchoredPosition)
        {
            RectTransform area = CreateControlArea(parent, name, sprite, anchor, anchoredPosition);
            RectTransform knob = CreateControlVisual(area, "MoveStick", sprite);
            OnScreenStick stick = knob.gameObject.AddComponent<OnScreenStick>();
            stick.controlPath = "<Gamepad>/leftStick";
            stick.movementRange = StickMovementRange;
            stick.useIsolatedInputActions = true;
        }

        private static void CreateButtonArea(Transform parent, Sprite sprite, string name,
            Vector2 anchor, Vector2 anchoredPosition)
        {
            RectTransform area = CreateControlArea(parent, name, sprite, anchor, anchoredPosition);
            OnScreenButton button = area.gameObject.AddComponent<OnScreenButton>();
            button.controlPath = "<Gamepad>/buttonEast";
        }

        private static GameObject BuildPlayerPrefab(Sprite sprite)
        {
            GameObject player = new GameObject("Player");
            player.transform.localScale = Vector3.one * 0.8f;
            SpriteRenderer renderer = player.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 1;
            player.AddComponent<Rigidbody2D>();
            player.AddComponent<CircleCollider2D>();
            Player.PlayerMovement movement = player.AddComponent<Player.PlayerMovement>();

            InputActionAsset inputActions =
                AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (inputActions == null)
            {
                throw new System.InvalidOperationException(
                    "Input asset not found at " + InputActionsPath);
            }
            SerializedObject serialized = new SerializedObject(movement);
            serialized.FindProperty("inputActions").objectReferenceValue = inputActions;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(player, PlayerPrefabPath);
            Object.DestroyImmediate(player);
            return prefab;
        }
    }
}
