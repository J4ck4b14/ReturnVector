using UnityEngine;

// Script summary: Pure reflection and steering helpers for authored surface responses.

namespace ReturnVector.Surfaces
{
    /// <summary>
    /// Pure reflection and steering helpers for authored surface responses.
    /// </summary>
    public static class WeaponSurfaceMath
    {
        /// <summary>
        /// Reflects a planar direction using the supplied collision normal.
        /// </summary>
        public static Vector3 ReflectPlanar(
            Vector3 incomingDirection,
            Vector3 surfaceNormal)
        {
            Vector3 incoming = incomingDirection;
            incoming.y = 0f;

            Vector3 normal = surfaceNormal;
            normal.y = 0f;

            if (incoming.sqrMagnitude < 0.000001f)
            {
                return Vector3.forward;
            }

            if (normal.sqrMagnitude < 0.000001f)
            {
                return incoming.normalized;
            }

            Vector3 reflected = Vector3.Reflect(
                incoming.normalized,
                normal.normalized);
            reflected.y = 0f;

            return reflected.sqrMagnitude >= 0.000001f
                ? reflected.normalized
                : -incoming.normalized;
        }

        /// <summary>
        /// Steers a planar direction toward the requested direction by a bounded angle.
        /// </summary>
        public static Vector3 SteerPlanar(
            Vector3 currentDirection,
            Vector3 desiredDirection,
            float degreesPerSecond,
            float deltaTime)
        {
            Vector3 current = currentDirection;
            current.y = 0f;

            Vector3 desired = desiredDirection;
            desired.y = 0f;

            if (desired.sqrMagnitude < 0.000001f)
            {
                return current.sqrMagnitude >= 0.000001f
                    ? current.normalized
                    : Vector3.forward;
            }

            if (current.sqrMagnitude < 0.000001f)
            {
                return desired.normalized;
            }

            float maxRadians =
                Mathf.Max(0f, degreesPerSecond) *
                Mathf.Deg2Rad *
                Mathf.Max(0f, deltaTime);

            Vector3 steered = Vector3.RotateTowards(
                current.normalized,
                desired.normalized,
                maxRadians,
                0f);
            steered.y = 0f;

            return steered.sqrMagnitude >= 0.000001f
                ? steered.normalized
                : desired.normalized;
        }
    }
}
