#if UNITY_EDITOR
using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using Reebles2D.Interaction;
using Reebles2D.Quests;
using Reebles2D.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Reebles2D.Tests.PlayMode
{
    /// <summary>
    /// PlayMode coverage for the REEB-154 quest loop: CarrySlot gating,
    /// QuestService pickup/delivery, QuestGiverInteractable phase lines, and
    /// Hud objective/hearts/toast updates. Objects spawn in code — no scene
    /// load, nothing to restore.
    /// </summary>
    public class QuestLoopTests : InputTestFixture
    {
        private const string QuestId = "errand_berries";
        private const string GiverNpcId = "marla_baker";
        private const string FetchItemId = "berries";
        private const string FetchTargetId = "berry_bush";

        private GameObject serviceObject;
        private GameObject slotObject;
        private GameObject hudObject;
        private GameObject dialogueObject;
        private GameObject giverObject;
        private GameObject bushObject;

        private QuestService service;
        private CarrySlot slot;
        private Hud hud;
        private DialogueCard card;
        private QuestGiverInteractable giver;
        private NpcComponent giverNpc;
        private QuestItemInteractable bush;
        private GameObject e2ePlayer;
        private GameObject e2eMarla;
        private InputActionAsset e2eActions;

        public override void Setup()
        {
            base.Setup();
            SpawnHud();
            SpawnService();
            SpawnSlot();
            SpawnDialogueUi();
            SpawnGiver();
            SpawnBush();
        }

        public override void TearDown()
        {
            foreach (GameObject go in new[]
                { serviceObject, slotObject, hudObject, dialogueObject,
                    giverObject, bushObject, e2ePlayer, e2eMarla })
            {
                if (go != null)
                {
                    UnityEngine.Object.Destroy(go);
                }
            }
            if (e2eActions != null)
            {
                UnityEngine.Object.Destroy(e2eActions);
                e2eActions = null;
            }
            HudViewLocator.Current = null;
            base.TearDown();
        }

        private static void SetField(object target, string name, object value)
        {
            // Private fields aren't returned for base types — walk the chain.
            for (Type type = target.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name,
                    BindingFlags.NonPublic | BindingFlags.Instance
                    | BindingFlags.DeclaredOnly);
                if (field != null)
                {
                    field.SetValue(target, value);
                    return;
                }
            }
            throw new ArgumentException(
                $"No field '{name}' on {target.GetType()} or its bases");
        }

        private void SpawnService()
        {
            serviceObject = new GameObject("QuestService");
            service = serviceObject.AddComponent<QuestService>();
        }

        private void SpawnSlot()
        {
            slotObject = new GameObject("Player");
            slot = slotObject.AddComponent<CarrySlot>();
        }

        private void SpawnHud()
        {
            hudObject = new GameObject("Hud");
            hudObject.SetActive(false);
            var objective = new GameObject("ObjectiveText").AddComponent<Text>();
            objective.transform.SetParent(hudObject.transform, false);
            var hearts = new GameObject("HeartsText").AddComponent<Text>();
            hearts.transform.SetParent(hudObject.transform, false);
            var toastRoot = new GameObject("Toast");
            toastRoot.transform.SetParent(hudObject.transform, false);
            var toastGroup = toastRoot.AddComponent<CanvasGroup>();
            var toast = toastRoot.AddComponent<Text>();

            hud = hudObject.AddComponent<Hud>();
            SetField(hud, "objectiveText", objective);
            SetField(hud, "heartsText", hearts);
            SetField(hud, "toastText", toast);
            SetField(hud, "toastGroup", toastGroup);
            hudObject.SetActive(true);
        }

        private void SpawnDialogueUi(InputActionAsset actions = null)
        {
            dialogueObject = new GameObject("DialogueUI");
            dialogueObject.SetActive(false);
            var cardRoot = new GameObject("DialogueCard");
            cardRoot.transform.SetParent(dialogueObject.transform, false);
            var nameText = new GameObject("NameText").AddComponent<Text>();
            nameText.transform.SetParent(cardRoot.transform, false);
            var bodyText = new GameObject("BodyText").AddComponent<Text>();
            bodyText.transform.SetParent(cardRoot.transform, false);
            cardRoot.SetActive(false);

            card = dialogueObject.AddComponent<DialogueCard>();
            SetField(card, "cardRoot", cardRoot);
            SetField(card, "nameText", nameText);
            SetField(card, "bodyText", bodyText);
            var controller = dialogueObject.AddComponent<DialogueController>();
            SetField(controller, "card", card);
            SetField(controller, "inputActions", actions);
            dialogueObject.SetActive(true);
        }

        private void SpawnGiver()
        {
            giverObject = new GameObject("Marla");
            giverObject.SetActive(false);
            giverNpc = giverObject.AddComponent<NpcComponent>();
            SetField(giverNpc, "displayName", "Marla the Baker");
            giver = giverObject.AddComponent<QuestGiverInteractable>();
            SetField(giver, "npcId", GiverNpcId);
            SetField(giver, "questId", QuestId);
            // NpcInteractable's private Awake would lazy-bind npc, but Unity
            // only invokes the most-derived Awake — wire it explicitly.
            SetField(giver, "npc", giverNpc);
            giverObject.SetActive(true);
        }

        private void SpawnBush()
        {
            bushObject = new GameObject("BerryBush");
            bush = bushObject.AddComponent<QuestItemInteractable>();
            SetField(bush, "targetId", FetchTargetId);
        }

        private void CloseOpenDialogue()
        {
            if (DialogueController.Instance != null
                && DialogueController.Instance.IsDialogueOpen)
            {
                DialogueController.Instance.Close();
            }
        }

        /// <summary>
        /// Waits until Time.frameCount advances past the frame the dialogue
        /// closed on — Interact guards reject presses on LastClosedFrame.
        /// </summary>
        private static IEnumerator NextFrame()
        {
            int frame = Time.frameCount;
            while (Time.frameCount == frame)
            {
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator Pickup_NoQuestActive_LeavesSlotEmpty()
        {
            yield return null; // let QuestService.Start push initial state

            bush.Interact();
            Assert.That(slot.IsEmpty, Is.True);
            Assert.That(bush.InteractCount, Is.EqualTo(0));
            Assert.That(hud.ToastText, Is.EqualTo("Nothing to pick here right now."));
            Assert.That(hud.ToastAlpha, Is.EqualTo(1f));
        }

        [UnityTest]
        public IEnumerator Pickup_QuestActive_FillsSlot_AndBlocksSecondGrab()
        {
            Assert.That(service.StartQuest(QuestId), Is.True);
            yield return null;

            bush.Interact();
            Assert.That(slot.CurrentItemId, Is.EqualTo(FetchItemId));
            Assert.That(bush.InteractCount, Is.EqualTo(1));
            Assert.That(hud.ToastText, Is.EqualTo("Picked up berries!"));

            bush.Interact();
            Assert.That(slot.CurrentItemId, Is.EqualTo(FetchItemId),
                "second pickup must not replace the carried item");
            Assert.That(bush.InteractCount, Is.EqualTo(1));
            Assert.That(hud.ToastText, Is.EqualTo("Your hands are full."));
        }

        [UnityTest]
        public IEnumerator Pickup_WrongTarget_Fails()
        {
            Assert.That(service.StartQuest(QuestId), Is.True);
            yield return null;

            var otherObject = new GameObject("OtherBush");
            var other = otherObject.AddComponent<QuestItemInteractable>();
            SetField(other, "targetId", "some_other_bush");
            other.Interact();
            Assert.That(slot.IsEmpty, Is.True);
            UnityEngine.Object.Destroy(otherObject);
        }

        [UnityTest]
        public IEnumerator Giver_FirstTalk_OffersAndStartsQuest()
        {
            yield return null;

            giver.Interact();
            Assert.That(service.ActiveQuest, Is.Not.Null);
            Assert.That(service.ActiveQuest.id, Is.EqualTo(QuestId));
            Assert.That(DialogueController.IsOpen, Is.True);
            Assert.That(card.CurrentName, Is.EqualTo("Marla the Baker"));
            Assert.That(card.CurrentLine,
                Is.EqualTo(service.QuestFor(QuestId).offerLines[0]));
            Assert.That(hud.ObjectiveText,
                Is.EqualTo(service.QuestFor(QuestId).objectiveText));
        }

        [UnityTest]
        public IEnumerator Giver_ActiveTalk_ShowsActiveLines()
        {
            Assert.That(service.StartQuest(QuestId), Is.True);
            yield return null;

            giver.Interact();
            Assert.That(DialogueController.IsOpen, Is.True);
            Assert.That(card.CurrentLine,
                Is.EqualTo(service.QuestFor(QuestId).activeLines[0]));
            Assert.That(service.Hearts, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Giver_CarryingItem_DeliversAndAwardsHeart()
        {
            Assert.That(service.StartQuest(QuestId), Is.True);
            Assert.That(slot.TryPickup(FetchItemId), Is.True);
            yield return null;

            giver.Interact();
            Assert.That(card.CurrentLine,
                Is.EqualTo(service.QuestFor(QuestId).completeLines[0]));
            Assert.That(service.ActiveQuest, Is.Null, "quest should complete");
            Assert.That(service.IsCompleted(QuestId), Is.True);
            Assert.That(service.Hearts, Is.EqualTo(1));
            Assert.That(slot.IsEmpty, Is.True, "delivered item must leave the slot");
            Assert.That(hud.HeartsText, Is.EqualTo("♥ 1"));
            Assert.That(hud.ObjectiveText, Is.EqualTo(""));
            Assert.That(hud.ToastText, Does.Contain("+1 heart"));
        }

        [UnityTest]
        public IEnumerator Giver_AfterComplete_FallsBackToFlavorLines()
        {
            Assert.That(service.StartQuest(QuestId), Is.True);
            Assert.That(slot.TryPickup(FetchItemId), Is.True);
            yield return null;
            giver.Interact();
            CloseOpenDialogue();
            yield return NextFrame(); // clear the same-frame reopen guard

            SetField(giverNpc, "lines", new[] { "Lovely day for baking." });
            giver.Interact();
            Assert.That(card.CurrentLine, Is.EqualTo("Lovely day for baking."));
        }

        [UnityTest]
        public IEnumerator Toast_FadesAfterDuration()
        {
            Assert.That(service.StartQuest(QuestId), Is.True);
            yield return null;
            bush.Interact();
            Assert.That(hud.ToastAlpha, Is.EqualTo(1f));

            float waited = 0f;
            while (waited < 3.2f && hud.ToastAlpha > 0f)
            {
                waited += Time.deltaTime;
                yield return null;
            }
            Assert.That(hud.ToastAlpha, Is.EqualTo(0f));
        }

        /// <summary>Minimal Player map with an Interact action bound to E.</summary>
        private static InputActionAsset CreateActions()
        {
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = new InputActionMap("Player");
            map.AddAction("Interact", InputActionType.Button, "<Keyboard>/e");
            asset.AddActionMap(map);
            return asset;
        }

        /// <summary>
        /// Queues a single E-key tap and waits for both the press and the
        /// release to be consumed — a held key never re-triggers
        /// WasPerformedThisFrame, so each press needs both halves.
        /// </summary>
        private static IEnumerator PressInteract(Keyboard keyboard)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
        }

        /// <summary>
        /// Taps E until <paramref name="done"/> reports the interaction
        /// landed, bounded so a real wiring break still fails fast. Input
        /// events occasionally miss a frame under batch-mode pacing, so the
        /// loop behaves like a player retrying a keypress.
        /// </summary>
        private static IEnumerator PressInteractUntil(Keyboard keyboard,
            Func<bool> done, int maxPresses = 8)
        {
            for (int i = 0; !done() && i < maxPresses; i++)
            {
                yield return PressInteract(keyboard);
            }
        }

        [UnityTest]
        public IEnumerator EndToEnd_OfferFetchDeliver_CompletesQuest()
        {
            // Swap the code-built stand-ins for the shipped prefabs so the
            // scan/interact path runs through the real wiring: colliders,
            // shared input asset, and the CarrySlot singleton.
            foreach (GameObject go in new[]
                { slotObject, giverObject, bushObject, dialogueObject })
            {
                UnityEngine.Object.Destroy(go);
            }
            slotObject = giverObject = bushObject = dialogueObject = null;

            var keyboard = InputSystem.AddDevice<Keyboard>();
            // Imported InputActionAsset instances can hold stale control
            // resolution under the test InputManager (actions report zero
            // bound controls), so the test drives E presses through a
            // per-test synthetic asset like InteractionTests/DialogueTests.
            e2eActions = CreateActions();
            SpawnDialogueUi(e2eActions); // so E taps advance/close lines

            var playerPrefab = UnityEditor.AssetDatabase
                .LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
            var npcPrefab = UnityEditor.AssetDatabase
                .LoadAssetAtPath<GameObject>("Assets/Prefabs/Npc.prefab");
            Assert.That(playerPrefab, Is.Not.Null, "Player prefab missing");
            Assert.That(npcPrefab, Is.Not.Null, "Npc prefab missing");

            e2ePlayer = UnityEngine.Object.Instantiate(
                playerPrefab, Vector3.zero, Quaternion.identity);
            e2eMarla = UnityEngine.Object.Instantiate(
                npcPrefab, new Vector3(1.4f, 0f, 0f), Quaternion.identity);
            var marla = e2eMarla.GetComponent<QuestGiverInteractable>();
            var interactor = e2ePlayer.GetComponent<Interactor>();
            Assert.That(marla, Is.Not.Null);
            Assert.That(interactor, Is.Not.Null);

            // Point the prefab's Interactor at the synthetic asset — its
            // interactAction was cached in Awake, so rebind both fields and
            // enable the map for this fixture's keyboard.
            InputActionMap e2eMap = e2eActions.FindActionMap("Player");
            SetField(interactor, "inputActions", e2eActions);
            SetField(interactor, "interactAction", e2eMap.FindAction("Interact"));
            e2eMap.Enable();

            bushObject = new GameObject("BerryBush");
            bushObject.transform.position = new Vector3(7f, 0f, 0f);
            var bushCollider = bushObject.AddComponent<CircleCollider2D>();
            bushCollider.isTrigger = true;
            bushCollider.radius = 0.4f;
            bush = bushObject.AddComponent<QuestItemInteractable>();
            SetField(bush, "targetId", FetchTargetId);
            Physics2D.SyncTransforms();

            yield return null; // QuestService.Start push + Interactor first scan
            Assert.That(hud.ObjectiveText, Is.EqualTo(""),
                "no objective before the quest is offered");
            Assert.That(interactor.CurrentTarget, Is.SameAs(marla));

            // Talk to Marla: offer lines show and the quest goes active.
            yield return PressInteractUntil(keyboard,
                () => DialogueController.IsOpen);
            Assert.That(service.ActiveQuest, Is.Not.Null,
                "interact press never reached Marla");
            Assert.That(service.ActiveQuest.id, Is.EqualTo(QuestId));
            Assert.That(DialogueController.IsOpen, Is.True);
            Assert.That(card.CurrentLine,
                Is.EqualTo(service.QuestFor(QuestId).offerLines[0]));
            Assert.That(hud.ObjectiveText,
                Is.EqualTo(service.QuestFor(QuestId).objectiveText));

            // Finish the offer dialogue — further E taps advance/close it.
            int guard = 0;
            while (DialogueController.IsOpen && guard++ < 10)
            {
                yield return PressInteract(keyboard);
            }
            Assert.That(DialogueController.IsOpen, Is.False,
                "offer dialogue should close after its lines");
            yield return NextFrame(); // clear the same-frame reopen guard

            // Walk to the bush and pick the berries.
            e2ePlayer.transform.position = new Vector3(6.4f, 0f, 0f);
            Physics2D.SyncTransforms();
            yield return null;
            Assert.That(interactor.CurrentTarget, Is.SameAs(bush));
            yield return PressInteractUntil(keyboard,
                () => !CarrySlot.Instance.IsEmpty);
            Assert.That(CarrySlot.Instance.CurrentItemId, Is.EqualTo(FetchItemId));
            Assert.That(hud.ToastText, Is.EqualTo("Picked up berries!"));

            // Back to Marla: delivery completes the quest and awards a heart.
            e2ePlayer.transform.position = Vector3.zero;
            Physics2D.SyncTransforms();
            yield return null;
            Assert.That(interactor.CurrentTarget, Is.SameAs(marla));
            yield return PressInteractUntil(keyboard,
                () => service.Hearts > 0);
            Assert.That(card.CurrentLine,
                Is.EqualTo(service.QuestFor(QuestId).completeLines[0]));
            Assert.That(service.ActiveQuest, Is.Null);
            Assert.That(service.IsCompleted(QuestId), Is.True);
            Assert.That(service.Hearts, Is.EqualTo(1));
            Assert.That(CarrySlot.Instance.IsEmpty, Is.True);
            Assert.That(hud.HeartsText, Is.EqualTo("♥ 1"));
            Assert.That(hud.ToastText, Does.Contain("+1 heart"));
            Assert.That(hud.ObjectiveText, Is.EqualTo(""));
        }
    }
}
#endif
