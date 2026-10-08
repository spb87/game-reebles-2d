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
    /// PlayMode tests for <see cref="PlayerFacing"/>: the SpriteRenderer swaps
    /// to the correct directional sprite for the dominant input axis and keeps
    /// the last facing while idle.
    /// </summary>
    public class PlayerFacingTests : InputTestFixture
    {
        private Keyboard keyboard;
        private GameObject player;
        private Sprite front;
        private Sprite back;
        private Sprite left;
        private Sprite right;

        public override void Setup()
        {
            base.Setup();
            keyboard = InputSystem.AddDevice<Keyboard>();
            front = MakeSprite();
            back = MakeSprite();
            left = MakeSprite();
            right = MakeSprite();
        }

        public override void TearDown()
        {
            if (player != null)
            {
                Object.Destroy(player);
            }
            Object.Destroy(front);
            Object.Destroy(back);
            Object.Destroy(left);
            Object.Destroy(right);
            base.TearDown();
        }

        private static Sprite MakeSprite()
        {
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
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
            asset.AddActionMap(map);
            return asset;
        }

        private PlayerFacing SpawnPlayer()
        {
            var go = new GameObject("Player");
            go.SetActive(false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = front;
            var facing = go.AddComponent<PlayerFacing>();
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            typeof(PlayerFacing).GetField("inputActions", flags)
                .SetValue(facing, CreateActions());
            typeof(PlayerFacing).GetField("frontSprite", flags).SetValue(facing, front);
            typeof(PlayerFacing).GetField("backSprite", flags).SetValue(facing, back);
            typeof(PlayerFacing).GetField("leftSprite", flags).SetValue(facing, left);
            typeof(PlayerFacing).GetField("rightSprite", flags).SetValue(facing, right);
            go.SetActive(true);
            player = go;
            return facing;
        }

        private void HoldKeys(params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        }

        [UnityTest]
        public IEnumerator UpInput_SwitchesToBackSprite()
        {
            var facing = SpawnPlayer();
            HoldKeys(Key.W);
            yield return null;

            Assert.That(facing.Facing, Is.EqualTo(Vector2.up));
            Assert.That(player.GetComponent<SpriteRenderer>().sprite, Is.SameAs(back));
        }

        [UnityTest]
        public IEnumerator LeftThenRelease_KeepsLastFacing()
        {
            var facing = SpawnPlayer();
            HoldKeys(Key.A);
            yield return null;
            Assert.That(player.GetComponent<SpriteRenderer>().sprite, Is.SameAs(left));

            HoldKeys();
            yield return null;

            Assert.That(facing.Facing, Is.EqualTo(Vector2.left));
            Assert.That(player.GetComponent<SpriteRenderer>().sprite, Is.SameAs(left));
        }
    }
}
