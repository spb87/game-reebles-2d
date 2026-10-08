using UnityEngine;
using UnityEngine.InputSystem;

namespace Reebles2D.Player
{
    /// <summary>
    /// Swaps the player's SpriteRenderer sprite based on the dominant input
    /// axis: up shows the back view, down the front, left/right the matching
    /// side view (flipX reuses the side sprite when one is missing). Idle
    /// keeps the last facing.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerFacing : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private Sprite frontSprite;
        [SerializeField] private Sprite backSprite;
        [SerializeField] private Sprite leftSprite;
        [SerializeField] private Sprite rightSprite;
        [SerializeField] private float deadZone = 0.1f;

        private SpriteRenderer spriteRenderer;
        private InputAction moveAction;

        /// <summary>Dominant facing direction; persists while idle.</summary>
        public Vector2 Facing { get; private set; } = Vector2.down;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (inputActions != null)
            {
                moveAction = inputActions.FindActionMap("Player")?.FindAction("Move");
            }
        }

        private void OnEnable()
        {
            inputActions?.FindActionMap("Player").Enable();
        }

        private void Update()
        {
            if (moveAction == null)
            {
                return;
            }
            SetFacing(moveAction.ReadValue<Vector2>());
        }

        /// <summary>Applies a facing direction; ignores input inside the dead zone.</summary>
        public void SetFacing(Vector2 input)
        {
            if (input.sqrMagnitude < deadZone * deadZone)
            {
                return;
            }
            Facing = Mathf.Abs(input.x) > Mathf.Abs(input.y)
                ? new Vector2(Mathf.Sign(input.x), 0f)
                : new Vector2(0f, Mathf.Sign(input.y));
            ApplySprite();
        }

        private void ApplySprite()
        {
            if (Facing.y > 0f)
            {
                SetSprite(backSprite, flipX: false);
            }
            else if (Facing.y < 0f)
            {
                SetSprite(frontSprite, flipX: false);
            }
            else if (Facing.x < 0f)
            {
                SetSprite(leftSprite != null ? leftSprite : rightSprite,
                    flipX: leftSprite == null);
            }
            else
            {
                SetSprite(rightSprite != null ? rightSprite : leftSprite,
                    flipX: rightSprite == null);
            }
        }

        private void SetSprite(Sprite sprite, bool flipX)
        {
            if (sprite != null)
            {
                spriteRenderer.sprite = sprite;
            }
            spriteRenderer.flipX = flipX;
        }
    }
}
