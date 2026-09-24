using NUnit.Framework;
using ReturnVector.GameFeel;

namespace ReturnVector.Tests
{
    /// <summary>
    /// Edit Mode coverage for RVGameFeelMath.
    /// </summary>
    public sealed class RVGameFeelMathTests
    {
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
