// World-side half of a fetch quest: an Interactable whose Interact() asks
// QuestService to fill the player's carry slot. All gating (quest active,
// target match, slot free) lives in QuestService so this stays a thin shell.

using Reebles2D.Interaction;
using UnityEngine;

namespace Reebles2D.Quests
{
    /// <summary>
    /// Interactable quest pickup source (e.g., the berry bush). Interact
    /// delegates to <see cref="QuestService.RequestPickup"/>; the interact
    /// only counts on success so tests can distinguish gated calls.
    /// </summary>
    public class QuestItemInteractable : Interactable
    {
        [SerializeField] private string targetId;

        /// <summary>Fetch-target id this source matches in quest JSON.</summary>
        public string TargetId => targetId;

        /// <summary>
        /// Requests the pickup through QuestService. Falls back to the base
        /// flavor-line behavior when no QuestService exists in the scene.
        /// </summary>
        public override void Interact()
        {
            if (QuestService.Instance == null)
            {
                base.Interact();
                return;
            }
            if (QuestService.Instance.RequestPickup(targetId))
            {
                base.Interact();
            }
        }
    }
}
