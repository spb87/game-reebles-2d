// Persistent heads-up display: quest objective top-left, heart counter
// top-right, and a bottom-center toast that fades after a few seconds.
// Implements IHudView (Interaction assembly) and registers with
// HudViewLocator so QuestService can push updates without a UI->Quests
// assembly reference.

using Reebles2D.Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace Reebles2D.UI
{
    /// <summary>
    /// uGUI HUD driven entirely by <see cref="IHudView"/> pushes. The toast is
    /// a Text inside a CanvasGroup whose alpha eases out over
    /// <see cref="toastFadeSeconds"/> once <see cref="toastDurationSeconds"/>
    /// has elapsed.
    /// </summary>
    public class Hud : MonoBehaviour, IHudView
    {
        [SerializeField] private Text objectiveText;
        [SerializeField] private Text heartsText;
        [SerializeField] private Text toastText;
        [SerializeField] private CanvasGroup toastGroup;
        [SerializeField] private float toastDurationSeconds = 2.5f;
        [SerializeField] private float toastFadeSeconds = 0.5f;

        private float toastTimer;

        /// <summary>Current objective line (for tests).</summary>
        public string ObjectiveText => objectiveText != null ? objectiveText.text : "";

        /// <summary>Current hearts line (for tests).</summary>
        public string HeartsText => heartsText != null ? heartsText.text : "";

        /// <summary>Current toast message (for tests).</summary>
        public string ToastText => toastText != null ? toastText.text : "";

        /// <summary>Current toast alpha, 0–1 (for tests).</summary>
        public float ToastAlpha => toastGroup != null ? toastGroup.alpha : 0f;

        private void OnEnable()
        {
            HudViewLocator.Current = this;
        }

        private void OnDisable()
        {
            if (ReferenceEquals(HudViewLocator.Current, this))
            {
                HudViewLocator.Current = null;
            }
        }

        /// <summary>Sets the objective line; empty hides it.</summary>
        public void SetObjective(string text)
        {
            if (objectiveText != null)
            {
                objectiveText.text = text ?? "";
            }
        }

        /// <summary>Renders the heart counter as "♥ N".</summary>
        public void SetHearts(int hearts)
        {
            if (heartsText != null)
            {
                heartsText.text = "♥ " + hearts;
            }
        }

        /// <summary>Shows a toast that fades after toastDurationSeconds.</summary>
        public void ShowToast(string message)
        {
            if (toastText != null)
            {
                toastText.text = message;
            }
            toastTimer = toastDurationSeconds;
            if (toastGroup != null)
            {
                toastGroup.alpha = 1f;
            }
        }

        private void Update()
        {
            if (toastTimer <= 0f || toastGroup == null)
            {
                return;
            }
            toastTimer -= Time.deltaTime;
            if (toastTimer <= 0f)
            {
                toastGroup.alpha = 0f;
            }
            else
            {
                // Hold full alpha until the final fade window, then ease out.
                toastGroup.alpha = Mathf.Clamp01(
                    (toastTimer - (toastDurationSeconds - toastFadeSeconds))
                    / toastFadeSeconds + 1f);
            }
        }
    }
}
