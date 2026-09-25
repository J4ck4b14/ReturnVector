using NUnit.Framework;
using ReturnVector.Weapon;

// Script summary: Edit Mode coverage for OutboundTravelMath.

namespace ReturnVector.Tests
{
    /// <summary>
    /// Edit Mode coverage for OutboundTravelMath.
    /// </summary>
    public sealed class OutboundTravelMathTests
    {
        /// <summary>
        /// Verifies that step distance uses speed times delta.
        /// </summary>
        [Test]
        public void StepDistance_UsesSpeedTimesDelta()
        {
            float distance = OutboundTravelMath.StepDistance(
                20f,
                0.05f,
                10f);

            Assert.AreEqual(1f, distance, 0.0001f);
        }

        /// <summary>
        /// Verifies that step distance does not overshoot remAIning distance.
        /// </summary>
        [Test]
        public void StepDistance_DoesNotOvershootRemainingDistance()
        {
            float distance = OutboundTravelMath.StepDistance(
                100f,
                1f,
                0.35f);

            Assert.AreEqual(0.35f, distance, 0.0001f);
        }

        /// <summary>
        /// Verifies that step distance invalid or empty input returns zero.
        /// </summary>
        [TestCase(0f, 0.1f, 10f)]
        [TestCase(20f, 0f, 10f)]
        [TestCase(20f, 0.1f, 0f)]
        public void StepDistance_InvalidOrEmptyInput_ReturnsZero(
            float speed,
            float deltaTime,
            float remaining)
        {
            Assert.AreEqual(
                0f,
                OutboundTravelMath.StepDistance(
                    speed,
                    deltaTime,
                    remaining));
        }
    }
}
