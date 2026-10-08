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
        private const string NpcPrefabPath = PrefabFolder + "/Npc.prefab";
        private const string InputActionsPath = "Assets/Input/ReeblesInput.inputactions";

        // Villager NPC (REEB-152): imported large like the props, then scaled to
        // a ~1.2x-reeble world height. The solid footprint circle blocks the
        // player; the Talk radius is generous so she is easy to approach.
        private const float NpcPpu = 512f;
        private const float NpcHeight = 1.2f;
        private const float NpcFootprintRadius = 0.35f;
        private const float NpcInteractRadius = 1.75f;
        private const float NpcShadowWidth = 0.8f;
        private const float NpcDoorOffset = 1.5f;

        private const int UiSpritePixels = 16;

        // Quest wiring (REEB-154): Marla offers/completes errand_berries;
        // every berry bush is a fetch source for its fetchTargetId.
        private const string QuestGiverNpcId = "marla_baker";
        private const string QuestId = "errand_berries";
        private const string QuestFetchTargetId = "berry_bush";

        // HUD canvas sits under the dialogue card (90) and controls (100).
        private const int HudCanvasSortingOrder = 80;
        private const float HudMargin = 24f;
        private const float HudToastBottom = 280f;
        private static readonly Color HudTextColor = new Color(0.98f, 0.96f, 1f, 1f);
        private static readonly Color HudHeartColor = new Color(1f, 0.55f, 0.65f, 1f);

        // All promoted sprites are 1024x1024. PPU chosen so the art lands at
        // sensible world sizes; transforms below fine-tune the final footprint.
        private const float BuildingPpu = 250f;   // ~4.1 units per building
        private const float FountainPpu = 400f;   // ~2.6 units
        private const float LanternPpu = 512f;    // ~2 units
        private const float PropPpu = 300f;       // ~3.4 units tall for 1024px prop art
        private const float GroundPpu = 256f;     // backdrop is stretched to map bounds; PPU only sets import scale
        private const float ReeblePpu = 512f;     // ~341px sheet cell -> ~0.67 units
        private const float SingleReeblePpu = 768f; // ~803px single sprite -> ~1 unit
        private const float DirectionalReeblePpu = 330f; // ~330px view sprites -> ~1 unit

        // Backdrop light comes from upper-right (path edges shade lower-left),
        // so cast shadows offset down-left of each object's base.
        private static readonly Vector2 ShadowOffset = new Vector2(-0.18f, -0.16f);
        private const int ShadowSortingOrder = -9;
        private const int ShadowTexturePixels = 128;
        private const float ShadowPpu = 128f;     // 1 unit wide at scale 1
        private static readonly Color ShadowColor = new Color(0.16f, 0.1f, 0.22f, 0.5f);

        // Colliders cover only the base of each sprite (doors/water/fence rails),
        // not the full artwork, so the player can walk in front of roofs.
        private const float FootprintWidthFraction = 0.7f;
        private const float FootprintHeightFraction = 0.3f;
        private const float FountainFootprintFraction = 0.35f;

        private static readonly Rect MapBounds = new Rect(-30f, -20f, 60f, 40f);

        // Treeline perimeter: trees sit ~1.2u inside the bounds; collision is a
        // separate thin wall strip on each edge so spacing cannot open gaps.
        private const float TreelineInset = 1.2f;
        private const float TreelineSpacing = 1.35f;
        private const float WallThickness = 0.3f;

        // Outskirts prop sizes (world height) and collider footprint.
        private const float TreeHeight = 3.2f;
        private const float BushHeight = 1.3f;
        private const float RockHeight = 1f;
        private const float FlowerPatchSize = 1f;
        private const float PropFootprintRadius = 0.3f;

        // Prop kinds scattered between the village core and the treeline.
        private enum OutskirtsProp { Oak, Pine, RoundTree, Bush, BerryBush, Rock, Flowers }

        // Hand-placed to stay out of the ~11u village core, the cardinal path
        // corridors (|x| < 2 / |y| < 2), and the treeline inset band.
        private static readonly (OutskirtsProp Kind, Vector2 Position)[] ScatterPlacements =
        {
            (OutskirtsProp.Oak, new Vector2(-22f, 10f)),
            (OutskirtsProp.Pine, new Vector2(-15f, -13f)),
            (OutskirtsProp.RoundTree, new Vector2(19f, 11f)),
            (OutskirtsProp.Oak, new Vector2(24f, -9f)),
            (OutskirtsProp.Pine, new Vector2(-25f, -4f)),
            (OutskirtsProp.RoundTree, new Vector2(13f, 15f)),
            (OutskirtsProp.Bush, new Vector2(-18f, 14f)),
            (OutskirtsProp.Bush, new Vector2(15f, -14f)),
            (OutskirtsProp.Bush, new Vector2(-19f, -11f)),
            (OutskirtsProp.Bush, new Vector2(23f, 5f)),
            (OutskirtsProp.Bush, new Vector2(11f, -16f)),
            (OutskirtsProp.BerryBush, new Vector2(-17f, 5f)),
            (OutskirtsProp.BerryBush, new Vector2(16f, -7f)),
            (OutskirtsProp.BerryBush, new Vector2(25f, 13f)),
            (OutskirtsProp.Rock, new Vector2(-11f, -16f)),
            (OutskirtsProp.Rock, new Vector2(26f, 3f)),
            (OutskirtsProp.Rock, new Vector2(6f, 17f)),
            (OutskirtsProp.Rock, new Vector2(-26f, 15f)),
            (OutskirtsProp.Flowers, new Vector2(-9f, 13f)),
            (OutskirtsProp.Flowers, new Vector2(10f, 13f)),
            (OutskirtsProp.Flowers, new Vector2(-21f, 2f)),
            (OutskirtsProp.Flowers, new Vector2(20f, -15f)),
            (OutskirtsProp.Flowers, new Vector2(-27f, -9f)),
        };

        private const float ControlEdgeOffset = 160f;
        private const float ControlAreaSize = 220f;
        private const float ControlVisualSize = 120f;
        private const float StickMovementRange = 60f;
        private static readonly Color ControlAreaColor = new Color(1f, 1f, 1f, 0.12f);
        private static readonly Color ControlVisualColor = new Color(1f, 1f, 1f, 0.45f);

        // Dialogue card: bottom-center panel on its own overlay canvas,
        // layered under the mobile controls canvas (sortingOrder 100).
        private const int DialogueCanvasSortingOrder = 90;
        private const int CardSpritePixels = 96;
        private const float CardCornerRadius = 20f;
        private const int CardSpriteBorder = 24;
        private static readonly Vector2 CardSize = new Vector2(900f, 220f);
        private const float CardBottomMargin = 24f;
        private const float CardPadding = 28f;
        private static readonly Color CardColor = new Color(0.08f, 0.06f, 0.12f, 0.92f);
        private static readonly Color CardNameColor = new Color(1f, 0.9f, 0.6f, 1f);
        private static readonly Color CardBodyColor = new Color(0.95f, 0.93f, 0.98f, 1f);
        private static readonly Color CardHintColor = new Color(1f, 1f, 1f, 0.55f);

        /// <summary>Regenerates the Village scene and Player prefab from promoted art.</summary>
        public static void Build()
        {
            EnsureFolder("Assets", "Art");
            EnsureFolder("Assets/Art", "Greybox");
            EnsureFolder("Assets", "Prefabs");
            EnsureFolder("Assets", "Scenes");

            Sprite groundSprite = EnsureSpriteImport(
                BackdropsFolder + "/world_terrain.jpg", GroundPpu, alpha: false);
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
            Sprite treeOakSprite = EnsureSpriteImport(
                SpritesFolder + "/tree_oak.png", PropPpu);
            Sprite treePineSprite = EnsureSpriteImport(
                SpritesFolder + "/tree_pine.png", PropPpu);
            Sprite treeRoundSprite = EnsureSpriteImport(
                SpritesFolder + "/tree_round.png", PropPpu);
            Sprite bushSprite = EnsureSpriteImport(
                SpritesFolder + "/bush.png", PropPpu);
            Sprite berryBushSprite = EnsureSpriteImport(
                SpritesFolder + "/bush_berry.png", PropPpu);
            Sprite rockSprite = EnsureSpriteImport(
                SpritesFolder + "/rock.png", PropPpu);
            Sprite flowersSprite = EnsureSpriteImport(
                SpritesFolder + "/flowers.png", PropPpu);
            Sprite lanternSprite = EnsureSpriteImport(
                SpritesFolder + "/lantern.png", LanternPpu);
            Sprite villagerSprite = EnsureSpriteImport(
                SpritesFolder + "/npc_villager.png", NpcPpu);
            Sprite reebleSprite;
            Sprite reebleFront = null;
            Sprite reebleBack = null;
            Sprite reebleLeft = null;
            string directionalFrontPath = SpritesFolder + "/reeble_front.png";
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(directionalFrontPath) != null)
            {
                // REEB-143 turnaround views supersede the single sprite.
                reebleFront = EnsureSpriteImport(directionalFrontPath, DirectionalReeblePpu);
                reebleBack = EnsureSpriteImport(
                    SpritesFolder + "/reeble_back.png", DirectionalReeblePpu);
                reebleLeft = EnsureSpriteImport(
                    SpritesFolder + "/reeble_left.png", DirectionalReeblePpu);
                // reeble_right.png was another front view, not a profile;
                // right-facing uses the left sprite flipped (rightSprite null).
                reebleSprite = reebleFront;
            }
            else
            {
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
            }

            Sprite uiSprite = CreateUiRectSprite("UI_Rect");
            Sprite shadowSprite = CreateShadowSprite("Shadow");
            Sprite promptSprite = CreatePromptSprite("PromptBubble");
            Sprite cardSprite = CreateCardSprite("DialogueCardPanel");

            GameObject playerPrefab = BuildPlayerPrefab(
                reebleSprite, reebleFront, reebleBack, reebleLeft, right: null,
                shadowSprite, promptSprite);
            GameObject npcPrefab = BuildNpcPrefab(villagerSprite, shadowSprite);

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Village";

            // Single stretched backdrop — the art is a full-map painting, not a
            // tile, so Tiled drawMode would repeat the whole scene.
            CreateScaledSprite("Ground", groundSprite, MapBounds.center,
                new Vector2(MapBounds.width, MapBounds.height), sortingOrder: -10);

            Vector2 bakeryPosition = new Vector2(-8f, 5f);
            Vector2 bakerySize = new Vector2(4f, 4f);
            CreateBuilding("Bakery", bakerySprite, bakeryPosition, bakerySize, shadowSprite);
            CreateBuilding("Smithy", smithySprite, new Vector2(-8f, -5f), new Vector2(4f, 4f), shadowSprite);
            CreateBuilding("Herbalist", herbalistSprite, new Vector2(8f, 5f), new Vector2(4f, 4f), shadowSprite);
            CreateBuilding("Store", storeSprite, new Vector2(8f, -5f), new Vector2(4f, 4f), shadowSprite);
            CreateBuilding("Inn", innSprite, new Vector2(0f, 7f), new Vector2(6f, 4f), shadowSprite);

            GameObject fountain = CreateScaledSprite("Fountain", fountainSprite,
                Vector3.zero, Vector2.one * 2.5f, sortingOrder: 0);
            CircleCollider2D fountainCollider = fountain.AddComponent<CircleCollider2D>();
            float fountainLocalRadius = fountainSprite.bounds.extents.x;
            fountainCollider.radius = fountainLocalRadius * FountainFootprintFraction;
            fountainCollider.offset = new Vector2(0f, -fountainLocalRadius * 0.4f);
            AddShadow(shadowSprite, Vector2.zero, 2.5f);

            // Flavor interactable — Admiring the fountain opens a one-line
            // dialogue card. The solid footprint collider doubles as the
            // OverlapCircleAll hit target.
            UI.NpcComponent fountainNpc = fountain.AddComponent<UI.NpcComponent>();
            SerializedObject serializedNpc = new SerializedObject(fountainNpc);
            serializedNpc.FindProperty("displayName").stringValue = "Fountain";
            SerializedProperty fountainLines = serializedNpc.FindProperty("lines");
            fountainLines.arraySize = 1;
            fountainLines.GetArrayElementAtIndex(0).stringValue =
                "The fountain burbles quietly in the square.";
            serializedNpc.ApplyModifiedPropertiesWithoutUndo();

            UI.NpcInteractable fountainInteract =
                fountain.AddComponent<UI.NpcInteractable>();
            SerializedObject serializedInteract = new SerializedObject(fountainInteract);
            serializedInteract.FindProperty("promptVerb").stringValue = "Admire";
            serializedInteract.FindProperty("interactRadius").floatValue = 2f;
            serializedInteract.FindProperty("promptHeight").floatValue = 1.4f;
            serializedInteract.FindProperty("npc").objectReferenceValue = fountainNpc;
            serializedInteract.ApplyModifiedPropertiesWithoutUndo();

            CreateLantern(lanternSprite, new Vector2(-6.6f, 3.2f), shadowSprite);
            CreateLantern(lanternSprite, new Vector2(9.4f, 3.2f), shadowSprite);

            BuildTreeline(treeOakSprite, treePineSprite, treeRoundSprite, shadowSprite);
            ScatterOutskirtsProps(shadowSprite, treeOakSprite, treePineSprite,
                treeRoundSprite, bushSprite, berryBushSprite, rockSprite, flowersSprite);

            // Marla stands just outside the bakery's front face, a short step
            // toward the plaza so she greets players crossing the square.
            Vector2 npcPosition = bakeryPosition
                - new Vector2(0f, bakerySize.y * 0.5f + NpcDoorOffset);
            GameObject npc = (GameObject)PrefabUtility.InstantiatePrefab(npcPrefab, scene);
            npc.transform.position = npcPosition;

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

            new GameObject("QuestService").AddComponent<Quests.QuestService>();

            BuildHud();
            BuildMobileControls(uiSprite);
            BuildDialogueUi(cardSprite);

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

        /// <summary>
        /// Generates the shared soft-ellipse shadow sprite: a radial-gradient
        /// Texture2D, dark purple and ~50% alpha fading to transparent at the
        /// edge. One asset is reused under every prop and the player.
        /// </summary>
        private static Sprite CreateShadowSprite(string name)
        {
            string path = GreyboxFolder + "/" + name + ".png";
            int size = ShadowTexturePixels;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            float radius = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f - radius) / radius;
                    float dy = (y + 0.5f - radius) / (radius * 0.5f);
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float falloff = Mathf.Clamp01(1f - distance);
                    Color c = ShadowColor;
                    c.a = ShadowColor.a * falloff * falloff;
                    pixels[y * size + x] = c;
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = ShadowPpu;
            importer.filterMode = FilterMode.Bilinear;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new System.InvalidOperationException("Failed to load shadow sprite: " + path);
            }
            return sprite;
        }

        /// <summary>
        /// Generates the interact prompt bubble: a soft-edged disc with a dark
        /// "!" glyph. Procedural like the shadow so no promoted art is needed.
        /// </summary>
        private static Sprite CreatePromptSprite(string name)
        {
            const int size = 64;
            const float bubbleRadius = 27f;
            const float bubbleCenterY = 32f;
            string path = GreyboxFolder + "/" + name + ".png";
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            Color bubbleColor = new Color(1f, 0.95f, 0.6f, 0.95f);
            Color glyphColor = new Color(0.2f, 0.12f, 0.25f, 1f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - size * 0.5f;
                    float dy = y + 0.5f - bubbleCenterY;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    if (distance > bubbleRadius)
                    {
                        continue;
                    }
                    float edge = Mathf.Clamp01(bubbleRadius - distance);
                    Color c = bubbleColor;
                    c.a *= edge;
                    // "!" glyph in texture space: bar above, dot below.
                    bool bar = Mathf.Abs(dx) < 3f && dy > 2f && dy < 16f;
                    bool dot = dx * dx + (dy + 10f) * (dy + 10f) < 16f;
                    pixels[y * size + x] = (bar || dot) ? glyphColor : c;
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = size;
            importer.filterMode = FilterMode.Bilinear;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new System.InvalidOperationException("Failed to load prompt sprite: " + path);
            }
            return sprite;
        }

        /// <summary>
        /// Places a flattened shadow sprite <see cref="ShadowOffset"/> down-left
        /// of <paramref name="basePosition"/>, scaled to the object's footprint.
        /// </summary>
        private static void AddShadow(Sprite shadowSprite, Vector2 basePosition,
            float footprintWidth, Transform parent = null,
            Vector2 extraLocalOffset = default)
        {
            GameObject shadow = new GameObject("Shadow");
            if (parent != null)
            {
                shadow.transform.SetParent(parent, false);
                shadow.transform.localPosition = ShadowOffset + extraLocalOffset;
                shadow.transform.localScale = new Vector3(footprintWidth, footprintWidth, 1f);
            }
            else
            {
                shadow.transform.position = basePosition + ShadowOffset;
                shadow.transform.localScale = new Vector3(footprintWidth, footprintWidth, 1f);
            }
            SpriteRenderer renderer = shadow.AddComponent<SpriteRenderer>();
            renderer.sprite = shadowSprite;
            renderer.sortingOrder = ShadowSortingOrder;
        }

        private static void CreateBuilding(string name, Sprite sprite,
            Vector2 position, Vector2 size, Sprite shadowSprite)
        {
            GameObject building = CreateScaledSprite(name, sprite, position, size, sortingOrder: 0);
            Vector2 spriteSize = sprite.bounds.size;
            BoxCollider2D collider = building.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(
                spriteSize.x * FootprintWidthFraction,
                spriteSize.y * FootprintHeightFraction);
            collider.offset = new Vector2(0f,
                -spriteSize.y * (0.5f - FootprintHeightFraction * 0.5f));
            AddShadow(shadowSprite,
                new Vector2(position.x, position.y - size.y * 0.5f), size.x * FootprintWidthFraction);
        }

        private static void CreateLantern(Sprite sprite, Vector2 position, Sprite shadowSprite)
        {
            const float lanternSize = 1f;
            CreateScaledSprite("Lantern", sprite, position, Vector2.one * lanternSize, sortingOrder: 1);
            AddShadow(shadowSprite,
                new Vector2(position.x, position.y - lanternSize * 0.5f), lanternSize * FootprintWidthFraction);
        }

        /// <summary>
        /// Rings the map with a dense treeline — oak/pine/round sprites cycled
        /// along all four edges just inside the bounds. Collision stays on four
        /// thin wall strips so tree spacing cannot open gaps.
        /// </summary>
        private static void BuildTreeline(Sprite oak, Sprite pine, Sprite round,
            Sprite shadowSprite)
        {
            Sprite[] cycle = { oak, pine, round };
            GameObject treelineRoot = new GameObject("Treeline");
            float halfWidth = MapBounds.width * 0.5f;
            float halfHeight = MapBounds.height * 0.5f;
            float insetY = halfHeight - TreelineInset;
            float insetX = halfWidth - TreelineInset;

            LayTreelineEdge(treelineRoot.transform, cycle, "TreelineNorth",
                new Vector2(-halfWidth, insetY), Vector2.right,
                MapBounds.width, shadowSprite);
            LayTreelineEdge(treelineRoot.transform, cycle, "TreelineSouth",
                new Vector2(-halfWidth, -insetY), Vector2.right,
                MapBounds.width, shadowSprite);
            LayTreelineEdge(treelineRoot.transform, cycle, "TreelineWest",
                new Vector2(-insetX, -halfHeight), Vector2.up,
                MapBounds.height, shadowSprite);
            LayTreelineEdge(treelineRoot.transform, cycle, "TreelineEast",
                new Vector2(insetX, -halfHeight), Vector2.up,
                MapBounds.height, shadowSprite);

            CreateWallCollider(treelineRoot.transform, "WallNorth",
                new Vector2(MapBounds.center.x, halfHeight),
                new Vector2(MapBounds.width + WallThickness, WallThickness));
            CreateWallCollider(treelineRoot.transform, "WallSouth",
                new Vector2(MapBounds.center.x, -halfHeight),
                new Vector2(MapBounds.width + WallThickness, WallThickness));
            CreateWallCollider(treelineRoot.transform, "WallWest",
                new Vector2(-halfWidth, MapBounds.center.y),
                new Vector2(WallThickness, MapBounds.height + WallThickness));
            CreateWallCollider(treelineRoot.transform, "WallEast",
                new Vector2(halfWidth, MapBounds.center.y),
                new Vector2(WallThickness, MapBounds.height + WallThickness));
        }

        /// <summary>
        /// Places trees end to end from <paramref name="start"/> along
        /// <paramref name="direction"/>, cycling oak/pine/round sprites.
        /// </summary>
        private static void LayTreelineEdge(Transform parent, Sprite[] cycle,
            string name, Vector2 start, Vector2 direction, float length,
            Sprite shadowSprite)
        {
            int count = Mathf.Max(1, Mathf.RoundToInt(length / TreelineSpacing));
            float step = length / count;
            for (int i = 0; i < count; i++)
            {
                Vector2 position = start + direction * (step * (i + 0.5f));
                Sprite sprite = cycle[i % cycle.Length];
                CreateScaledSprite(name + "_" + i, sprite, position,
                    Vector2.one * TreeHeight, sortingOrder: 0)
                    .transform.SetParent(parent, true);
                AddShadow(shadowSprite,
                    new Vector2(position.x, position.y - TreeHeight * 0.5f),
                    TreeHeight * 0.35f);
            }
        }

        private static void CreateWallCollider(Transform parent, string name,
            Vector2 center, Vector2 size)
        {
            GameObject strip = new GameObject(name);
            strip.transform.SetParent(parent);
            strip.transform.position = center;
            BoxCollider2D collider = strip.AddComponent<BoxCollider2D>();
            collider.size = size;
        }

        /// <summary>
        /// Scatters the hand-placed outskirts props from
        /// <see cref="ScatterPlacements"/>: trees/bushes/rocks get a base circle
        /// collider and a soft shadow; flower patches are flat pass-through
        /// decals (no collider, no shadow).
        /// </summary>
        private static void ScatterOutskirtsProps(Sprite shadowSprite,
            Sprite oak, Sprite pine, Sprite round, Sprite bush,
            Sprite berryBush, Sprite rock, Sprite flowers)
        {
            GameObject root = new GameObject("OutskirtsProps");
            for (int i = 0; i < ScatterPlacements.Length; i++)
            {
                OutskirtsProp kind = ScatterPlacements[i].Kind;
                Vector2 position = ScatterPlacements[i].Position;
                Sprite sprite = SpriteForProp(kind, oak, pine, round, bush,
                    berryBush, rock, flowers);
                float height = PropHeight(kind);
                bool flat = kind == OutskirtsProp.Flowers;

                GameObject prop = CreateScaledSprite(kind + "_" + i, sprite, position,
                    Vector2.one * height, sortingOrder: flat ? -8 : 0);
                prop.transform.SetParent(root.transform, true);
                if (flat)
                {
                    continue;
                }

                CircleCollider2D collider = prop.AddComponent<CircleCollider2D>();
                collider.radius = PropFootprintRadius / prop.transform.localScale.x;
                collider.offset = new Vector2(0f, -sprite.bounds.extents.y * 0.7f);
                AddShadow(shadowSprite,
                    new Vector2(position.x, position.y - height * 0.5f),
                    PropFootprintRadius * 2f);

                if (kind == OutskirtsProp.BerryBush)
                {
                    // All three bushes are quest fetch sources — uniform behavior
                    // is simpler than flagging one "real" bush (REEB-154).
                    Quests.QuestItemInteractable berryInteract =
                        prop.AddComponent<Quests.QuestItemInteractable>();
                    SerializedObject serializedBerry = new SerializedObject(berryInteract);
                    serializedBerry.FindProperty("promptVerb").stringValue = "Pick berries";
                    serializedBerry.FindProperty("targetId").stringValue = QuestFetchTargetId;
                    serializedBerry.FindProperty("interactRadius").floatValue = 1.6f;
                    serializedBerry.FindProperty("promptHeight").floatValue = 0.8f;
                    serializedBerry.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        private static Sprite SpriteForProp(OutskirtsProp kind,
            Sprite oak, Sprite pine, Sprite round, Sprite bush,
            Sprite berryBush, Sprite rock, Sprite flowers)
        {
            switch (kind)
            {
                case OutskirtsProp.Oak: return oak;
                case OutskirtsProp.Pine: return pine;
                case OutskirtsProp.RoundTree: return round;
                case OutskirtsProp.Bush: return bush;
                case OutskirtsProp.BerryBush: return berryBush;
                case OutskirtsProp.Rock: return rock;
                default: return flowers;
            }
        }

        private static float PropHeight(OutskirtsProp kind)
        {
            switch (kind)
            {
                case OutskirtsProp.Oak:
                case OutskirtsProp.Pine:
                case OutskirtsProp.RoundTree:
                    return TreeHeight;
                case OutskirtsProp.Rock:
                    return RockHeight;
                case OutskirtsProp.Flowers:
                    return FlowerPatchSize;
                default:
                    return BushHeight;
            }
        }

        /// <summary>
        /// Generates the rounded-corner panel sprite for the dialogue card,
        /// baked white (tinted via Image.color) with a 24px border so the Image
        /// can 9-slice to any card size without distorting the corners.
        /// </summary>
        private static Sprite CreateCardSprite(string name)
        {
            string path = GreyboxFolder + "/" + name + ".png";
            int size = CardSpritePixels;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            float half = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Signed distance to a rounded rectangle centered on the texel.
                    float qx = Mathf.Abs(x + 0.5f - half) - (half - CardCornerRadius);
                    float qy = Mathf.Abs(y + 0.5f - half) - (half - CardCornerRadius);
                    float distance = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude
                        + Mathf.Min(Mathf.Max(qx, qy), 0f) - CardCornerRadius;
                    float alpha = Mathf.Clamp01(-distance);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = CardSpritePixels;
            importer.spriteBorder = new Vector4(
                CardSpriteBorder, CardSpriteBorder, CardSpriteBorder, CardSpriteBorder);
            importer.filterMode = FilterMode.Bilinear;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new System.InvalidOperationException("Failed to load card sprite: " + path);
            }
            return sprite;
        }

        /// <summary>
        /// Builds the dialogue overlay canvas: an inactive bottom-center card
        /// (name header + body line + continue hint) plus the
        /// <see cref="UI.DialogueController"/> that drives it.
        /// </summary>
        private static void BuildDialogueUi(Sprite cardSprite)
        {
            GameObject canvasObject = new GameObject("DialogueUI");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = DialogueCanvasSortingOrder;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasObject.AddComponent<GraphicRaycaster>();

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject card = new GameObject("DialogueCard");
            RectTransform cardRect = card.AddComponent<RectTransform>();
            cardRect.SetParent(canvasObject.transform, false);
            cardRect.anchorMin = new Vector2(0.5f, 0f);
            cardRect.anchorMax = new Vector2(0.5f, 0f);
            cardRect.pivot = new Vector2(0.5f, 0f);
            cardRect.anchoredPosition = new Vector2(0f, CardBottomMargin);
            cardRect.sizeDelta = CardSize;
            Image panel = card.AddComponent<Image>();
            panel.sprite = cardSprite;
            panel.type = Image.Type.Sliced;
            panel.color = CardColor;
            panel.raycastTarget = false;

            Text nameText = CreateCardText(cardRect, "NameText", font, 30, CardNameColor,
                TextAnchor.MiddleLeft, FontStyle.Bold,
                new Rect(CardPadding, CardSize.y - 64f, CardSize.x - CardPadding * 2f, 40f));
            Text bodyText = CreateCardText(cardRect, "BodyText", font, 26, CardBodyColor,
                TextAnchor.UpperLeft, FontStyle.Normal,
                new Rect(CardPadding, CardPadding + 24f, CardSize.x - CardPadding * 2f, CardSize.y - 100f));
            CreateCardText(cardRect, "ContinueHint", font, 20, CardHintColor,
                TextAnchor.MiddleRight, FontStyle.Italic,
                new Rect(CardSize.x - CardPadding - 140f, 8f, 140f, 28f))
                .text = "...";

            card.SetActive(false);

            UI.DialogueCard cardView = canvasObject.AddComponent<UI.DialogueCard>();
            SerializedObject serializedCard = new SerializedObject(cardView);
            serializedCard.FindProperty("cardRoot").objectReferenceValue = card;
            serializedCard.FindProperty("nameText").objectReferenceValue = nameText;
            serializedCard.FindProperty("bodyText").objectReferenceValue = bodyText;
            serializedCard.ApplyModifiedPropertiesWithoutUndo();

            UI.DialogueController controller =
                canvasObject.AddComponent<UI.DialogueController>();
            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("card").objectReferenceValue = cardView;
            serializedController.FindProperty("inputActions").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            serializedController.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Creates a uGUI Text inside the dialogue card. <paramref name="rect"/>
        /// is a pixel rectangle in card space measured from the card's
        /// bottom-left corner.
        /// </summary>
        private static Text CreateCardText(RectTransform parent, string name,
            Font font, int fontSize, Color color, TextAnchor alignment,
            FontStyle style, Rect rect)
        {
            GameObject textObject = new GameObject(name);
            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.SetParent(parent, false);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.zero;
            textRect.pivot = Vector2.zero;
            textRect.anchoredPosition = rect.position;
            textRect.sizeDelta = rect.size;
            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = style;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        /// <summary>
        /// Builds the persistent HUD canvas (REEB-154): objective text pinned
        /// top-left, heart counter top-right, and a CanvasGroup-wrapped toast
        /// bottom-center above where the dialogue card sits. The Hud component
        /// registers with HudViewLocator so QuestService can push to it.
        /// </summary>
        private static void BuildHud()
        {
            GameObject canvasObject = new GameObject("Hud");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = HudCanvasSortingOrder;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            Text objective = CreateHudText(canvasObject.transform, "ObjectiveText",
                font, 26, HudTextColor, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(HudMargin, -HudMargin), new Vector2(700f, 80f));

            Text hearts = CreateHudText(canvasObject.transform, "HeartsText",
                font, 30, HudHeartColor, TextAnchor.UpperRight,
                new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-HudMargin, -HudMargin), new Vector2(200f, 44f));
            hearts.text = "♥ 0";

            GameObject toastObject = new GameObject("Toast");
            RectTransform toastRect = toastObject.AddComponent<RectTransform>();
            toastRect.SetParent(canvasObject.transform, false);
            toastRect.anchorMin = new Vector2(0.5f, 0f);
            toastRect.anchorMax = new Vector2(0.5f, 0f);
            toastRect.pivot = new Vector2(0.5f, 0f);
            toastRect.anchoredPosition = new Vector2(0f, HudToastBottom);
            toastRect.sizeDelta = new Vector2(800f, 44f);
            CanvasGroup toastGroup = toastObject.AddComponent<CanvasGroup>();
            toastGroup.alpha = 0f;
            toastGroup.interactable = false;
            toastGroup.blocksRaycasts = false;
            Text toast = toastObject.AddComponent<Text>();
            toast.font = font;
            toast.fontSize = 26;
            toast.color = HudTextColor;
            toast.alignment = TextAnchor.MiddleCenter;
            toast.horizontalOverflow = HorizontalWrapMode.Wrap;
            toast.verticalOverflow = VerticalWrapMode.Overflow;
            toast.raycastTarget = false;

            UI.Hud hud = canvasObject.AddComponent<UI.Hud>();
            SerializedObject serializedHud = new SerializedObject(hud);
            serializedHud.FindProperty("objectiveText").objectReferenceValue = objective;
            serializedHud.FindProperty("heartsText").objectReferenceValue = hearts;
            serializedHud.FindProperty("toastText").objectReferenceValue = toast;
            serializedHud.FindProperty("toastGroup").objectReferenceValue = toastGroup;
            serializedHud.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Creates a uGUI Text anchored to a screen corner/edge.
        /// <paramref name="anchor"/> is shared by anchorMin/anchorMax/pivot so
        /// <paramref name="anchoredPosition"/> offsets inward from it.
        /// </summary>
        private static Text CreateHudText(Transform parent, string name,
            Font font, int fontSize, Color color, TextAnchor alignment,
            Vector2 anchor, Vector2 pivot, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject textObject = new GameObject(name);
            RectTransform rect = textObject.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
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

        private static GameObject BuildPlayerPrefab(Sprite sprite,
            Sprite front, Sprite back, Sprite left, Sprite right,
            Sprite shadowSprite, Sprite promptSprite)
        {
            GameObject player = new GameObject("Player");
            player.transform.localScale = Vector3.one * 0.8f;
            SpriteRenderer renderer = player.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 1;
            player.AddComponent<Rigidbody2D>();
            player.AddComponent<CircleCollider2D>();
            Player.PlayerMovement movement = player.AddComponent<Player.PlayerMovement>();
            Player.PlayerFacing facing = player.AddComponent<Player.PlayerFacing>();
            Interaction.Interactor interactor =
                player.AddComponent<Interaction.Interactor>();
            player.AddComponent<Interaction.CarrySlot>();

            // Single shared prompt bubble: a world-space child the Interactor
            // repositions over the current target. ~0.4 world units after
            // cancelling the player's 0.8 scale.
            GameObject promptObject = new GameObject("InteractPrompt");
            promptObject.transform.SetParent(player.transform, false);
            promptObject.transform.localScale = Vector3.one * 0.5f;
            SpriteRenderer promptRenderer = promptObject.AddComponent<SpriteRenderer>();
            promptRenderer.sprite = promptSprite;
            promptRenderer.sortingOrder = 5;
            promptObject.SetActive(false);

            // Shadow is a child so it follows the player; local scale cancels
            // the player's 0.8 transform so the ellipse stays ~1 unit wide.
            AddShadow(shadowSprite, Vector2.zero, 1f / player.transform.localScale.x,
                player.transform);

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

            SerializedObject serializedFacing = new SerializedObject(facing);
            serializedFacing.FindProperty("inputActions").objectReferenceValue = inputActions;
            SetSpriteProperty(serializedFacing, "frontSprite", front);
            SetSpriteProperty(serializedFacing, "backSprite", back);
            SetSpriteProperty(serializedFacing, "leftSprite", left);
            SetSpriteProperty(serializedFacing, "rightSprite", right);
            serializedFacing.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject serializedInteractor = new SerializedObject(interactor);
            serializedInteractor.FindProperty("inputActions").objectReferenceValue = inputActions;
            serializedInteractor.FindProperty("prompt").objectReferenceValue =
                promptObject.transform;
            serializedInteractor.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(player, PlayerPrefabPath);
            Object.DestroyImmediate(player);
            return prefab;
        }

        /// <summary>
        /// Generates Npc.prefab: the villager sprite scaled to
        /// <see cref="NpcHeight"/>, a solid circle footprint so she blocks
        /// movement, an <see cref="UI.NpcComponent"/> holding Marla's lines, an
        /// <see cref="UI.NpcInteractable"/> that opens the dialogue card, and a
        /// shadow child at her feet.
        /// </summary>
        private static GameObject BuildNpcPrefab(Sprite sprite, Sprite shadowSprite)
        {
            GameObject npc = new GameObject("Npc");
            SpriteRenderer renderer = npc.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 1;
            Vector2 spriteSize = sprite.bounds.size;
            float scale = NpcHeight / spriteSize.y;
            npc.transform.localScale = Vector3.one * scale;

            // Collider lives in local units, so world size is radius * scale.
            // Centered on the feet — the lower edge of the sprite rect.
            CircleCollider2D collider = npc.AddComponent<CircleCollider2D>();
            collider.radius = NpcFootprintRadius / scale;
            collider.offset = new Vector2(0f,
                -spriteSize.y * 0.5f + collider.radius * 0.5f);

            UI.NpcComponent component = npc.AddComponent<UI.NpcComponent>();
            SerializedObject serializedNpc = new SerializedObject(component);
            serializedNpc.FindProperty("displayName").stringValue = "Marla the Baker";
            SerializedProperty lines = serializedNpc.FindProperty("lines");
            lines.arraySize = 3;
            lines.GetArrayElementAtIndex(0).stringValue =
                "Fresh bread this morning — the rye's still warm, if you've a mind.";
            lines.GetArrayElementAtIndex(1).stringValue =
                "That oven keeps the whole street cozy through the cold nights.";
            lines.GetArrayElementAtIndex(2).stringValue =
                "Mind the flour on your sleeves, dear. It gets everywhere.";
            serializedNpc.ApplyModifiedPropertiesWithoutUndo();

            // QuestGiverInteractable subclasses NpcInteractable: it serves
            // quest offer/active/complete lines by phase and falls back to the
            // NpcComponent flavor lines once the quest is done (REEB-154).
            Quests.QuestGiverInteractable interactable =
                npc.AddComponent<Quests.QuestGiverInteractable>();
            SerializedObject serializedInteract = new SerializedObject(interactable);
            serializedInteract.FindProperty("promptVerb").stringValue = "Talk";
            serializedInteract.FindProperty("interactRadius").floatValue = NpcInteractRadius;
            serializedInteract.FindProperty("promptHeight").floatValue = NpcHeight + 0.2f;
            serializedInteract.FindProperty("npc").objectReferenceValue = component;
            serializedInteract.FindProperty("npcId").stringValue = QuestGiverNpcId;
            serializedInteract.FindProperty("questId").stringValue = QuestId;
            serializedInteract.ApplyModifiedPropertiesWithoutUndo();

            // Shadow scale cancels the NPC's transform so the ellipse lands at
            // NpcShadowWidth world units under her feet.
            AddShadow(shadowSprite, Vector2.zero, NpcShadowWidth / scale,
                npc.transform, new Vector2(0f, -spriteSize.y * 0.5f));

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(npc, NpcPrefabPath);
            Object.DestroyImmediate(npc);
            return prefab;
        }

        private static void SetSpriteProperty(SerializedObject serialized,
            string property, Sprite sprite)
        {
            if (sprite != null)
            {
                serialized.FindProperty(property).objectReferenceValue = sprite;
            }
        }
    }
}
