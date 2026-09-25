using NUnit.Framework;
using ReturnVector.Weapon;
using UnityEngine;

// Script summary: Edit Mode coverage for RecallTravelMath.

namespace ReturnVector.Tests
{
    /// <summary>
    /// Edit Mode coverage for RecallTravelMath.
    /// </summary>
    public sealed class RecallTravelMathTests
    {
        /// <summary>
        /// Verifies that accelerate approaches max without overshoot.
        /// </summary>
        [Test]
        public void Accelerate_ApproachesMaxWithoutOvershoot()
        {
            float speed = RecallTravelMath.Accelerate(10f, 20f, 100f, 1f);
            Assert.AreEqual(20f, speed, 0.0001f);
        }

        /// <summary>
        /// Verifies that step distance does not overshoot target.
        /// </summary>
        [Test]
        public void StepDistance_DoesNotOvershootTarget()
        {
            float distance = RecallTravelMath.StepDistance(30f, 0.1f, 1.25f);
            Assert.AreEqual(1.25f, distance, 0.0001f);
        }

        /// <summary>
        /// Verifies that steer rotates toward desired direction.
        /// </summary>
        [Test]
        public void Steer_RotatesTowardDesiredDirection()
        {
            Vector3 result = RecallTravelMath.Steer(
                Vector3.forward,
                Vector3.right,
                90f,
                0.5f);

            Assert.Greater(result.x, 0f);
            Assert.Greater(result.z, 0f);
            Assert.AreEqual(1f, result.magnitude, 0.0001f);
        }

        /// <summary>
        /// Verifies that near catch turn rate increases close to target.
        /// </summary>
        [Test]
        public void NearCatchTurnRate_IncreasesCloseToTarget()
        {
            float far = RecallTravelMath.TurnRateForDistance(600f, 10f, 3f, 2f);
            float near = RecallTravelMath.TurnRateForDistance(600f, 0.5f, 3f, 2f);

            Assert.AreEqual(600f, far, 0.0001f);
            Assert.Greater(near, far);
        }

        /// <summary>
        /// Verifies that catch ease reaches one.
        /// </summary>
        [Test]
        public void CatchEase_ReachesOne()
        {
            Assert.AreEqual(1f, RecallTravelMath.CatchEase(1f), 0.0001f);
        }
    }
}
