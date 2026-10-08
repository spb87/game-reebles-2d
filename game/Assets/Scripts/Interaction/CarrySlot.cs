// Player carry slot: holds at most one quest item ("scope's carry slot").
// Also hosts IHudView/HudViewLocator — small secondary types that live in the
// Interaction assembly so QuestService (Quests asmdef) and Hud (UI asmdef)
// can communicate without a Quests<->UI assembly reference cycle:
// QuestService pushes to the locator, Hud registers itself as the view.

using UnityEngine;

namespace Reebles2D.Interaction
{
    /// <summary>
    /// View surface the HUD exposes to systems that must not reference the UI
    /// assembly. Implemented by <c>Reebles2D.UI.Hud</c> and discovered through
    /// <see cref="HudViewLocator"/>.
    /// </summary>
    public interface IHudView
    {
        /// <summary>Shows the active objective text (empty string hides it).</summary>
        void SetObjective(string text);

        /// <summary>Updates the heart counter display.</summary>
        void SetHearts(int hearts);

        /// <summary>Shows a transient toast message that fades on its own.</summary>
        void ShowToast(string message);
    }

    /// <summary>
    /// Static registry for the scene's <see cref="IHudView"/>. The Hud sets
    /// <see cref="Current"/> on enable; QuestService reads it to push state.
    /// </summary>
    public static class HudViewLocator
    {
        /// <summary>The registered HUD view, or null when no HUD exists.</summary>
        public static IHudView Current { get; set; }
    }

    /// <summary>
    /// One-item carry slot on the player. <see cref="TryPickup"/> fails while
    /// occupied; <see cref="TryDeliver"/> clears the slot only when the item
    /// matches. Singleton via <see cref="Instance"/> so QuestService can reach
    /// the player's slot without a scene-graph lookup each interact.
    /// </summary>
    public class CarrySlot : MonoBehaviour
    {
        /// <summary>The player's slot (one per scene), or null.</summary>
        public static CarrySlot Instance { get; private set; }

        /// <summary>Id of the carried item, or null when empty.</summary>
        public string CurrentItemId { get; private set; }

        /// <summary>True while nothing is carried.</summary>
        public bool IsEmpty => CurrentItemId == null;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Picks up <paramref name="itemId"/>. Returns false (and changes
        /// nothing) when the slot is already occupied or the id is empty.
        /// </summary>
        public bool TryPickup(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || CurrentItemId != null)
            {
                return false;
            }
            CurrentItemId = itemId;
            return true;
        }

        /// <summary>
        /// Hands over <paramref name="itemId"/>: clears the slot and returns
        /// true only when the carried item matches.
        /// </summary>
        public bool TryDeliver(string itemId)
        {
            if (CurrentItemId == null || CurrentItemId != itemId)
            {
                return false;
            }
            CurrentItemId = null;
            return true;
        }
    }
}
