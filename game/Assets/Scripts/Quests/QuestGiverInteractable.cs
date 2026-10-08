// NPC-side half of a fetch quest: an NpcInteractable that picks dialogue
// lines by quest phase instead of always playing the flavor lines. Phase
// order per Interact: carrying the fetch item -> completeLines + Deliver;
// quest active -> activeLines; quest available -> offerLines + StartQuest;
// otherwise -> the NpcComponent flavor lines.

using Reebles2D.UI;
using UnityEngine;

namespace Reebles2D.Quests
{
    /// <summary>
    /// Quest-giver variant of <see cref="NpcInteractable"/>. Wired with the
    /// giver's npcId and questId; falls back to plain NPC dialogue whenever
    /// the quest is unavailable, finished, or another quest is active.
    /// </summary>
    public class QuestGiverInteractable : NpcInteractable
    {
        [SerializeField] private string npcId;
        [SerializeField] private string questId;

        private NpcComponent questNpc;

        /// <summary>Npc id matched against <see cref="QuestDef.giverNpcId"/>.</summary>
        public string NpcId => npcId;

        /// <summary>Quest this giver offers and completes.</summary>
        public string QuestId => questId;

        private void Awake()
        {
            questNpc = GetComponent<NpcComponent>();
        }

        /// <summary>
        /// Opens the phase-appropriate dialogue. Delivery happens on the same
        /// press that opens the completion lines — the toast lands while the
        /// card is still open.
        /// </summary>
        public override void Interact()
        {
            if (DialogueController.IsOpen
                || DialogueController.LastClosedFrame == Time.frameCount)
            {
                return;
            }

            QuestService service = QuestService.Instance;
            QuestDef quest = service != null ? service.QuestFor(questId) : null;
            if (service == null || quest == null)
            {
                base.Interact();
                return;
            }

            bool delivering = service.CanDeliver(npcId);
            bool thisQuestActive = service.ActiveQuest != null
                && service.ActiveQuest.id == questId;

            if (delivering)
            {
                OpenLines(quest.completeLines);
                service.Deliver(npcId, questNpc != null ? questNpc.DisplayName : null);
            }
            else if (thisQuestActive)
            {
                OpenLines(quest.activeLines);
            }
            else if (service.ActiveQuest == null && !service.IsCompleted(questId))
            {
                OpenLines(quest.offerLines);
                service.StartQuest(questId);
            }
            else
            {
                // Quest done or busy elsewhere — plain flavor dialogue.
                base.Interact();
            }
        }

        private void OpenLines(string[] lines)
        {
            // NOT base.Interact(): NpcInteractable would open the flavor
            // NpcComponent lines first and the quest lines could never show.
            if (questNpc != null && lines != null && lines.Length > 0)
            {
                DialogueController.Instance?.OpenDialogue(questNpc.DisplayName, lines);
            }
        }
    }
}
