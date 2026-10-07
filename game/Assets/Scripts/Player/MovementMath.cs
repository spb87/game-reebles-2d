using UnityEngine;

namespace Reebles2D.Player
{
    /// <summary>
    /// Pure movement math for the player. EditMode-testable; no scene dependency.
    /// </summary>
    public static class MovementMath
    {
        private const float Deadzone = 0.125f;

        /// <summary>
        /// Converts raw input into a world-space velocity. Normalizes diagonal
        /// input, applies run multiplier, and returns zero for sub-deadzone input.
        /// </summary>
        public static Vector2 ComputeVelocity(Vector2 input, float walkSpeed, float runMultiplier)
        {
            if (input.sqrMagnitude < Deadzone * Deadzone)
            {
                return Vector2.zero;
            }

            Vector2 direction = input.sqrMagnitude > 1f ? input.normalized : input;
            return direction * walkSpeed * runMultiplier;
        }
    }
}
