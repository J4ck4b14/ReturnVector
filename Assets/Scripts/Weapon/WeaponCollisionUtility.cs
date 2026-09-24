using System;
using System.Collections.Generic;
using ReturnVector.Combat;
using UnityEngine;

namespace ReturnVector.Weapon
{
    /// <summary>
    /// Shared collision filtering and component lookup for both weapon travel phases.
    /// </summary>
    public static class WeaponCollisionUtility
    {
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
            public static readonly RaycastHitDistanceComparer Instance =
                new RaycastHitDistanceComparer();

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
