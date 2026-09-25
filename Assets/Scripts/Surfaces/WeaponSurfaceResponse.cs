using UnityEngine;

// Script summary: Resolved response values consumed by the weapon motors.

namespace ReturnVector.Surfaces
{
    /// <summary>
    /// Resolved response values consumed by the weapon motors.
    /// </summary>
    public readonly struct WeaponSurfaceResponse
    {
        // Surface variables
        public readonly WeaponSurfaceKind Kind;
        public readonly bool Blocks;
        public readonly Vector3 OutgoingDirection;
        public readonly float SpeedRetention;
        public readonly float DistanceCost;

        /// <summary>
        /// Creates a new WeaponSurfaceResponse with the supplied values.
        /// </summary>
        public WeaponSurfaceResponse(
            WeaponSurfaceKind kind,
            bool blocks,
            Vector3 outgoingDirection,
            float speedRetention,
            float distanceCost)
        {
            Kind = kind;
            Blocks = blocks;
            OutgoingDirection = outgoingDirection;
            SpeedRetention = Mathf.Clamp01(speedRetention);
            DistanceCost = Mathf.Max(0f, distanceCost);
        }
    }
}
