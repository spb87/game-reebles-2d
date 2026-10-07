using UnityEngine;
using UnityEngine.InputSystem;

namespace Reebles2D.Player
{
    /// <summary>
    /// Thin MonoBehaviour: reads the Player action map and drives a kinematic
    /// Rigidbody2D via MovePosition in FixedUpdate.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float runMultiplier = 1.8f;

        private Rigidbody2D body;
        private InputAction moveAction;
        private InputAction runAction;

        /// <summary>Current planar speed in units per second.</summary>
        public float CurrentSpeed { get; private set; }

        /// <summary>True while the Run action is held.</summary>
        public bool IsRunning { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;

            if (inputActions != null)
            {
                InputActionMap map = inputActions.FindActionMap("Player");
                moveAction = map.FindAction("Move");
                runAction = map.FindAction("Run");
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

        private void FixedUpdate()
        {
            if (moveAction == null)
            {
                CurrentSpeed = 0f;
                IsRunning = false;
                return;
            }

            Vector2 input = moveAction.ReadValue<Vector2>();
            IsRunning = runAction != null && runAction.IsPressed();

            float multiplier = IsRunning ? runMultiplier : 1f;
            Vector2 velocity = MovementMath.ComputeVelocity(input, walkSpeed, multiplier);
            CurrentSpeed = velocity.magnitude;

            body.MovePosition(body.position + velocity * Time.fixedDeltaTime);
        }
    }
}
