using System;
using UnityEngine;

namespace ReturnVector.Weapon
{
    /// <summary>
    /// Temporary recall lock that releases after sufficient player displacement or its fail-safe window.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WeaponRecallConstraint : MonoBehaviour
    {
        [SerializeField] private WeaponController weapon;

        private Transform owner;
        private Vector3 ownerPositionAtPin;
        private float requiredRepositionDistance;
        private float maxPinSeconds;
        private float elapsed;
        private bool pinned;

        public bool IsPinned => pinned;
        public bool CanRecall => !pinned;

        public float RequiredRepositionDistance =>
            requiredRepositionDistance;

        public float RepositionDistance
        {
            get
            {
                if (!pinned || owner == null)
                {
                    return 0f;
                }

                Vector3 a = ownerPositionAtPin;
                Vector3 b = owner.position;
                a.y = 0f;
                b.y = 0f;

                return Vector3.Distance(a, b);
            }
        }

        public float RepositionProgress =>
            requiredRepositionDistance <= 0f
                ? 1f
                : Mathf.Clamp01(
                    RepositionDistance /
                    requiredRepositionDistance);

        public event Action Pinned;
        public event Action Released;

        private void Awake()
        {
            ResolveOwner();
        }

        private void Update()
        {
            if (!pinned)
            {
                return;
            }

            elapsed += Time.deltaTime;

            if (WeaponRecallConstraintMath.ShouldRelease(
                    RepositionDistance,
                    requiredRepositionDistance,
                    elapsed,
                    maxPinSeconds))
            {
                Release();
            }
        }

        public void Configure(
            WeaponController newWeapon)
        {
            weapon = newWeapon;
            ResolveOwner();
        }

        public void Pin(
            float repositionDistance,
            float failSafeSeconds)
        {
            ResolveOwner();

            requiredRepositionDistance =
                Mathf.Max(0f, repositionDistance);

            maxPinSeconds =
                Mathf.Max(0.25f, failSafeSeconds);

            ownerPositionAtPin =
                owner != null
                    ? owner.position
                    : Vector3.zero;

            elapsed = 0f;

            if (pinned)
            {
                return;
            }

            pinned = true;
            Pinned?.Invoke();
        }

        public void Release()
        {
            if (!pinned)
            {
                return;
            }

            pinned = false;
            elapsed = 0f;
            Released?.Invoke();
        }

        private void ResolveOwner()
        {
            owner =
                weapon != null
                    ? weapon.Owner
                    : null;
        }
    }
}
