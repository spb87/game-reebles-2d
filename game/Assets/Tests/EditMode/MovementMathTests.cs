using NUnit.Framework;
using Reebles2D.Player;
using UnityEngine;

namespace Reebles2D.Tests.EditMode
{
    /// <summary>
    /// Pure-math tests for <see cref="MovementMath.ComputeVelocity"/>:
    /// happy path, run multiplier, diagonal normalization, deadzone.
    /// </summary>
    public class MovementMathTests
    {
        private const float WalkSpeed = 3f;

        [Test]
        public void ComputeVelocity_RightInput_ReturnsPlusXAtWalkSpeed()
        {
            Vector2 v = MovementMath.ComputeVelocity(Vector2.right, WalkSpeed, 1f);

            Assert.That(v.x, Is.EqualTo(WalkSpeed).Within(0.0001f));
            Assert.That(v.y, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void ComputeVelocity_RunMultiplier_ScalesSpeed()
        {
            Vector2 v = MovementMath.ComputeVelocity(Vector2.up, WalkSpeed, 1.8f);

            Assert.That(v.magnitude, Is.EqualTo(WalkSpeed * 1.8f).Within(0.0001f));
            Assert.That(v.y, Is.GreaterThan(0f));
        }

        [Test]
        public void ComputeVelocity_DiagonalInput_IsNormalizedToWalkSpeed()
        {
            Vector2 v = MovementMath.ComputeVelocity(new Vector2(1f, 1f), WalkSpeed, 1f);

            Assert.That(v.magnitude, Is.EqualTo(WalkSpeed).Within(0.0001f));
            Assert.That(v.magnitude, Is.LessThan(WalkSpeed * 1.5f));
        }

        [Test]
        public void ComputeVelocity_BelowDeadzone_ReturnsZero()
        {
            Vector2 v = MovementMath.ComputeVelocity(new Vector2(0.05f, -0.05f), WalkSpeed, 1f);

            Assert.That(v, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void ComputeVelocity_PartialMagnitudeInput_PreservesReducedSpeed()
        {
            Vector2 v = MovementMath.ComputeVelocity(new Vector2(0.5f, 0f), WalkSpeed, 1f);

            Assert.That(v.magnitude, Is.EqualTo(WalkSpeed * 0.5f).Within(0.0001f));
        }
    }
}
