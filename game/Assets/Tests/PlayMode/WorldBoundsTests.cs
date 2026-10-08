using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Reebles2D.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace Reebles2D.Tests.PlayMode
{
    /// <summary>
    /// PlayMode tests for the expanded world bounds: the four thin wall strips
    /// the VillageSceneBuilder places along the treeline perimeter (map
    /// 60x40 centered on origin, walls at x=±30, y=±20) must keep the player
    /// inside the map. Mirrors the wall geometry from BuildTreeline.
    /// </summary>
    public class WorldBoundsTests : InputTestFixture
    {
        private const float HalfMapWidth = 30f;
        private const float HalfMapHeight = 20f;
        private const float WallThickness = 0.3f;
        private const float PlayerRadius = 0.25f;

        private readonly List<GameObject> walls = new List<GameObject>();
        private Keyboard keyboard;
        private GameObject player;

        public override void Setup()
        {
            base.Setup();
            keyboard = InputSystem.AddDevice<Keyboard>();
        }

        public override void TearDown()
        {
            if (player != null)
            {
                Object.Destroy(player);
            }
            foreach (GameObject wall in walls)
            {
                Object.Destroy(wall);
            }
            walls.Clear();
            base.TearDown();
        }

        private void HoldKeys(params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        }

        private void ReleaseAllKeys()
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        }

        private static InputActionAsset CreateActions()
        {
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = new InputActionMap("Player");
            var move = map.AddAction("Move", InputActionType.Value);
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            map.AddAction("Run", InputActionType.Button, "<Keyboard>/leftShift");
            asset.AddActionMap(map);
            return asset;
        }

        private void SpawnPlayer(Vector2 position)
        {
            var go = new GameObject("Player");
            go.SetActive(false);
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            go.AddComponent<CircleCollider2D>().radius = PlayerRadius;
            var movement = go.AddComponent<PlayerMovement>();
            typeof(PlayerMovement)
                .GetField("inputActions", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(movement, CreateActions());
            go.transform.position = position;
            go.SetActive(true);
            player = go;
        }

        private void SpawnWall(string name, Vector2 center, Vector2 size)
        {
            var wall = new GameObject(name);
            walls.Add(wall);
            wall.transform.position = center;
            var col = wall.AddComponent<BoxCollider2D>();
            col.size = size;
            Physics2D.SyncTransforms();
        }

        /// <summary>
        /// Rebuilds the same four wall strips VillageSceneBuilder.BuildTreeline
        /// creates around the 60x40 map.
        /// </summary>
        private void SpawnPerimeterWalls()
        {
            SpawnWall("WallNorth",
                new Vector2(0f, HalfMapHeight),
                new Vector2(HalfMapWidth * 2f + WallThickness, WallThickness));
            SpawnWall("WallSouth",
                new Vector2(0f, -HalfMapHeight),
                new Vector2(HalfMapWidth * 2f + WallThickness, WallThickness));
            SpawnWall("WallWest",
                new Vector2(-HalfMapWidth, 0f),
                new Vector2(WallThickness, HalfMapHeight * 2f + WallThickness));
            SpawnWall("WallEast",
                new Vector2(HalfMapWidth, 0f),
                new Vector2(WallThickness, HalfMapHeight * 2f + WallThickness));
        }

        [UnityTest]
        public IEnumerator EastEdge_PlayerStaysInsideBounds()
        {
            SpawnPerimeterWalls();
            SpawnPlayer(new Vector2(HalfMapWidth - 2f, 0f));

            HoldKeys(Key.D);
            for (int i = 0; i < 120; i++)
            {
                yield return new WaitForFixedUpdate();
            }
            ReleaseAllKeys();

            float x = player.transform.position.x;
            Assert.That(x, Is.GreaterThan(HalfMapWidth - 2f),
                "player never moved toward the east edge");
            Assert.That(x, Is.LessThan(HalfMapWidth - PlayerRadius),
                "player escaped the east bounds wall");
        }

        [UnityTest]
        public IEnumerator NorthEdge_PlayerStaysInsideBounds()
        {
            SpawnPerimeterWalls();
            SpawnPlayer(new Vector2(0f, HalfMapHeight - 2f));

            HoldKeys(Key.W);
            for (int i = 0; i < 120; i++)
            {
                yield return new WaitForFixedUpdate();
            }
            ReleaseAllKeys();

            float y = player.transform.position.y;
            Assert.That(y, Is.GreaterThan(HalfMapHeight - 2f),
                "player never moved toward the north edge");
            Assert.That(y, Is.LessThan(HalfMapHeight - PlayerRadius),
                "player escaped the north bounds wall");
        }

        [UnityTest]
        public IEnumerator SouthWestCorner_PlayerStaysInsideBounds()
        {
            SpawnPerimeterWalls();
            SpawnPlayer(new Vector2(-HalfMapWidth + 2f, -HalfMapHeight + 2f));

            HoldKeys(Key.A, Key.S);
            for (int i = 0; i < 120; i++)
            {
                yield return new WaitForFixedUpdate();
            }
            ReleaseAllKeys();

            Vector2 pos = player.transform.position;
            Assert.That(pos.x, Is.LessThan(-HalfMapWidth + 2f),
                "player never moved toward the west edge");
            Assert.That(pos.y, Is.LessThan(-HalfMapHeight + 2f),
                "player never moved toward the south edge");
            Assert.That(pos.x, Is.GreaterThan(-HalfMapWidth + PlayerRadius),
                "player escaped the west bounds wall");
            Assert.That(pos.y, Is.GreaterThan(-HalfMapHeight + PlayerRadius),
                "player escaped the south bounds wall");
        }
    }
}
