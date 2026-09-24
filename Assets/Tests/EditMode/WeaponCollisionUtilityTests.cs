using NUnit.Framework;
using ReturnVector.Weapon;
using UnityEngine;

namespace ReturnVector.Tests
{
    /// <summary>
    /// Covers contact placement used by swept weapon collision.
    /// </summary>
    public sealed class WeaponCollisionUtilityTests
    {
        [Test]
        public void SurfaceRestPosition_LeavesSphereOutsideSurface()
        {
            Vector3 result =
                WeaponCollisionUtility.SurfaceRestPosition(
                    Vector3.zero,
                    Vector3.right,
                    0.12f,
                    0.005f,
                    Vector3.one);

            Assert.AreEqual(
                0.13f,
                result.x,
                0.0001f);
            Assert.AreEqual(0f, result.y, 0.0001f);
            Assert.AreEqual(0f, result.z, 0.0001f);
        }

        [Test]
        public void SurfaceRestPosition_UsesFallbackForInvalidNormal()
        {
            Vector3 fallback =
                new Vector3(2f, 3f, 4f);

            Vector3 result =
                WeaponCollisionUtility.SurfaceRestPosition(
                    Vector3.zero,
                    Vector3.zero,
                    0.12f,
                    0.005f,
                    fallback);

            Assert.AreEqual(fallback, result);
        }
    }
}
