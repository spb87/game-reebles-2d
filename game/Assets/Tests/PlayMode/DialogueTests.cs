using System.Collections;
using System.Reflection;
using NUnit.Framework;
using Reebles2D.Interaction;
using Reebles2D.Player;
using Reebles2D.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Reebles2D.Tests.PlayMode
{
    /// <summary>
    /// PlayMode tests for <see cref="DialogueController"/>,
    /// <see cref="DialogueCard"/> and <see cref="NpcInteractable"/>: interacting
    /// opens the card with the speaker name and first line, further Interact
    /// presses cycle lines, the last line closes the card, and the Move action
    /// is suppressed while the card is open. Objects are spawned in code —
    /// no scene load, so nothing to restore.
    /// </summary>
    public class DialogueTests : InputTestFixture
    {
        private Keyboard keyboard;
        private InputActionAsset actions;
        private GameObject dialogueUiObject;
        private GameObject playerObject;
        private GameObject npcObject;
        private DialogueCard card;
        private DialogueController controller;

        private static readonly string[] FountainLines =
        {
            "The fountain burbles quietly in the square.",
        };

        public override void Setup()
        {
            base.Setup();
            keyboard = InputSystem.AddDevice<Keyboard>();
            actions = CreateActions();
            SpawnDialogueUi();
        }

        public override void TearDown()
        {
            if (playerObject != null)
            {
                Object.Destroy(playerObject);
            }
            if (npcObject != null)
            {
                Object.Destroy(npcObject);
            }
            if (dialogueUiObject != null)
            {
                Object.Destroy(dialogueUiObject);
            }
            if (actions != null)
            {
                Object.Destroy(actions);
            }
            base.TearDown();
        }

        private static InputActionAsset CreateActions()
        {
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = new InputActionMap("Player");
            var move = map.AddAction("Move", InputActionType.Value);
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Left", "<Keyboard>/a")
                .With("Down", "<Keyboard>/s")
                .With("Right", "<Keyboard>/d");
            map.AddAction("Run", InputActionType.Button, "<Keyboard>/leftShift");
            map.AddAction("Interact", InputActionType.Button, "<Keyboard>/e");
            asset.AddActionMap(map);
            return asset;
        }

        private static void SetField(object target, string name, object value)
        {
            target.GetType()
                .GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(target, value);
        }

        private void SpawnDialogueUi()
        {
            var go = new GameObject("DialogueUI");
            go.SetActive(false);

            var cardRoot = new GameObject("DialogueCard");
            cardRoot.transform.SetParent(go.transform, false);
            var nameText = new GameObject("NameText").AddComponent<Text>();
            nameText.transform.SetParent(cardRoot.transform, false);
            var bodyText = new GameObject("BodyText").AddComponent<Text>();
            bodyText.transform.SetParent(cardRoot.transform, false);
            cardRoot.SetActive(false);

            card = go.AddComponent<DialogueCard>();
            SetField(card, "cardRoot", cardRoot);
            SetField(card, "nameText", nameText);
            SetField(card, "bodyText", bodyText);

            controller = go.AddComponent<DialogueController>();
            SetField(controller, "card", card);
            SetField(controller, "inputActions", actions);

            dialogueUiObject = go;
            go.SetActive(true);
        }

        private Interactor SpawnPlayer(Vector2 position)
        {
            var go = new GameObject("Player");
            go.SetActive(false);
            var interactor = go.AddComponent<Interactor>();
            SetField(interactor, "inputActions", actions);
            var movement = go.AddComponent<PlayerMovement>();
            SetField(movement, "inputActions", actions);
            go.transform.position = position;
            go.SetActive(true);
            playerObject = go;
            return interactor;
        }

        private NpcInteractable SpawnNpc(Vector2 position, string displayName,
            string[] lines)
        {
            var go = new GameObject("Npc");
            go.transform.position = position;
            var collider = go.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.4f;
            var npc = go.AddComponent<NpcComponent>();
            SetField(npc, "displayName", displayName);
            SetField(npc, "lines", lines);
            var interactable = go.AddComponent<NpcInteractable>();
            npcObject = go;
            Physics2D.SyncTransforms();
            return interactable;
        }

        private IEnumerator PressInteract()
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
        }

        [UnityTest]
        public IEnumerator InteractAction_OpensCardWithNameAndLine()
        {
            SpawnPlayer(Vector2.zero);
            SpawnNpc(new Vector2(1f, 0f), "Fountain", FountainLines);
            yield return null;

            yield return PressInteract();

            Assert.That(DialogueController.IsOpen, Is.True);
            Assert.That(card.IsVisible, Is.True);
            Assert.That(card.CurrentName, Is.EqualTo("Fountain"));
            Assert.That(card.CurrentLine, Is.EqualTo(FountainLines[0]));
        }

        [UnityTest]
        public IEnumerator InteractAction_AdvancesThroughLines()
        {
            string[] lines = { "Hello there.", "Lovely day, isn't it?" };
            SpawnPlayer(Vector2.zero);
            SpawnNpc(new Vector2(1f, 0f), "Herbalist", lines);
            yield return null;

            yield return PressInteract();
            Assert.That(card.CurrentLine, Is.EqualTo(lines[0]));

            yield return PressInteract();
            Assert.That(DialogueController.IsOpen, Is.True);
            Assert.That(card.CurrentLine, Is.EqualTo(lines[1]));
        }

        [UnityTest]
        public IEnumerator InteractAction_LastLine_ClosesCard()
        {
            SpawnPlayer(Vector2.zero);
            var target = SpawnNpc(new Vector2(1f, 0f), "Fountain", FountainLines);
            yield return null;
            yield return PressInteract();
            Assert.That(DialogueController.IsOpen, Is.True);

            yield return PressInteract();

            Assert.That(DialogueController.IsOpen, Is.False);
            Assert.That(card.IsVisible, Is.False);
            // The closing press must not immediately reopen the dialogue.
            Assert.That(target.InteractCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator DialogueOpen_SuppressesPlayerMovement()
        {
            SpawnPlayer(Vector2.zero);
            yield return null;
            InputAction moveAction = actions.FindActionMap("Player").FindAction("Move");

            controller.OpenDialogue("Fountain", FountainLines);
            yield return null;

            Assert.That(DialogueController.IsOpen, Is.True);
            Assert.That(moveAction.enabled, Is.False);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D));
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.That(playerObject.transform.position.x, Is.EqualTo(0f).Within(0.001f));

            controller.Close();
            Assert.That(moveAction.enabled, Is.True);
        }
    }
}
