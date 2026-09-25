using UnityEngine;
using UnityEngine.Serialization;

// Script summary: Movement, attack and phase values for the Return Warden.

namespace ReturnVector.Enemies
{
    [CreateAssetMenu(
        fileName = "SO_ReturnWardenTuning",
        menuName = "RETURN VECTOR/Enemies/Return Warden Tuning")]
    /// <summary>
    /// Movement, attack and phase values for the Return Warden.
    /// </summary>
    public sealed class ReturnWardenTuning : ScriptableObject
    {
        // Health and weapon relationship variables
        [Header("Health and weapon relationship")]
        [Min(1f)] public float MaxHealth = 12f;
        [Range(0f, 1f)] public float OutboundDamageMultiplier = 0.30f;
        [Min(1f)] public float RecallDamageMultiplier = 1.75f;
        [Min(0.1f)] public float PinRepositionDistance = 2.8f;
        [Min(0.25f)] public float PinFailSafeSeconds = 6f;

        // Warden phase two variables
        [Header("Phase two")]
        [Range(0.1f, 0.9f)] public float PhaseTwoHealthRatio = 0.50f;
        [Min(0.1f)] public float PhaseTwoPinDistanceMultiplier = 1.25f;
        [Min(0.1f)] public float PhaseTwoSpeedMultiplier = 2.15f;
        [Range(0.2f, 1f)] public float PhaseTwoRecoveryMultiplier = 0.72f;
        [Range(0.2f, 1f)] public float PhaseTwoCooldownMultiplier = 0.62f;
        [Min(0.1f)] public float PhaseTwoChargeSpeedMultiplier = 1.22f;
        [Min(0.1f)] public float PhaseTwoDamageMultiplier = 1.15f;
        [Min(0.25f)] public float PhaseTransitionDuration = 2.35f;

        // Warden phase three variables
        [Header("Phase three")]
        [Min(0.1f)] public float PhaseThreeSpeedMultiplier = 1.08f;
        [Range(0.2f, 1f)] public float PhaseThreeRecoveryMultiplier = 0.88f;
        [Range(0.2f, 1f)] public float PhaseThreeCooldownMultiplier = 0.82f;
        [Min(0.1f)] public float PhaseThreeChargeSpeedMultiplier = 1.05f;
        [Min(0.1f)] public float PhaseThreeDamageMultiplier = 1.20f;
        [Min(0.25f)] public float PhaseThreeTransitionDuration = 2.65f;

        // Warden movement variables
        [Header("Movement")]
        [Min(0f)] public float BaseMoveSpeed = 1.85f;
        [Min(0f)] public float PinnedPressureSpeed = 2.75f;
        [Min(0f)] public float PreferredDistance = 1.8f;

        // Warden slam variables
        [Header("Slam")]
        [Min(0f)] public float SlamRange = 2.15f;
        [Min(0f)] public float SlamDamage = 2.2f;
        [Min(0f)] public float SlamWindup = 0.72f;
        [FormerlySerializedAs("PhaseTwoWindup")]
        [Min(0f)] public float PhaseTwoSlamWindup = 0.38f;
        [Min(0f)] public float SlamRecovery = 0.82f;
        [Min(0f)] public float SlamCooldown = 1.45f;

        // Warden charge variables
        [Header("Charge")]
        [Min(0f)] public float ChargeTriggerRange = 7.5f;
        [Min(0f)] public float ChargeWindup = 0.72f;
        [Min(0f)] public float PhaseTwoChargeWindup = 0.42f;
        [Min(0f)] public float ChargeSpeed = 10.5f;
        [Min(0f)] public float ChargeDistance = 5.4f;
        [Min(0f)] public float ChargeHitRadius = 1.05f;
        [Min(0f)] public float ChargeDamage = 1.55f;
        [Min(0f)] public float ChargeRecovery = 0.72f;
        [Min(0f)] public float ChargeCooldown = 1.9f;

        // Warden shockwave variables
        [Header("Shockwave")]
        [Min(0f)] public float ShockwaveRange = 4.25f;
        [Min(0f)] public float ShockwaveDamage = 3.0f;
        [Min(0f)] public float ShockwavePushDistance = 2.6f;
        [Min(0f)] public float ShockwaveWindup = 0.82f;
        [Min(0f)] public float ShockwaveRecovery = 0.92f;
        [Min(0f)] public float ShockwaveCooldown = 2.3f;

        /// <summary>
        /// Restores the authored default tuning values.
        /// </summary>
        public void ResetDefaults()
        {
            MaxHealth = 12f;
            OutboundDamageMultiplier = 0.30f;
            RecallDamageMultiplier = 1.75f;
            PinRepositionDistance = 2.8f;
            PinFailSafeSeconds = 6f;

            PhaseTwoHealthRatio = 0.50f;
            PhaseTwoPinDistanceMultiplier = 1.25f;
            PhaseTwoSpeedMultiplier = 2.15f;
            PhaseTwoRecoveryMultiplier = 0.72f;
            PhaseTwoCooldownMultiplier = 0.62f;
            PhaseTwoChargeSpeedMultiplier = 1.22f;
            PhaseTwoDamageMultiplier = 1.15f;
            PhaseTransitionDuration = 2.35f;

            PhaseThreeSpeedMultiplier = 1.08f;
            PhaseThreeRecoveryMultiplier = 0.88f;
            PhaseThreeCooldownMultiplier = 0.82f;
            PhaseThreeChargeSpeedMultiplier = 1.05f;
            PhaseThreeDamageMultiplier = 1.20f;
            PhaseThreeTransitionDuration = 2.65f;

            BaseMoveSpeed = 1.85f;
            PinnedPressureSpeed = 2.75f;
            PreferredDistance = 1.8f;

            SlamRange = 2.15f;
            SlamDamage = 2.2f;
            SlamWindup = 0.72f;
            PhaseTwoSlamWindup = 0.38f;
            SlamRecovery = 0.82f;
            SlamCooldown = 1.45f;

            ChargeTriggerRange = 7.5f;
            ChargeWindup = 0.72f;
            PhaseTwoChargeWindup = 0.42f;
            ChargeSpeed = 10.5f;
            ChargeDistance = 5.4f;
            ChargeHitRadius = 1.05f;
            ChargeDamage = 1.55f;
            ChargeRecovery = 0.72f;
            ChargeCooldown = 1.9f;

            ShockwaveRange = 4.25f;
            ShockwaveDamage = 3.0f;
            ShockwavePushDistance = 2.6f;
            ShockwaveWindup = 0.82f;
            ShockwaveRecovery = 0.92f;
            ShockwaveCooldown = 2.3f;
        }
    }
}
