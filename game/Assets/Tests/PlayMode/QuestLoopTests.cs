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
                    giverObject, bushObject })
            {
                if (go != null)
                {
                    UnityEngine.Object.Destroy(go);
                }
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

        private void SpawnDialogueUi()
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
    }
}
#endif
