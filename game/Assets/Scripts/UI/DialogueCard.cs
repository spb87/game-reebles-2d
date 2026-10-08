// View-only wrapper around the generated dialogue card hierarchy: the builder
// assembles the panel + texts, this script just toggles visibility and fills
// the name header and body line.

using UnityEngine;
using UnityEngine.UI;

namespace Reebles2D.UI
{
    /// <summary>
    /// Bottom-center dialogue card. <see cref="DialogueController"/> owns the
    /// open/close state; this class only renders one line at a time.
    /// </summary>
    public class DialogueCard : MonoBehaviour
    {
        [SerializeField] private GameObject cardRoot;
        [SerializeField] private Text nameText;
        [SerializeField] private Text bodyText;

        /// <summary>True while the card panel is active.</summary>
        public bool IsVisible => cardRoot != null && cardRoot.activeSelf;

        /// <summary>Text currently shown in the name header (for tests).</summary>
        public string CurrentName => nameText != null ? nameText.text : "";

        /// <summary>Text currently shown in the body (for tests).</summary>
        public string CurrentLine => bodyText != null ? bodyText.text : "";

        /// <summary>Shows the card with the given speaker name and dialogue line.</summary>
        public void Show(string speaker, string line)
        {
            if (nameText != null)
            {
                nameText.text = speaker;
            }
            if (bodyText != null)
            {
                bodyText.text = line;
            }
            if (cardRoot != null)
            {
                cardRoot.SetActive(true);
            }
        }

        /// <summary>Replaces only the body line (advance within one dialogue).</summary>
        public void SetLine(string line)
        {
            if (bodyText != null)
            {
                bodyText.text = line;
            }
        }

        /// <summary>Hides the card panel.</summary>
        public void Hide()
        {
            if (cardRoot != null)
            {
                cardRoot.SetActive(false);
            }
        }
    }
}
