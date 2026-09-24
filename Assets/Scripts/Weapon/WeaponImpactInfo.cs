using UnityEngine;

namespace ReturnVector.Weapon
{
    /// <summary>
    /// Common impact payload published by outbound and recall motors.
    /// </summary>
    public readonly struct WeaponImpactInfo
    {
        public readonly Vector3 Point;
        public readonly Vector3 Normal;
        public readonly Collider Collider;
        public readonly bool Blocking;
        public readonly bool DamagedTarget;

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
