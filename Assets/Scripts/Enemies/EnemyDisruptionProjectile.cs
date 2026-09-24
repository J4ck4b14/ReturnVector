using ReturnVector.Weapon;
using UnityEngine;

namespace ReturnVector.Enemies
{
    /// <summary>
    /// Travels on a fixed line and deflects an outbound or returning weapon on contact.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyDisruptionProjectile : MonoBehaviour
    {
        private WeaponController weapon;
        private WeaponInterferenceController interference;
        private Vector3 direction;
        private float speed;
        private float hitRadius;
        private float remainingLifetime;
        private float deflectionDegrees;
        private Vector3 sourcePosition;
        private bool initialized;

        public void Configure(
            WeaponController targetWeapon,
            WeaponInterferenceController targetInterference,
            Vector3 initialDirection,
            Vector3 origin,
            float projectileSpeed,
            float radius,
            float lifetime,
            float deflection)
        {
            weapon = targetWeapon;
            interference = targetInterference;
            direction =
                initialDirection.sqrMagnitude > 0.0001f
                    ? initialDirection.normalized
                    : Vector3.forward;
            sourcePosition = origin;
            speed = Mathf.Max(0f, projectileSpeed);
            hitRadius = Mathf.Max(0.01f, radius);
            remainingLifetime = Mathf.Max(0.05f, lifetime);
            deflectionDegrees = Mathf.Max(0f, deflection);
            initialized = true;

            transform.rotation =
                Quaternion.LookRotation(
                    direction,
                    Vector3.up);
        }

        private void Update()
        {
            if (!initialized)
            {
                Destroy(gameObject);
                return;
            }

            float dt = Time.deltaTime;
            remainingLifetime -= dt;

            if (remainingLifetime <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            if (weapon == null ||
                interference == null ||
                weapon.State == WeaponState.Held ||
                weapon.State == WeaponState.ThrowAnticipation ||
                weapon.State == WeaponState.Catching ||
                weapon.State == WeaponState.Disabled)
            {
                Destroy(gameObject);
                return;
            }

            float travel =
                speed * Mathf.Max(0f, dt);

            Vector3 segmentStart =
                transform.position;

            Vector3 segmentEnd =
                segmentStart +
                direction * travel;

            Vector3 weaponPosition =
                weapon.transform.position;

            float distanceToSegment =
                EnemyProjectileMath.DistancePointToSegment(
                    weaponPosition,
                    segmentStart,
                    segmentEnd);

            if (distanceToSegment <= hitRadius)
            {
                interference.DeflectAwayFrom(
                    sourcePosition,
                    deflectionDegrees);
                Destroy(gameObject);
                return;
            }

            transform.position = segmentEnd;
        }
    }
}
