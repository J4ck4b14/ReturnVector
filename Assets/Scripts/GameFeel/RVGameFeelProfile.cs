using UnityEngine;

// Script summary: Shared tuning profile for hit-stop, camera and impact feedback.

namespace ReturnVector.GameFeel
{
    [CreateAssetMenu(
        fileName = "SO_GameFeel",
        menuName = "RETURN VECTOR/Game Feel Profile")]
    /// <summary>
    /// Shared tuning profile for hit-stop, camera and impact feedback.
    /// </summary>
    public sealed class RVGameFeelProfile : ScriptableObject
    {
        // Hit-stop variables
        [Header("Hit-stop")]
        [Min(0f)] public float OutboundHitStop = 0.025f;
        [Min(0f)] public float RecallHitStop = 0.045f;
        [Min(0f)] public float HeavyBlockHitStop = 0.035f;
        [Min(0f)] public float CatchHitStop = 0.022f;
        [Range(0.01f, 1f)] public float HitStopTimeScale = 0.08f;

        // Camera impulses variables
        [Header("Camera impulses")]
        [Min(0f)] public float ThrowImpulse = 0.08f;
        [Min(0f)] public float OutboundImpactImpulse = 0.13f;
        [Min(0f)] public float RecallImpactImpulse = 0.23f;
        [Min(0f)] public float CatchImpulse = 0.16f;
        [Min(0f)] public float PlayerDamageImpulse = 0.28f;
        [Min(0.01f)] public float ImpactImpulseSeconds = 0.12f;
        [Min(0.01f)] public float CatchImpulseSeconds = 0.16f;

        // Camera framing variables
        [Header("Camera framing")]
        [Min(0f)] public float RecallZoomIn = 0.45f;
        [Min(0f)] public float ImpactZoomPunch = 0.18f;
        [Min(0f)] public float CatchZoomPunch = 0.26f;
        [Min(0f)] public float ZoomSharpness = 16f;
        [Min(0f)] public float RecallBlendSharpness = 9f;

        // Visual reactions variables
        [Header("Visual reactions")]
        [Min(0f)] public float EnemyFlashSeconds = 0.07f;
        [Min(0f)] public float WeaponFlashSeconds = 0.055f;
        [Min(0f)] public float PlayerDamageFlashSeconds = 0.10f;
        [Min(0f)] public float PoseSharpness = 18f;

        /// <summary>
        /// Restores the authored default tuning values.
        /// </summary>
        public void ResetDefaults()
        {
            OutboundHitStop = 0.025f;
            RecallHitStop = 0.045f;
            HeavyBlockHitStop = 0.035f;
            CatchHitStop = 0.022f;
            HitStopTimeScale = 0.08f;
            ThrowImpulse = 0.08f;
            OutboundImpactImpulse = 0.13f;
            RecallImpactImpulse = 0.23f;
            CatchImpulse = 0.16f;
            PlayerDamageImpulse = 0.28f;
            ImpactImpulseSeconds = 0.12f;
            CatchImpulseSeconds = 0.16f;
            RecallZoomIn = 0.45f;
            ImpactZoomPunch = 0.18f;
            CatchZoomPunch = 0.26f;
            ZoomSharpness = 16f;
            RecallBlendSharpness = 9f;
            EnemyFlashSeconds = 0.07f;
            WeaponFlashSeconds = 0.055f;
            PlayerDamageFlashSeconds = 0.10f;
            PoseSharpness = 18f;
        }
    }
}
