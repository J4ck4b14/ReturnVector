using NUnit.Framework;
using ReturnVector.Combat;
using ReturnVector.Surfaces;
using UnityEngine;

// Script summary: Edit Mode coverage for WeaponSurfaceMath.

namespace ReturnVector.Tests
{
    /// <summary>
    /// Edit Mode coverage for WeaponSurfaceMath.
    /// </summary>
    public sealed class WeaponSurfaceMathTests
    {
        /// <summary>
        /// Verifies that reflection uses planar collision normal.
        /// </summary>
        [Test]
        public void Reflection_UsesPlanarCollisionNormal()
        {
            Vector3 result = WeaponSurfaceMath.ReflectPlanar(
                new Vector3(1f, 0f, 1f).normalized,
                Vector3.back);

            Assert.That(result.x, Is.GreaterThan(0.69f));
            Assert.That(result.z, Is.LessThan(-0.69f));
            Assert.That(result.y, Is.EqualTo(0f).Within(0.0001f));
        }

        /// <summary>
        /// Verifies that steer planar does not overshoot desired direction.
        /// </summary>
        [Test]
        public void SteerPlanar_DoesNotOvershootDesiredDirection()
        {
            Vector3 result = WeaponSurfaceMath.SteerPlanar(
                Vector3.forward,
                Vector3.right,
                90f,
                0.5f);

            float angle = Vector3.Angle(Vector3.forward, result);
            Assert.That(angle, Is.EqualTo(45f).Within(0.05f));
        }

        /// <summary>
        /// Verifies that penetrable profile resolves as non blocking.
        /// </summary>
        [Test]
        public void PenetrableProfile_ResolvesAsNonBlocking()
        {
            GameObject go = new GameObject("PenetrableTest");
            BoxCollider collider = go.AddComponent<BoxCollider>();
            WeaponSurface surface = go.AddComponent<WeaponSurface>();
            WeaponSurfaceProfile profile =
                ScriptableObject.CreateInstance<WeaponSurfaceProfile>();

            profile.ResetDefaults(
                WeaponSurfaceKind.Penetrable);
            surface.Configure(profile);

            bool resolved = WeaponSurfaceResolver.TryResolve(
                collider,
                Vector3.forward,
                Vector3.back,
                AttackPhase.Outbound,
                out WeaponSurfaceResponse response);

            Assert.IsTrue(resolved);
            Assert.IsFalse(response.Blocks);
            Assert.AreEqual(
                WeaponSurfaceKind.Penetrable,
                response.Kind);
            Assert.That(
                response.SpeedRetention,
                Is.LessThan(1f));

            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(go);
        }

        /// <summary>
        /// Verifies that absorbing profile resolves as blocking.
        /// </summary>
        [Test]
        public void AbsorbingProfile_ResolvesAsBlocking()
        {
            GameObject go = new GameObject("AbsorbTest");
            BoxCollider collider = go.AddComponent<BoxCollider>();
            WeaponSurface surface = go.AddComponent<WeaponSurface>();
            WeaponSurfaceProfile profile =
                ScriptableObject.CreateInstance<WeaponSurfaceProfile>();

            profile.ResetDefaults(
                WeaponSurfaceKind.Absorbing);
            surface.Configure(profile);

            bool resolved = WeaponSurfaceResolver.TryResolve(
                collider,
                Vector3.forward,
                Vector3.back,
                AttackPhase.Recall,
                out WeaponSurfaceResponse response);

            Assert.IsTrue(resolved);
            Assert.IsTrue(response.Blocks);
            Assert.AreEqual(
                WeaponSurfaceKind.Absorbing,
                response.Kind);

            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(go);
        }
    }
}
