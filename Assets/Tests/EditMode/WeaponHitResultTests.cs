using NUnit.Framework;
using ReturnVector.Combat;

namespace ReturnVector.Tests
{
    /// <summary>
    /// Edit Mode coverage for WeaponHitResult.
    /// </summary>
    public sealed class WeaponHitResultTests
    {
        [Test]
        public void DamageAndPierce_DamagesWithoutBlocking()
        {
            WeaponHitResult result =
                WeaponHitResult.DamageAndPierce;

            Assert.IsTrue(result.DamagedTarget);
            Assert.IsFalse(result.BlocksWeapon);
            Assert.IsFalse(result.DeflectsWeapon);
        }

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
