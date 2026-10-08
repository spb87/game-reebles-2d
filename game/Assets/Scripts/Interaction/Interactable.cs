using UnityEngine;

namespace Reebles2D.Interaction
{
    /// <summary>
    /// World object the player can interact with (NPC, pickup bush, fountain).
    /// Carries the approach radius and prompt wording; <see cref="Interact"/> is
    /// a virtual hook — subclasses or subclasses-to-come (pickup, dialogue)
    /// override it. Needs a collider (trigger or solid) on the same object so
    /// <see cref="Interactor"/>'s OverlapCircleAll scan can find it.
    /// </summary>
    public class Interactable : MonoBehaviour
    {
        [SerializeField] private float interactRadius = 1.5f;
        [SerializeField] private string promptVerb = "Interact";
        [SerializeField] private string flavorLine = "";
        [SerializeField] private float promptHeight = 1.2f;

        /// <summary>Max distance from the player at which this target activates.</summary>
        public float InteractRadius => interactRadius;

        /// <summary>Verb shown by the prompt bubble ("Talk", "Pick berries").</summary>
        public string PromptVerb => promptVerb;

        /// <summary>Vertical offset from the pivot where the prompt bubble floats.</summary>
        public float PromptHeight => promptHeight;

        /// <summary>Times <see cref="Interact"/> has fired — useful for tests.</summary>
        public int InteractCount { get; private set; }

        /// <summary>
        /// Performs the interaction. The base implementation only counts the
        /// call and logs the flavor line; concrete behavior (pickup, dialogue)
        /// arrives in later tasks via overrides.
        /// </summary>
        public virtual void Interact()
        {
            InteractCount++;
            if (!string.IsNullOrEmpty(flavorLine))
            {
                Debug.Log(flavorLine);
            }
        }
    }
}
