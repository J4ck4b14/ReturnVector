using UnityEngine;

namespace ReturnVector.Enemies
{
    [CreateAssetMenu(
        fileName = "SO_ReturnWardenTuning",
        menuName = "RETURN VECTOR/Enemies/Return Warden Tuning")]
    /// <summary>
    /// Movement, attack and pinning values for the Return Warden.
    /// </summary>
    public sealed class ReturnWardenTuning : ScriptableObject
    {
        [Header("Health and weapon relationship")]
        [Min(1f)] public float MaxHealth = 12f;
        [Range(0f, 1f)] public float OutboundDamageMultiplier = 0.30f;
        [Min(1f)] public float RecallDamageMultiplier = 1.75f;
        [Min(0.1f)] public float PinRepositionDistance = 2.8f;
        [Min(0.25f)] public float PinFailSafeSeconds = 6f;

        [Header("Phase two")]
        [Range(0.1f, 0.9f)] public float PhaseTwoHealthRatio = 0.50f;
        [Min(0.1f)] public float PhaseTwoPinDistanceMultiplier = 1.25f;

        [Header("Movement")]
        [Min(0f)] public float BaseMoveSpeed = 2.6f;
        [Min(0f)] public float PinnedPressureSpeed = 4.1f;
        [Min(0f)] public float PhaseTwoSpeedMultiplier = 1.18f;
        [Min(0f)] public float PreferredDistance = 1.8f;

        [Header("Slam")]
        [Min(0f)] public float SlamRange = 2.15f;
        [Min(0f)] public float SlamDamage = 2f;
        [Min(0f)] public float SlamWindup = 0.62f;
        [Min(0f)] public float PhaseTwoWindup = 0.40f;
        [Min(0f)] public float SlamRecovery = 0.85f;
        [Min(0f)] public float SlamCooldown = 1.6f;

        public void ResetDefaults()
        {
            MaxHealth = 12f;
            OutboundDamageMultiplier = 0.30f;
            RecallDamageMultiplier = 1.75f;
            PinRepositionDistance = 2.8f;
            PinFailSafeSeconds = 6f;
            PhaseTwoHealthRatio = 0.50f;
            PhaseTwoPinDistanceMultiplier = 1.25f;
            BaseMoveSpeed = 2.6f;
            PinnedPressureSpeed = 4.1f;
            PhaseTwoSpeedMultiplier = 1.18f;
            PreferredDistance = 1.8f;
            SlamRange = 2.15f;
            SlamDamage = 2f;
            SlamWindup = 0.62f;
            PhaseTwoWindup = 0.40f;
            SlamRecovery = 0.85f;
            SlamCooldown = 1.6f;
        }
    }
}
