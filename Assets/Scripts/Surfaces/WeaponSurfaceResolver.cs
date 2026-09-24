using ReturnVector.Combat;
using UnityEngine;

namespace ReturnVector.Surfaces
{
    /// <summary>
    /// Resolves the effective surface response for a collider and attack phase.
    /// </summary>
    public static class WeaponSurfaceResolver
    {
        public static bool TryGetProfile(
            Collider collider,
            out WeaponSurfaceProfile profile)
        {
            profile = null;

            if (collider == null)
            {
                return false;
            }

            WeaponSurface surface =
                collider.GetComponentInParent<WeaponSurface>();

            if (surface == null || surface.Profile == null)
            {
                return false;
            }

            // The collider carries the authored profile; phase-specific values are resolved below.
            profile = surface.Profile;
            return true;
        }

        public static bool TryResolve(
            Collider collider,
            Vector3 incomingDirection,
            Vector3 surfaceNormal,
            AttackPhase phase,
            out WeaponSurfaceResponse response)
        {
            response = default;

            if (!TryGetProfile(collider, out WeaponSurfaceProfile profile) ||
                !profile.AppliesTo(phase))
            {
                return false;
            }

            switch (profile.Kind)
            {
                case WeaponSurfaceKind.Reflective:
                    response = new WeaponSurfaceResponse(
                        profile.Kind,
                        false,
                        WeaponSurfaceMath.ReflectPlanar(
                            incomingDirection,
                            surfaceNormal),
                        profile.SpeedRetention,
                        profile.DistanceCost);
                    return true;

                case WeaponSurfaceKind.Penetrable:
                case WeaponSurfaceKind.Curving:
                    response = new WeaponSurfaceResponse(
                        profile.Kind,
                        false,
                        incomingDirection.normalized,
                        profile.SpeedRetention,
                        profile.DistanceCost);
                    return true;

                case WeaponSurfaceKind.Absorbing:
                case WeaponSurfaceKind.Neutral:
                default:
                    response = new WeaponSurfaceResponse(
                        profile.Kind,
                        true,
                        Vector3.zero,
                        0f,
                        profile.DistanceCost);
                    return true;
            }
        }
    }
}
