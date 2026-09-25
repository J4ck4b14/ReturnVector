
// Script summary: Pure release test for distance-based recall constraints.

namespace ReturnVector.Weapon
{
    /// <summary>
    /// Pure release test for distance-based recall constraints.
    /// </summary>
    public static class WeaponRecallConstraintMath
    {
        /// <summary>
        /// Checks whether the release should occur.
        /// </summary>
        public static bool ShouldRelease(
            float repositionDistance,
            float requiredDistance,
            float elapsedSeconds,
            float failSafeSeconds)
        {
            return
                repositionDistance >= requiredDistance ||
                elapsedSeconds >= failSafeSeconds;
        }
    }
}
