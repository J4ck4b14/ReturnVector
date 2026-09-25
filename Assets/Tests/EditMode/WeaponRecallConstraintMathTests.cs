using NUnit.Framework;
using ReturnVector.Weapon;

// Script summary: Edit Mode coverage for WeaponRecallConstraintMath.

namespace ReturnVector.Tests
{
    /// <summary>
    /// Edit Mode coverage for WeaponRecallConstraintMath.
    /// </summary>
    public sealed class WeaponRecallConstraintMathTests
    {
        /// <summary>
        /// Verifies that pin remAIns while player has not moved far enough.
        /// </summary>
        [Test]
        public void Pin_RemainsWhilePlayerHasNotMovedFarEnough()
        {
            Assert.IsFalse(
                WeaponRecallConstraintMath.ShouldRelease(
                    1.4f,
                    2.8f,
                    1f,
                    6f));
        }

        /// <summary>
        /// Verifies that pin releases at required displacement.
        /// </summary>
        [Test]
        public void Pin_ReleasesAtRequiredDisplacement()
        {
            Assert.IsTrue(
                WeaponRecallConstraintMath.ShouldRelease(
                    2.8f,
                    2.8f,
                    1f,
                    6f));
        }

        /// <summary>
        /// Verifies that fAIl safe prevents permanent softlock.
        /// </summary>
        [Test]
        public void FailSafe_PreventsPermanentSoftlock()
        {
            Assert.IsTrue(
                WeaponRecallConstraintMath.ShouldRelease(
                    0f,
                    2.8f,
                    6f,
                    6f));
        }
    }
}
