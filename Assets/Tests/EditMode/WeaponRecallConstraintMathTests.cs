using NUnit.Framework;
using ReturnVector.Weapon;

namespace ReturnVector.Tests
{
    /// <summary>
    /// Edit Mode coverage for WeaponRecallConstraintMath.
    /// </summary>
    public sealed class WeaponRecallConstraintMathTests
    {
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
