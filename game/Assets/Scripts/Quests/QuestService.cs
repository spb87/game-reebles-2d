// Scene-level quest orchestrator: owns the active quest, the player's heart
// total, and the pickup/delivery gates the interactables consult. Pushes
// objective/hearts/toast updates to the HUD through HudViewLocator (an
// Interaction-assembly surface) so neither side needs a reference that would
// create a Quests<->UI assembly cycle.

using System.Collections.Generic;
using Reebles2D.Interaction;
using UnityEngine;

namespace Reebles2D.Quests
{
    /// <summary>
    /// Singleton quest state machine. Start a quest with
    /// <see cref="StartQuest"/>; <see cref="RequestPickup"/> gates world-item
    /// pickup on the active quest's fetch target plus an empty
    /// <see cref="CarrySlot"/>; <see cref="Deliver"/> completes the quest and
    /// awards hearts when the giver NPC receives the fetch item.
    /// </summary>
    public class QuestService : MonoBehaviour
    {
        /// <summary>The live service (one per scene), or null.</summary>
        public static QuestService Instance { get; private set; }

        /// <summary>The quest in progress, or null.</summary>
        public QuestDef ActiveQuest { get; private set; }

        /// <summary>Total hearts earned from completed quests.</summary>
        public int Hearts { get; private set; }

        private readonly HashSet<string> completedQuestIds = new HashSet<string>();
        private Dictionary<string, QuestDef> questsById;

        private void Awake()
        {
            Instance = this;
            questsById = QuestLibrary.LoadById();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            // Push the initial (empty) state after every Awake has run so the
            // HUD is guaranteed registered regardless of script order.
            PushState();
        }

        /// <summary>Looks up a quest definition by id (null when unknown).</summary>
        public QuestDef QuestFor(string questId)
        {
            return questId != null && questsById.TryGetValue(questId, out QuestDef quest)
                ? quest
                : null;
        }

        /// <summary>True when <paramref name="questId"/> has been delivered.</summary>
        public bool IsCompleted(string questId)
        {
            return completedQuestIds.Contains(questId);
        }

        /// <summary>
        /// Makes the quest active. Returns false when another quest is already
        /// active, the id is unknown, or the quest was already completed.
        /// </summary>
        public bool StartQuest(string questId)
        {
            if (ActiveQuest != null || IsCompleted(questId))
            {
                return false;
            }
            QuestDef quest = QuestFor(questId);
            if (quest == null)
            {
                Debug.LogError($"QuestService: unknown quest id '{questId}'");
                return false;
            }
            ActiveQuest = quest;
            PushState();
            return true;
        }

        /// <summary>
        /// True when the active quest fetches from <paramref name="targetId"/>
        /// and the player's carry slot is free.
        /// </summary>
        public bool CanPickup(string targetId)
        {
            return ActiveQuest != null
                && ActiveQuest.fetchTargetId == targetId
                && CarrySlot.Instance != null
                && CarrySlot.Instance.IsEmpty;
        }

        /// <summary>
        /// Attempts to pick up the active quest's fetch item from
        /// <paramref name="targetId"/>. Failure paths toast the reason
        /// ("not now" / "hands full"); success fills the slot and toasts.
        /// </summary>
        public bool RequestPickup(string targetId)
        {
            if (ActiveQuest == null || ActiveQuest.fetchTargetId != targetId)
            {
                Toast("Nothing to pick here right now.");
                return false;
            }
            if (CarrySlot.Instance == null)
            {
                Debug.LogError("QuestService: no CarrySlot in scene — " +
                    "cannot pick up quest items");
                return false;
            }
            if (!CarrySlot.Instance.TryPickup(ActiveQuest.fetchItemId))
            {
                Toast("Your hands are full.");
                return false;
            }
            Toast($"Picked up {ActiveQuest.fetchItemId}!");
            return true;
        }

        /// <summary>
        /// True when <paramref name="npcId"/> is the active quest's giver and
        /// the player is carrying the fetch item.
        /// </summary>
        public bool CanDeliver(string npcId)
        {
            return ActiveQuest != null
                && ActiveQuest.giverNpcId == npcId
                && CarrySlot.Instance != null
                && CarrySlot.Instance.CurrentItemId == ActiveQuest.fetchItemId;
        }

        /// <summary>
        /// Completes the active quest at giver <paramref name="npcId"/>:
        /// consumes the carried item, awards the heart reward, and clears the
        /// active quest. Returns false when <see cref="CanDeliver"/> fails.
        /// </summary>
        public bool Deliver(string npcId, string npcName = null)
        {
            if (!CanDeliver(npcId))
            {
                return false;
            }
            QuestDef completed = ActiveQuest;
            CarrySlot.Instance.TryDeliver(completed.fetchItemId);
            Hearts += completed.rewardHearts;
            completedQuestIds.Add(completed.id);
            ActiveQuest = null;
            string thanker = string.IsNullOrEmpty(npcName) ? completed.title : npcName;
            Toast($"+{completed.rewardHearts} heart! {thanker} thanks you!");
            PushState();
            return true;
        }

        private void PushState()
        {
            HudViewLocator.Current?.SetObjective(
                ActiveQuest != null ? ActiveQuest.objectiveText : "");
            HudViewLocator.Current?.SetHearts(Hearts);
        }

        private void Toast(string message)
        {
            IHudView hud = HudViewLocator.Current;
            if (hud != null)
            {
                hud.ShowToast(message);
            }
            else
            {
                Debug.Log(message);
            }
        }
    }
}
