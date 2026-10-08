#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Reebles2D.Interaction;
using Reebles2D.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Reebles2D.Tests.PlayMode
{
    /// <summary>
    /// PlayMode coverage for the generated Npc.prefab (REEB-152): the villager
    /// carries a sprite, a solid footprint collider, dialogue data and a shadow
    /// child; E in range opens the card as 'Marla the Baker' and advancing
    /// through her lines closes it; the saved Village scene references the
    /// prefab. Objects spawn in code — no scene load, nothing to restore.
    /// </summary>
    public class NpcVillagerTests : InputTestFixture
    {
        private const string NpcPrefabPath = "Assets/Prefabs/Npc.prefab";
        private const string VillageScenePath = "Assets/Scenes/Village.unity";

        private Keyboard keyboard;
        private InputActionAsset actions;
        private GameObject dialogueUiObject;
        private GameObject playerObject;
        private GameObject npcObject;
        private DialogueCard card;
        private Interactor interactor;
        private NpcComponent npc;
        private NpcInteractable npcInteractable;

        public override void Setup()
        {
            base.Setup();
            keyboard = InputSystem.AddDevice<Keyboard>();
            actions = CreateActions();
            SpawnDialogueUi();
            SpawnNpc();
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
            map.AddAction("Move", InputActionType.Value)
                .AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Left", "<Keyboard>/a")
                .With("Down", "<Keyboard>/s")
                .With("Right", "<Keyboard>/d");
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
            var controller = go.AddComponent<DialogueController>();
            SetField(controller, "card", card);
            SetField(controller, "inputActions", actions);
            dialogueUiObject = go;
            go.SetActive(true);
        }

        private void SpawnNpc()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(NpcPrefabPath);
            Assert.That(prefab, Is.Not.Null,
                "Npc.prefab missing — run VillageSceneBuilder.Build");
            npcObject = Object.Instantiate(prefab, new Vector3(1f, 0f, 0f),
                Quaternion.identity);
            npc = npcObject.GetComponent<NpcComponent>();
            npcInteractable = npcObject.GetComponent<NpcInteractable>();
            Physics2D.SyncTransforms();
        }

        private void SpawnPlayer(Vector2 position)
        {
            var go = new GameObject("Player");
            go.SetActive(false);
            interactor = go.AddComponent<Interactor>();
            SetField(interactor, "inputActions", actions);
            go.transform.position = position;
            go.SetActive(true);
            playerObject = go;
        }

        private IEnumerator PressInteract()
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
        }

        [Test]
        public void Prefab_WiresVillagerComponents()
        {
            var renderer = npcObject.GetComponent<SpriteRenderer>();
            Assert.That(renderer.sprite, Is.Not.Null);

            var collider = npcObject.GetComponent<CircleCollider2D>();
            Assert.That(collider, Is.Not.Null, "footprint collider missing");
            Assert.That(collider.isTrigger, Is.False, "footprint must be solid");
            float worldRadius = collider.radius * npcObject.transform.localScale.x;
            Assert.That(worldRadius, Is.EqualTo(0.35f).Within(0.01f));

            Assert.That(npc.DisplayName, Is.EqualTo("Marla the Baker"));
            Assert.That(npc.Lines.Length, Is.GreaterThanOrEqualTo(2));
            Assert.That(npcInteractable.PromptVerb, Is.EqualTo("Talk"));
            Assert.That(npcInteractable.InteractRadius,
                Is.EqualTo(1.75f).Within(0.01f));

            Transform shadow = npcObject.transform.Find("Shadow");
            Assert.That(shadow, Is.Not.Null, "shadow child missing");
            Assert.That(shadow.GetComponent<SpriteRenderer>().sortingOrder,
                Is.EqualTo(-9));
        }

        [UnityTest]
        public IEnumerator Talk_OpensCard_Advances_Closes()
        {
            SpawnPlayer(Vector2.zero);
            yield return null;
            Assert.That(interactor.CurrentTarget,
                Is.EqualTo((Interactable)npcInteractable));

            yield return PressInteract();
            Assert.That(DialogueController.IsOpen, Is.True);
            Assert.That(card.CurrentName, Is.EqualTo("Marla the Baker"));
            Assert.That(card.CurrentLine, Is.EqualTo(npc.Lines[0]));

            for (int i = 1; i < npc.Lines.Length; i++)
            {
                yield return PressInteract();
                Assert.That(card.CurrentLine, Is.EqualTo(npc.Lines[i]));
            }
            yield return PressInteract();
            Assert.That(DialogueController.IsOpen, Is.False);
            Assert.That(card.IsVisible, Is.False);
        }

        [Test]
        public void VillageScene_ReferencesNpcPrefab()
        {
            string guid = AssetDatabase.AssetPathToGUID(NpcPrefabPath);
            string yaml = File.ReadAllText(VillageScenePath);
            Assert.That(yaml, Does.Contain(guid),
                "Village.unity has no Npc.prefab instance — rebuild the scene");
        }
    }
}
#endif
