// Dialogue session state machine. NpcInteractable opens it; each Update it
// advances on the Interact action or a pointer press and closes after the last
// line. While open it disables the Move action on the shared input asset —
// the Player assembly must not reference UI, so movement suppression happens
// here at the input layer instead of inside PlayerMovement.

using UnityEngine;
using UnityEngine.InputSystem;

namespace Reebles2D.UI
{
    /// <summary>
    /// Drives a single <see cref="DialogueCard"/>: opens a named multi-line
    /// dialogue, advances one line per Interact press or pointer click, closes
    /// at the end. Singleton via <see cref="Instance"/>; <see cref="IsOpen"/>
    /// is the global "player is in dialogue" flag.
    /// </summary>
    public class DialogueController : MonoBehaviour
    {
        [SerializeField] private DialogueCard card;
        [SerializeField] private InputActionAsset inputActions;

        private InputAction interactAction;
        private InputAction moveAction;
        private string speaker;
        private string[] lines;
        private int lineIndex;
        private int openedFrame = -1;

        /// <summary>The live controller instance (one per scene), or null.</summary>
        public static DialogueController Instance { get; private set; }

        /// <summary>Global flag — true while any dialogue card is open.</summary>
        public static bool IsOpen => Instance != null && Instance.IsDialogueOpen;

        /// <summary>
        /// Frame on which the last dialogue closed (or -1). NpcInteractable
        /// uses this to ignore a same-frame re-open — the Interact press that
        /// closes the final line must not immediately restart the dialogue,
        /// and script execution order between Interactor and this controller
        /// is undefined.
        /// </summary>
        public static int LastClosedFrame { get; private set; } = -1;

        /// <summary>True while this controller has a dialogue open.</summary>
        public bool IsDialogueOpen { get; private set; }

        private void Awake()
        {
            Instance = this;
            if (inputActions != null)
            {
                InputActionMap map = inputActions.FindActionMap("Player");
                interactAction = map.FindAction("Interact");
                moveAction = map.FindAction("Move");
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Opens the card for <paramref name="speaker"/> and shows the first of
        /// <paramref name="dialogueLines"/>. Ignored while a dialogue is already
        /// open or when the line list is empty.
        /// </summary>
        public void OpenDialogue(string speakerName, string[] dialogueLines)
        {
            if (IsDialogueOpen || dialogueLines == null || dialogueLines.Length == 0)
            {
                return;
            }
            speaker = speakerName;
            lines = dialogueLines;
            lineIndex = 0;
            openedFrame = Time.frameCount;
            IsDialogueOpen = true;
            card.Show(speaker, lines[0]);
            moveAction?.Disable();
        }

        /// <summary>Closes the card and releases movement.</summary>
        public void Close()
        {
            IsDialogueOpen = false;
            LastClosedFrame = Time.frameCount;
            lines = null;
            card.Hide();
            moveAction?.Enable();
        }

        private void Update()
        {
            if (!IsDialogueOpen)
            {
                return;
            }
            // The same Interact press that opened this dialogue must not also
            // advance it — skip the open frame regardless of script order.
            if (Time.frameCount == openedFrame)
            {
                return;
            }
            if (AdvancePressedThisFrame())
            {
                Advance();
            }
        }

        private bool AdvancePressedThisFrame()
        {
            if (interactAction != null && interactAction.WasPerformedThisFrame())
            {
                return true;
            }
            Pointer pointer = Pointer.current;
            return pointer != null && pointer.press.wasPressedThisFrame;
        }

        private void Advance()
        {
            lineIndex++;
            if (lineIndex >= lines.Length)
            {
                Close();
            }
            else
            {
                card.SetLine(lines[lineIndex]);
            }
        }
    }
}
