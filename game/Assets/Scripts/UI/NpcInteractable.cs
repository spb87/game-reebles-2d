// Interactable that opens the dialogue card for its sibling NpcComponent.
// Lives in the UI assembly (which references Interaction) so Interactable
// itself stays UI-agnostic.

using Reebles2D.Interaction;
using UnityEngine;

namespace Reebles2D.UI
{
    /// <summary>
    /// On <see cref="Interact"/>, hands the sibling <see cref="NpcComponent"/>'s
    /// name and lines to <see cref="DialogueController"/>. Repeated Interact
    /// presses while the card is open are ignored — the controller owns
    /// advancing and closing.
    /// </summary>
    [RequireComponent(typeof(NpcComponent))]
    public class NpcInteractable : Interactable
    {
        [SerializeField] private NpcComponent npc;

        private void Awake()
        {
            if (npc == null)
            {
                npc = GetComponent<NpcComponent>();
            }
        }

        /// <summary>
        /// Opens the dialogue card. No-ops while a dialogue is open or on the
        /// frame one just closed — the closing Interact press would otherwise
        /// reopen the card when the Interactor runs after the controller.
        /// </summary>
        public override void Interact()
        {
            if (DialogueController.IsOpen
                || DialogueController.LastClosedFrame == Time.frameCount)
            {
                return;
            }
            base.Interact();
            if (npc != null)
            {
                DialogueController.Instance?.OpenDialogue(npc.DisplayName, npc.Lines);
            }
        }
    }
}
