
// Script summary: Describes how a target changes weapon travel when resolving a hit.

namespace ReturnVector.Combat
{
    /// <summary>
    /// Describes how a target changes weapon travel when resolving a hit.
    /// </summary>
    public readonly struct WeaponHitResult
    {
        // Combat variables
        public readonly bool DamagedTarget;
        public readonly bool BlocksWeapon;
        public readonly bool DeflectsWeapon;
        public readonly float DeflectionDegrees;

        /// <summary>
        /// Creates a new WeaponHitResult with the supplied values.
        /// </summary>
        public WeaponHitResult(
            bool damagedTarget,
            bool blocksWeapon,
            bool deflectsWeapon = false,
            float deflectionDegrees = 0f)
        {
            DamagedTarget = damagedTarget;
            BlocksWeapon = blocksWeapon;
            DeflectsWeapon = deflectsWeapon;
            DeflectionDegrees = deflectionDegrees;
        }

        public static WeaponHitResult DamageAndPierce =>
            new WeaponHitResult(true, false);

        public static WeaponHitResult Block =>
            new WeaponHitResult(false, true);

        public static WeaponHitResult DamageAndBlock =>
            new WeaponHitResult(true, true);

        /// <summary>
        /// Creates a weapon-hit result that redirects travel by the requested angle.
        /// </summary>
        public static WeaponHitResult Deflect(float degrees) =>
            new WeaponHitResult(false, false, true, degrees);
    }
}
