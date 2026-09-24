namespace ReturnVector.Weapon
{
    /// <summary>
    /// Pure release test for distance-based recall constraints.
    /// </summary>
    public static class WeaponRecallConstraintMath
    {
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
