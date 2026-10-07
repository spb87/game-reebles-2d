using System.Collections;
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
    /// PlayMode tests for <see cref="PlayerMovement"/>: keyboard-driven movement
    /// and wall collision via Rigidbody2D.Slide (kinematic collide-and-slide).
    /// </summary>
    public class PlayerMovementTests : InputTestFixture
    {
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

        private PlayerMovement SpawnPlayer(Vector2 position)
        {
            var go = new GameObject("Player");
            go.SetActive(false);
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            go.AddComponent<CircleCollider2D>().radius = 0.25f;
            var movement = go.AddComponent<PlayerMovement>();
            typeof(PlayerMovement)
                .GetField("inputActions", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(movement, CreateActions());
            go.transform.position = position;
            go.SetActive(true);
            player = go;
            return movement;
        }

        private static void SpawnWall(Vector2 center, Vector2 size)
        {
            var wall = new GameObject("Wall");
            wall.transform.position = center;
            var col = wall.AddComponent<BoxCollider2D>();
            col.size = size;
            Physics2D.SyncTransforms();
        }

        [UnityTest]
        public IEnumerator MoveRight_ChangesPositionPlusX()
        {
            var movement = SpawnPlayer(Vector2.zero);
            float startX = player.transform.position.x;

            HoldKeys(Key.D);
            for (int i = 0; i < 30; i++)
            {
                yield return new WaitForFixedUpdate();
            }
            ReleaseAllKeys();

            Assert.That(player.transform.position.x, Is.GreaterThan(startX + 0.5f));
            Assert.That(movement.CurrentSpeed, Is.GreaterThan(0f));
        }

        [UnityTest]
        public IEnumerator MoveRightIntoWall_StopsBeforeWallFace()
        {
            // Wall face at x = 1.0 (center 1.5, size 1); player radius 0.25.
            SpawnWall(new Vector2(1.5f, 0f), new Vector2(1f, 5f));
            SpawnPlayer(Vector2.zero);

            HoldKeys(Key.D);
            for (int i = 0; i < 60; i++)
            {
                yield return new WaitForFixedUpdate();
            }
            ReleaseAllKeys();

            float x = player.transform.position.x;
            Assert.That(x, Is.GreaterThan(0.1f), "player never moved toward the wall");
            Assert.That(x, Is.LessThan(0.8f), "player passed through the wall");
        }

        [UnityTest]
        public IEnumerator RunHeld_SetsIsRunningAndBoostsSpeed()
        {
            var movement = SpawnPlayer(Vector2.zero);

            HoldKeys(Key.D, Key.LeftShift);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.That(movement.IsRunning, Is.True);
            Assert.That(movement.CurrentSpeed, Is.GreaterThan(3f));

            ReleaseAllKeys();
        }
    }
}
