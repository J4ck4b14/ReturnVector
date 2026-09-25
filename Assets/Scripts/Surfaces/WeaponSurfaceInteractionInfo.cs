using ReturnVector.Combat;
using UnityEngine;

// Script summary: Snapshot of one resolved surface interaction for feedback and diagnostics.

namespace ReturnVector.Surfaces
{
    /// <summary>
    /// Snapshot of one resolved surface interaction for feedback and diagnostics.
    /// </summary>
    public readonly struct WeaponSurfaceInteractionInfo
    {
        // Surface variables
        public readonly WeaponSurfaceKind Kind;
        public readonly AttackPhase Phase;
        public readonly Vector3 Point;
        public readonly Vector3 Normal;
        public readonly Vector3 IncomingDirection;
        public readonly Vector3 OutgoingDirection;
        public readonly Collider Collider;

        /// <summary>
        /// Creates a new WeaponSurfaceInteractionInfo with the supplied values.
        /// </summary>
        public WeaponSurfaceInteractionInfo(
            WeaponSurfaceKind kind,
            AttackPhase phase,
            Vector3 point,
            Vector3 normal,
            Vector3 incomingDirection,
            Vector3 outgoingDirection,
            Collider collider)
        {
            Kind = kind;
            Phase = phase;
            Point = point;
            Normal = normal;
            IncomingDirection = incomingDirection;
            OutgoingDirection = outgoingDirection;
            Collider = collider;
        }
    }
}
