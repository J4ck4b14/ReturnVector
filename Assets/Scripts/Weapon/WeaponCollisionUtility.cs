using System;
using System.Collections.Generic;
using ReturnVector.Combat;
using ReturnVector.Surfaces;
using UnityEngine;

// Script summary: Shared collision filtering and component lookup for both weapon travel phases.

namespace ReturnVector.Weapon
{
    /// <summary>
    /// Shared collision filtering and component lookup for both weapon travel phases.
    /// </summary>
    public static class WeaponCollisionUtility
    {
        /// <summary>
        /// Checks whether a collider belongs to the weapon owner hierarchy.
        /// </summary>
        public static bool IsOwnedCollider(
            Collider collider,
            Transform weaponRoot,
            Transform ownerRoot)
        {
            if (collider == null)
            {
                return true;
            }

            Transform hitTransform = collider.transform;

            if (weaponRoot != null &&
                (hitTransform == weaponRoot || hitTransform.IsChildOf(weaponRoot)))
            {
                return true;
            }

            if (ownerRoot != null &&
                (hitTransform == ownerRoot || hitTransform.IsChildOf(ownerRoot)))
            {
                return true;
            }

            return false;
        }



        /// <summary>
        /// Returns the stop distance.
        /// </summary>
        public static float StopDistance(
            float hitDistance,
            float configuredBackoff)
        {
            float skin = Mathf.Max(0.015f, configuredBackoff);
            return Mathf.Max(0f, hitDistance - skin);
        }

        /// <summary>
        /// Checks whether solid geometry blocks the path between two weapon positions.
        /// </summary>
        public static bool HasBlockingPath(
            Vector3 origin,
            Vector3 target,
            float radius,
            LayerMask collisionMask,
            AttackPhase phase,
            RaycastHit[] hitBuffer,
            Transform weaponRoot,
            Transform ownerRoot)
        {
            if (hitBuffer == null || hitBuffer.Length == 0)
            {
                return false;
            }

            Vector3 delta = target - origin;
            float distance = delta.magnitude;

            if (distance <= 0.0001f)
            {
                return false;
            }

            Vector3 direction = delta / distance;
            int hitCount = Physics.SphereCastNonAlloc(
                origin,
                radius,
                direction,
                hitBuffer,
                distance,
                collisionMask,
                QueryTriggerInteraction.Ignore);

            SortHitsByDistance(hitBuffer, hitCount);

            for (int i = 0; i < hitCount; i++)
            {
                Collider collider = hitBuffer[i].collider;

                if (IsOwnedCollider(
                        collider,
                        weaponRoot,
                        ownerRoot))
                {
                    continue;
                }

                if (TryGetWeaponHitReceiver(
                        collider,
                        out _) ||
                    TryGetDamageable(
                        collider,
                        out _))
                {
                    return true;
                }

                if (WeaponSurfaceResolver.TryResolve(
                        collider,
                        direction,
                        hitBuffer[i].normal,
                        phase,
                        out WeaponSurfaceResponse response))
                {
                    if (!response.Blocks &&
                        response.Kind !=
                        WeaponSurfaceKind.Reflective)
                    {
                        continue;
                    }
                }

                return true;
            }

            return false;
        }


        /// <summary>
        /// Finds the closest world position reachable from the player side without crossing solid
        /// geometry.
        /// </summary>
        public static bool TryResolveReachableWorldPosition(
            Vector3 referencePosition,
            Vector3 desiredPosition,
            float radius,
            float configuredBackoff,
            LayerMask collisionMask,
            AttackPhase phase,
            RaycastHit[] hitBuffer,
            Collider[] overlapBuffer,
            Transform weaponRoot,
            Transform ownerRoot,
            out Vector3 resolvedPosition,
            out bool blockedByGeometry)
        {
            resolvedPosition = referencePosition;
            blockedByGeometry = false;

            if (hitBuffer == null ||
                hitBuffer.Length == 0 ||
                overlapBuffer == null ||
                overlapBuffer.Length == 0 ||
                radius <= 0f)
            {
                return false;
            }

            if (HasBlockingOverlap(
                    referencePosition,
                    radius,
                    collisionMask,
                    phase,
                    overlapBuffer,
                    weaponRoot,
                    ownerRoot))
            {
                return false;
            }

            Vector3 delta = desiredPosition - referencePosition;
            float distance = delta.magnitude;

            if (distance <= 0.0001f)
            {
                resolvedPosition = referencePosition;
                return true;
            }

            Vector3 direction = delta / distance;
            int hitCount = Physics.SphereCastNonAlloc(
                referencePosition,
                radius,
                direction,
                hitBuffer,
                distance,
                collisionMask,
                QueryTriggerInteraction.Ignore);

            SortHitsByDistance(hitBuffer, hitCount);

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = hitBuffer[i];
                Collider collider = hit.collider;

                if (IsOwnedCollider(
                        collider,
                        weaponRoot,
                        ownerRoot) ||
                    TryGetWeaponHitReceiver(
                        collider,
                        out _) ||
                    TryGetDamageable(
                        collider,
                        out _))
                {
                    continue;
                }

                if (IsPassThroughSurface(
                        collider,
                        phase))
                {
                    continue;
                }

                blockedByGeometry = true;
                float travel = StopDistance(
                    hit.distance,
                    configuredBackoff);

                Vector3 candidate =
                    referencePosition +
                    direction * travel;

                if (HasBlockingOverlap(
                        candidate,
                        radius,
                        collisionMask,
                        phase,
                        overlapBuffer,
                        weaponRoot,
                        ownerRoot))
                {
                    candidate = FindFarthestClearPoint(
                        referencePosition,
                        candidate,
                        radius,
                        collisionMask,
                        phase,
                        overlapBuffer,
                        weaponRoot,
                        ownerRoot);
                }

                resolvedPosition = candidate;
                return true;
            }

            if (!HasBlockingOverlap(
                    desiredPosition,
                    radius,
                    collisionMask,
                    phase,
                    overlapBuffer,
                    weaponRoot,
                    ownerRoot))
            {
                resolvedPosition = desiredPosition;
                return true;
            }

            blockedByGeometry = true;
            resolvedPosition = FindFarthestClearPoint(
                referencePosition,
                desiredPosition,
                radius,
                collisionMask,
                phase,
                overlapBuffer,
                weaponRoot,
                ownerRoot);
            return true;
        }

        /// <summary>
        /// Finds the farthest clear point.
        /// </summary>
        private static Vector3 FindFarthestClearPoint(
            Vector3 clearReference,
            Vector3 blockedTarget,
            float radius,
            LayerMask collisionMask,
            AttackPhase phase,
            Collider[] overlapBuffer,
            Transform weaponRoot,
            Transform ownerRoot)
        {
            float clearT = 0f;
            float blockedT = 1f;

            for (int i = 0; i < 12; i++)
            {
                float testT =
                    (clearT + blockedT) * 0.5f;

                Vector3 candidate =
                    Vector3.Lerp(
                        clearReference,
                        blockedTarget,
                        testT);

                if (HasBlockingOverlap(
                        candidate,
                        radius,
                        collisionMask,
                        phase,
                        overlapBuffer,
                        weaponRoot,
                        ownerRoot))
                {
                    blockedT = testT;
                }
                else
                {
                    clearT = testT;
                }
            }

            return Vector3.Lerp(
                clearReference,
                blockedTarget,
                clearT);
        }

        /// <summary>
        /// Checks whether the collider is an authored surface that permits this weapon phase to
        /// pass.
        /// </summary>
        private static bool IsPassThroughSurface(
            Collider collider,
            AttackPhase phase)
        {
            return
                WeaponSurfaceResolver.TryGetProfile(
                    collider,
                    out WeaponSurfaceProfile profile) &&
                profile.AppliesTo(phase) &&
                (profile.Kind == WeaponSurfaceKind.Penetrable ||
                 profile.Kind == WeaponSurfaceKind.Curving);
        }

        /// <summary>
        /// Checks whether the baton currently overlaps solid blocking geometry.
        /// </summary>
        public static bool HasBlockingOverlap(
            Vector3 position,
            float radius,
            LayerMask collisionMask,
            AttackPhase phase,
            Collider[] overlapBuffer,
            Transform weaponRoot,
            Transform ownerRoot)
        {
            int count = Physics.OverlapSphereNonAlloc(
                position,
                radius,
                overlapBuffer,
                collisionMask,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                Collider collider = overlapBuffer[i];

                if (IsOwnedCollider(
                        collider,
                        weaponRoot,
                        ownerRoot))
                {
                    continue;
                }

                if (TryGetWeaponHitReceiver(
                        collider,
                        out _) ||
                    TryGetDamageable(
                        collider,
                        out _))
                {
                    continue;
                }

                if (WeaponSurfaceResolver.TryGetProfile(
                        collider,
                        out WeaponSurfaceProfile profile) &&
                    profile.AppliesTo(phase) &&
                    (profile.Kind ==
                        WeaponSurfaceKind.Penetrable ||
                     profile.Kind ==
                        WeaponSurfaceKind.Curving))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to find a weapon-hit receiver on the supplied collider hierarchy.
        /// </summary>
        public static bool TryGetWeaponHitReceiver(
            Collider collider,
            out IWeaponHitReceiver receiver)
        {
            receiver = null;

            if (collider == null)
            {
                return false;
            }

            // Compound colliders resolve their combat receiver from the nearest parent chain.
            MonoBehaviour[] behaviours =
                collider.GetComponentsInParent<MonoBehaviour>(true);

            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IWeaponHitReceiver candidate)
                {
                    receiver = candidate;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to find a damageable target on the supplied collider hierarchy.
        /// </summary>
        public static bool TryGetDamageable(
            Collider collider,
            out IDamageable damageable)
        {
            damageable = null;

            if (collider == null)
            {
                return false;
            }

            // Damage lookup follows the same compound-collider ownership chain.
            MonoBehaviour[] behaviours =
                collider.GetComponentsInParent<MonoBehaviour>(true);

            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IDamageable candidate &&
                    candidate.CanReceiveDamage)
                {
                    damageable = candidate;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Sorts the hits by distance.
        /// </summary>
        public static void SortHitsByDistance(
            RaycastHit[] hits,
            int hitCount)
        {
            // SphereCastNonAlloc returns an unspecified contact order; nearest contact resolves first.
            Array.Sort(
                hits,
                0,
                hitCount,
                RaycastHitDistanceComparer.Instance);
        }

        private sealed class RaycastHitDistanceComparer :
            IComparer<RaycastHit>
        {
            // Weapon variables
            public static readonly RaycastHitDistanceComparer Instance =
                new RaycastHitDistanceComparer();

            /// <summary>
            /// Orders raycast hits by distance for deterministic collision processing.
            /// </summary>
            public int Compare(RaycastHit a, RaycastHit b)
            {
                int distanceOrder = a.distance.CompareTo(b.distance);
                if (distanceOrder != 0)
                {
                    return distanceOrder;
                }

                int aId = a.collider != null ? a.collider.GetInstanceID() : int.MaxValue;
                int bId = b.collider != null ? b.collider.GetInstanceID() : int.MaxValue;
                return aId.CompareTo(bId);
            }
        }
    }
}
