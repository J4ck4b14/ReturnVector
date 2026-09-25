using NUnit.Framework;
using ReturnVector.Combat;
using ReturnVector.Surfaces;
using ReturnVector.Weapon;
using UnityEngine;

// Script summary: Contains Edit Mode coverage for Weapon Collision Utility.

namespace ReturnVector.Tests.EditMode
{
    public sealed class WeaponCollisionUtilityTests
    {
        /// <summary>
        /// Verifies that stop distance uses minimum contact skin.
        /// </summary>
        [Test]
        public void StopDistance_UsesMinimumContactSkin()
        {
            float result =
                WeaponCollisionUtility.StopDistance(
                    1f,
                    0.001f);

            Assert.AreEqual(
                0.985f,
                result,
                0.0001f);
        }

        /// <summary>
        /// Verifies that stop distance never moves past origin.
        /// </summary>
        [Test]
        public void StopDistance_NeverMovesPastOrigin()
        {
            float result =
                WeaponCollisionUtility.StopDistance(
                    0.005f,
                    0.02f);

            Assert.AreEqual(
                0f,
                result,
                0.0001f);
        }

        /// <summary>
        /// Verifies that reachable position stops before solid geometry.
        /// </summary>
        [Test]
        public void ReachablePosition_StopsBeforeSolidGeometry()
        {
            GameObject wall = new GameObject("Wall");
            BoxCollider collider = wall.AddComponent<BoxCollider>();
            wall.transform.position = new Vector3(1f, 0f, 0f);
            collider.size = new Vector3(0.2f, 2f, 2f);
            Physics.SyncTransforms();

            RaycastHit[] hits = new RaycastHit[8];
            Collider[] overlaps = new Collider[8];

            bool resolved =
                WeaponCollisionUtility.TryResolveReachableWorldPosition(
                    Vector3.zero,
                    new Vector3(2f, 0f, 0f),
                    0.1f,
                    0.015f,
                    ~0,
                    AttackPhase.Outbound,
                    hits,
                    overlaps,
                    null,
                    null,
                    out Vector3 position,
                    out bool blocked);

            Assert.IsTrue(resolved);
            Assert.IsTrue(blocked);
            Assert.Less(position.x, 0.8f);

            Object.DestroyImmediate(wall);
        }

        /// <summary>
        /// Verifies that reachable position allows penetrable surface.
        /// </summary>
        [Test]
        public void ReachablePosition_AllowsPenetrableSurface()
        {
            GameObject wall = new GameObject("PenetrableWall");
            BoxCollider collider = wall.AddComponent<BoxCollider>();
            wall.transform.position = new Vector3(1f, 0f, 0f);
            collider.size = new Vector3(0.2f, 2f, 2f);

            WeaponSurfaceProfile profile =
                ScriptableObject.CreateInstance<WeaponSurfaceProfile>();
            profile.ResetDefaults(WeaponSurfaceKind.Penetrable);

            WeaponSurface surface = wall.AddComponent<WeaponSurface>();
            surface.Configure(profile);
            Physics.SyncTransforms();

            RaycastHit[] hits = new RaycastHit[8];
            Collider[] overlaps = new Collider[8];

            bool resolved =
                WeaponCollisionUtility.TryResolveReachableWorldPosition(
                    Vector3.zero,
                    new Vector3(2f, 0f, 0f),
                    0.1f,
                    0.015f,
                    ~0,
                    AttackPhase.Outbound,
                    hits,
                    overlaps,
                    null,
                    null,
                    out Vector3 position,
                    out bool blocked);

            Assert.IsTrue(resolved);
            Assert.IsFalse(blocked);
            Assert.AreEqual(2f, position.x, 0.001f);

            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(wall);
        }
    }
}
