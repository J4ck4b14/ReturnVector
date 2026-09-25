using UnityEngine;

// Script summary: Common impact payload published by outbound and recall motors.

namespace ReturnVector.Weapon
{
    /// <summary>
    /// Common impact payload published by outbound and recall motors.
    /// </summary>
    public readonly struct WeaponImpactInfo
    {
        // Weapon variables
        public readonly Vector3 Point;
        public readonly Vector3 Normal;
        public readonly Collider Collider;
        public readonly bool Blocking;
        public readonly bool DamagedTarget;

        /// <summary>
        /// Creates a new WeaponImpactInfo with the supplied values.
        /// </summary>
        public WeaponImpactInfo(
            Vector3 point,
            Vector3 normal,
            Collider collider,
            bool blocking,
            bool damagedTarget)
        {
            Point = point;
            Normal = normal;
            Collider = collider;
            Blocking = blocking;
            DamagedTarget = damagedTarget;
        }
    }
}
