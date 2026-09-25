using NUnit.Framework;
using ReturnVector.Player;
using UnityEngine;

// Script summary: Edit Mode coverage for PlayerMovementMath.

namespace ReturnVector.Tests
{
    /// <summary>
    /// Edit Mode coverage for PlayerMovementMath.
    /// </summary>
    public sealed class PlayerMovementMathTests
    {
        /// <summary>
        /// Verifies that diagonal input is clamped to unit length.
        /// </summary>
        [Test]
        public void DiagonalInput_IsClampedToUnitLength()
        {
            Vector3 move = PlayerMovementMath.GetWorldMove(
                new Vector2(1f, 1f),
                0.05f);

            Assert.That(move.magnitude, Is.EqualTo(1f).Within(0.0001f));
        }

        /// <summary>
        /// Verifies that dodge prefers movement input.
        /// </summary>
        [Test]
        public void DodgePrefersMovementInput()
        {
            Vector3 direction = PlayerMovementMath.ChooseDodgeDirection(
                Vector2.right,
                0.05f,
                Vector3.forward,
                Vector3.back);

            Assert.That(direction.x, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(direction.z, Is.EqualTo(0f).Within(0.0001f));
        }

        /// <summary>
        /// Verifies that dodge falls back to AIm when stationary.
        /// </summary>
        [Test]
        public void DodgeFallsBackToAimWhenStationary()
        {
            Vector3 direction = PlayerMovementMath.ChooseDodgeDirection(
                Vector2.zero,
                0.05f,
                Vector3.left,
                Vector3.forward);

            Assert.That(direction.x, Is.EqualTo(-1f).Within(0.0001f));
        }

        /// <summary>
        /// Verifies that dodge distance delta never moves backward.
        /// </summary>
        [Test]
        public void DodgeDistanceDelta_NeverMovesBackward()
        {
            AnimationCurve curve =
                AnimationCurve.Linear(
                    0f,
                    0f,
                    1f,
                    1f);

            float delta = PlayerMovementMath.DodgeDistanceDelta(
                curve,
                0.25f,
                0.5f,
                4f);

            Assert.That(delta, Is.EqualTo(1f).Within(0.0001f));
        }

        /// <summary>
        /// Verifies that dodge distance delta clamps descending cuRVe motion to zero.
        /// </summary>
        [Test]
        public void DodgeDistanceDelta_ClampsDescendingCurveMotionToZero()
        {
            AnimationCurve curve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.5f, 1f),
                new Keyframe(1f, 0f));

            float delta = PlayerMovementMath.DodgeDistanceDelta(
                curve,
                0.5f,
                0.75f,
                4f);

            Assert.That(delta, Is.EqualTo(0f).Within(0.0001f));
        }

    }
}
