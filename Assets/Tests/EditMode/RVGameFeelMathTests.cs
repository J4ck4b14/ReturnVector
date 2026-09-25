using NUnit.Framework;
using ReturnVector.GameFeel;

// Script summary: Edit Mode coverage for RVGameFeelMath.

namespace ReturnVector.Tests
{
    /// <summary>
    /// Edit Mode coverage for RVGameFeelMath.
    /// </summary>
    public sealed class RVGameFeelMathTests
    {
        /// <summary>
        /// Verifies that impact envelope starts at one.
        /// </summary>
        [Test]
        public void ImpactEnvelope_StartsAtOne()
        {
            float envelope =
                RVGameFeelMath.ImpactEnvelope(
                    0.2f,
                    0.2f);

            Assert.AreEqual(
                1f,
                envelope,
                0.0001f);
        }

        /// <summary>
        /// Verifies that impact envelope decays quadratically.
        /// </summary>
        [Test]
        public void ImpactEnvelope_DecaysQuadratically()
        {
            float envelope =
                RVGameFeelMath.ImpactEnvelope(
                    0.1f,
                    0.2f);

            Assert.AreEqual(
                0.25f,
                envelope,
                0.0001f);
        }

        /// <summary>
        /// Verifies that exponential blend stays inside zero one.
        /// </summary>
        [Test]
        public void ExponentialBlend_StaysInsideZeroOne()
        {
            float blend =
                RVGameFeelMath.ExponentialBlend(
                    16f,
                    1f / 60f);

            Assert.That(
                blend,
                Is.GreaterThan(0f));

            Assert.That(
                blend,
                Is.LessThan(1f));
        }

        /// <summary>
        /// Verifies that zero delta produces no blend.
        /// </summary>
        [Test]
        public void ZeroDelta_ProducesNoBlend()
        {
            Assert.AreEqual(
                0f,
                RVGameFeelMath.ExponentialBlend(
                    16f,
                    0f),
                0.0001f);
        }
    }
}
