// Greybox scene generator: builds the Village scene, Player prefab, and
// placeholder sprites entirely in code. Run via:
//   Unity.exe -batchmode -nographics -projectPath game \
//     -executeMethod Reebles2D.Editor.VillageSceneBuilder.Build -quit

using System.IO;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reebles2D.Editor
{
    /// <summary>
    /// Single entry point that regenerates the greybox village: sprites under
    /// Assets/Art/Greybox, Assets/Prefabs/Player.prefab, and Assets/Scenes/Village.unity.
    /// Idempotent — re-running overwrites all outputs cleanly.
    /// </summary>
    public static class VillageSceneBuilder
    {
        private const string GreyboxFolder = "Assets/Art/Greybox";
        private const string PrefabFolder = "Assets/Prefabs";
        private const string SceneFolder = "Assets/Scenes";
        private const string ScenePath = SceneFolder + "/Village.unity";
        private const string PlayerPrefabPath = PrefabFolder + "/Player.prefab";
        private const string InputActionsPath = "Assets/Input/ReeblesInput.inputactions";

        private const int SpritePixels = 16;

        private static readonly Rect MapBounds = new Rect(-15f, -10f, 30f, 20f);
        private const float FenceThickness = 0.3f;

        private static readonly Color GroundColor = new Color(0.42f, 0.55f, 0.33f);
        private static readonly Color PlayerColor = new Color(0.95f, 0.55f, 0.20f);
        private static readonly Color FountainColor = new Color(0.30f, 0.55f, 0.80f);
        private static readonly Color FenceColor = new Color(0.30f, 0.20f, 0.12f);
        private static readonly Color[] BuildingColors =
        {
            new Color(0.72f, 0.42f, 0.30f),
            new Color(0.65f, 0.38f, 0.28f),
            new Color(0.70f, 0.45f, 0.34f),
            new Color(0.62f, 0.36f, 0.30f),
            new Color(0.75f, 0.48f, 0.32f),
        };

        /// <summary>Regenerates all greybox assets and the Village scene.</summary>
        public static void Build()
        {
            EnsureFolder("Assets", "Art");
            EnsureFolder("Assets/Art", "Greybox");
            EnsureFolder("Assets", "Prefabs");
            EnsureFolder("Assets", "Scenes");

            Sprite groundSprite = CreateRectSprite("Ground", GroundColor);
            Sprite rectSprite = CreateRectSprite("Rect", PlayerColor);
            Sprite terracottaSprite = CreateRectSprite("Building", BuildingColors[0]);
            Sprite fountainSprite = CreateCircleSprite("Fountain", FountainColor);
            Sprite fenceSprite = CreateRectSprite("Fence", FenceColor);
            Sprite[] buildingSprites = new Sprite[BuildingColors.Length];
            for (int i = 0; i < BuildingColors.Length; i++)
            {
                buildingSprites[i] = CreateRectSprite("Building" + i, BuildingColors[i]);
            }

            GameObject playerPrefab = BuildPlayerPrefab(rectSprite);

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Village";

            CreateScaledSprite("Ground", groundSprite, Vector3.zero,
                new Vector2(MapBounds.width, MapBounds.height), sortingOrder: -10);

            CreateBuilding("Bakery", buildingSprites[0], new Vector2(-8f, 5f), new Vector2(4f, 3f));
            CreateBuilding("Smithy", buildingSprites[1], new Vector2(-8f, -5f), new Vector2(4f, 3f));
            CreateBuilding("Herbalist", buildingSprites[2], new Vector2(8f, 5f), new Vector2(4f, 3f));
            CreateBuilding("Store", buildingSprites[3], new Vector2(8f, -5f), new Vector2(4f, 3f));
            CreateBuilding("Inn", buildingSprites[4], new Vector2(0f, 7f), new Vector2(6f, 3f));

            GameObject fountain = CreateScaledSprite("Fountain", fountainSprite,
                Vector3.zero, Vector2.one * 2f, sortingOrder: 0);
            CircleCollider2D fountainCollider = fountain.AddComponent<CircleCollider2D>();
            fountainCollider.radius = 0.5f;

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

        private static Sprite CreateRectSprite(string name, Color color)
        {
            return CreateSprite(name, color, circle: false);
        }

        private static Sprite CreateCircleSprite(string name, Color color)
        {
            return CreateSprite(name, color, circle: true);
        }

        private static Sprite CreateSprite(string name, Color color, bool circle)
        {
            string path = GreyboxFolder + "/" + name + ".png";
            Texture2D texture = new Texture2D(SpritePixels, SpritePixels, TextureFormat.RGBA32, false);
            float radius = (SpritePixels - 1) * 0.5f;
            for (int y = 0; y < SpritePixels; y++)
            {
                for (int x = 0; x < SpritePixels; x++)
                {
                    bool inside = !circle ||
                        Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius)) <= radius;
                    texture.SetPixel(x, y, inside ? color : Color.clear);
                }
            }
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = SpritePixels;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();

            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Sprite sprite)
                {
                    return sprite;
                }
            }
            throw new System.InvalidOperationException("Failed to load generated sprite: " + path);
        }

        private static GameObject CreateScaledSprite(string name, Sprite sprite,
            Vector3 position, Vector2 size, int sortingOrder)
        {
            GameObject obj = new GameObject(name);
            obj.transform.position = position;
            obj.transform.localScale = new Vector3(size.x, size.y, 1f);
            SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            return obj;
        }

        private static void CreateBuilding(string name, Sprite sprite, Vector2 position, Vector2 size)
        {
            GameObject building = CreateScaledSprite(name, sprite, position, size, sortingOrder: 0);
            BoxCollider2D collider = building.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
        }

        private static void BuildFence(Sprite fenceSprite)
        {
            GameObject fenceRoot = new GameObject("Fence");
            float halfWidth = MapBounds.width * 0.5f;
            float halfHeight = MapBounds.height * 0.5f;
            Vector2[] centers =
            {
                new Vector2(MapBounds.center.x, halfHeight),
                new Vector2(MapBounds.center.x, -halfHeight),
                new Vector2(-halfWidth, MapBounds.center.y),
                new Vector2(halfWidth, MapBounds.center.y),
            };
            Vector2[] sizes =
            {
                new Vector2(MapBounds.width + FenceThickness, FenceThickness),
                new Vector2(MapBounds.width + FenceThickness, FenceThickness),
                new Vector2(FenceThickness, MapBounds.height + FenceThickness),
                new Vector2(FenceThickness, MapBounds.height + FenceThickness),
            };
            string[] names = { "FenceNorth", "FenceSouth", "FenceWest", "FenceEast" };

            for (int i = 0; i < centers.Length; i++)
            {
                GameObject segment = CreateScaledSprite(names[i], fenceSprite,
                    centers[i], sizes[i], sortingOrder: 0);
                segment.transform.SetParent(fenceRoot.transform);
                BoxCollider2D collider = segment.AddComponent<BoxCollider2D>();
                collider.size = Vector2.one;
            }
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
