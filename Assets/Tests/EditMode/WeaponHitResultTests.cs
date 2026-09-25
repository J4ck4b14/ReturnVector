using NUnit.Framework;
using ReturnVector.Combat;

// Script summary: Edit Mode coverage for WeaponHitResult.

namespace ReturnVector.Tests
{
    /// <summary>
    /// Edit Mode coverage for WeaponHitResult.
    /// </summary>
    public sealed class WeaponHitResultTests
    {
        /// <summary>
        /// Verifies that damage and pierce damages without blocking.
        /// </summary>
        [Test]
        public void DamageAndPierce_DamagesWithoutBlocking()
        {
            WeaponHitResult result =
                WeaponHitResult.DamageAndPierce;

            Assert.IsTrue(result.DamagedTarget);
            Assert.IsFalse(result.BlocksWeapon);
            Assert.IsFalse(result.DeflectsWeapon);
        }

        /// <summary>
        /// Verifies that deflect stores requested angle.
        /// </summary>
        [Test]
        public void Deflect_StoresRequestedAngle()
        {
            WeaponHitResult result =
                WeaponHitResult.Deflect(27f);

            Assert.IsFalse(result.DamagedTarget);
            Assert.IsFalse(result.BlocksWeapon);
            Assert.IsTrue(result.DeflectsWeapon);
            Assert.AreEqual(27f, result.DeflectionDegrees, 0.001f);
        }
    }
}
