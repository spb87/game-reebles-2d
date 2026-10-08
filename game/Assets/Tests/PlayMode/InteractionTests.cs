using System.Collections;
using System.Reflection;
using NUnit.Framework;
using Reebles2D.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace Reebles2D.Tests.PlayMode
{
    /// <summary>
    /// PlayMode tests for <see cref="Interactor"/>/<see cref="Interactable"/>:
    /// nearest-in-range detection, prompt visibility, and the Interact action
    /// invoking the current target.
    /// </summary>
    public class InteractionTests : InputTestFixture
    {
        private Keyboard keyboard;
        private GameObject player;
        private GameObject interactableObject;

        private class FlagInteractable : Interactable
        {
            public bool Fired;

            public override void Interact()
            {
                base.Interact();
                Fired = true;
            }
        }

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
            if (interactableObject != null)
            {
                Object.Destroy(interactableObject);
            }
            base.TearDown();
        }

        private static InputActionAsset CreateActions()
        {
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = new InputActionMap("Player");
            map.AddAction("Interact", InputActionType.Button, "<Keyboard>/e");
            asset.AddActionMap(map);
            return asset;
        }

        private Interactor SpawnPlayer(Vector2 position)
        {
            var go = new GameObject("Player");
            go.SetActive(false);
            var interactor = go.AddComponent<Interactor>();
            typeof(Interactor)
                .GetField("inputActions", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(interactor, CreateActions());
            var prompt = new GameObject("Prompt");
            prompt.SetActive(false);
            typeof(Interactor)
                .GetField("prompt", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(interactor, prompt.transform);
            go.transform.position = position;
            go.SetActive(true);
            player = go;
            return interactor;
        }

        private FlagInteractable SpawnInteractable(Vector2 position)
        {
            var go = new GameObject("Target");
            go.transform.position = position;
            var collider = go.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.4f;
            var target = go.AddComponent<FlagInteractable>();
            interactableObject = go;
            Physics2D.SyncTransforms();
            return target;
        }

        private GameObject PromptObject()
        {
            return ((Transform)typeof(Interactor)
                .GetField("prompt", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(player.GetComponent<Interactor>())).gameObject;
        }

        [UnityTest]
        public IEnumerator InRange_SetsCurrentTargetAndShowsPrompt()
        {
            var interactor = SpawnPlayer(Vector2.zero);
            SpawnInteractable(new Vector2(1f, 0f));

            yield return null;

            Assert.That(interactor.CurrentTarget, Is.Not.Null);
            Assert.That(PromptObject().activeSelf, Is.True);
        }

        [UnityTest]
        public IEnumerator OutOfRange_ClearsCurrentTargetAndHidesPrompt()
        {
            var interactor = SpawnPlayer(Vector2.zero);
            SpawnInteractable(new Vector2(1f, 0f));
            yield return null;
            Assert.That(interactor.CurrentTarget, Is.Not.Null);

            // Move beyond the default 1.5 interactRadius but stay under the
            // scan radius — no false positives from out-of-range targets.
            interactableObject.transform.position = new Vector3(2.5f, 0f, 0f);
            Physics2D.SyncTransforms();
            yield return null;

            Assert.That(interactor.CurrentTarget, Is.Null);
            Assert.That(PromptObject().activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator InteractAction_FiresCurrentTarget()
        {
            SpawnPlayer(Vector2.zero);
            var target = SpawnInteractable(new Vector2(1f, 0f));
            yield return null;

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());

            Assert.That(target.Fired, Is.True);
            Assert.That(target.InteractCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator InteractAction_NoTarget_DoesNotFire()
        {
            SpawnPlayer(Vector2.zero);
            var target = SpawnInteractable(new Vector2(2.5f, 0f));
            yield return null;

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());

            Assert.That(target.Fired, Is.False);
        }
    }
}
