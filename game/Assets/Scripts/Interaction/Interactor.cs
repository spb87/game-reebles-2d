using UnityEngine;
using UnityEngine.InputSystem;

namespace Reebles2D.Interaction
{
    /// <summary>
    /// Player-side interaction brain: each Update it scans a circle around the
    /// player for colliders carrying an <see cref="Interactable"/>, selects the
    /// nearest one that is inside its own <see cref="Interactable.InteractRadius"/>,
    /// floats the shared prompt bubble above it, and calls
    /// <see cref="Interactable.Interact"/> when the Interact action fires.
    /// </summary>
    public class Interactor : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private Transform prompt;
        [SerializeField] private float scanRadius = 3f;

        private InputAction interactAction;

        /// <summary>The nearest in-range interactable, or null.</summary>
        public Interactable CurrentTarget { get; private set; }

        private void Awake()
        {
            if (inputActions != null)
            {
                interactAction = inputActions
                    .FindActionMap("Player").FindAction("Interact");
            }
        }

        private void OnEnable()
        {
            inputActions?.FindActionMap("Player").Enable();
        }

        private void OnDisable()
        {
            inputActions?.FindActionMap("Player").Disable();
        }

        private void Update()
        {
            CurrentTarget = FindNearestTarget();
            UpdatePrompt();

            if (CurrentTarget != null
                && interactAction != null
                && interactAction.WasPerformedThisFrame())
            {
                CurrentTarget.Interact();
            }
        }

        private Interactable FindNearestTarget()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, scanRadius);
            Interactable nearest = null;
            float nearestDistance = float.MaxValue;
            foreach (Collider2D hit in hits)
            {
                Interactable candidate = hit.GetComponentInParent<Interactable>();
                if (candidate == null)
                {
                    continue;
                }
                float distance = Vector2.Distance(
                    transform.position, candidate.transform.position);
                if (distance <= candidate.InteractRadius && distance < nearestDistance)
                {
                    nearest = candidate;
                    nearestDistance = distance;
                }
            }
            return nearest;
        }

        private void UpdatePrompt()
        {
            if (prompt == null)
            {
                return;
            }
            bool show = CurrentTarget != null;
            if (prompt.gameObject.activeSelf != show)
            {
                prompt.gameObject.SetActive(show);
            }
            if (show)
            {
                prompt.position = CurrentTarget.transform.position
                    + new Vector3(0f, CurrentTarget.PromptHeight, 0f);
            }
        }
    }
}
