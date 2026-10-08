// Data component for anything that speaks through the dialogue card — a named
// speaker plus its line list. Reused for flavor objects (the fountain) by
// giving the object a display name and a single line.

using UnityEngine;

namespace Reebles2D.UI
{
    /// <summary>
    /// Holds the speaker name and dialogue lines an
    /// <see cref="NpcInteractable"/> feeds to <see cref="DialogueController"/>.
    /// </summary>
    public class NpcComponent : MonoBehaviour
    {
        [SerializeField] private string displayName = "???";
        [SerializeField, TextArea] private string[] lines = new string[0];

        /// <summary>Name shown in the card header.</summary>
        public string DisplayName => displayName;

        /// <summary>Dialogue lines, in order.</summary>
        public string[] Lines => lines;
    }
}
