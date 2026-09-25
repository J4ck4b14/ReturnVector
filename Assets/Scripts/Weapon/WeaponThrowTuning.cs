using UnityEngine;

// Script summary: Simulation, collision and damage values for outbound travel.

namespace ReturnVector.Weapon
{
    [CreateAssetMenu(
        fileName = "SO_ThrowTuning",
        menuName = "RETURN VECTOR/Throw Tuning")]
    /// <summary>
    /// Simulation, collision and damage values for outbound travel.
    /// </summary>
    public sealed class WeaponThrowTuning : ScriptableObject
    {
        // Timing variables
        [Header("Timing")]
        [Min(0f)] public float AnticipationSeconds = 0.08f;
        [Min(0f)] public float EnemyImpactPauseSeconds = 0.025f;

        // Outbound Travel variables
        [Header("Outbound Travel")]
        [Min(0.01f)] public float OutboundSpeed = 26f;
        [Min(0.01f)] public float MaxDistance = 18f;
        [Min(0.001f)] public float CollisionRadius = 0.12f;
        [Min(0f)] public float SurfaceBackoff = 0.015f;
        public LayerMask CollisionMask = ~0;

        // Simulation variables
        [Header("Simulation")]
        [Range(30, 240)] public int SimulationHz = 120;
        [Range(1, 24)] public int MaxSimulationStepsPerFrame = 12;
        [Min(0.01f)] public float MaxAccumulatedTime = 0.1f;

        // Combat variables
        [Header("Combat")]
        [Min(0f)] public float OutboundDamage = 1f;

        // Preview variables
        [Header("Preview")]
        [Min(0.001f)] public float PreviewWidth = 0.035f;
        public bool HidePreviewDuringOutbound = true;

        // Visual variables
        [Header("Visual")]
        public float SpinDegreesPerSecond = 1080f;

        public float SimulationStep =>
            SimulationHz > 0 ? 1f / SimulationHz : 1f / 120f;

        /// <summary>
        /// Restores the authored default tuning values.
        /// </summary>
        public void ResetDefaults()
        {
            AnticipationSeconds = 0.08f;
            EnemyImpactPauseSeconds = 0.025f;
            OutboundSpeed = 26f;
            MaxDistance = 18f;
            CollisionRadius = 0.12f;
            SurfaceBackoff = 0.015f;
            CollisionMask = ~0;
            SimulationHz = 120;
            MaxSimulationStepsPerFrame = 12;
            MaxAccumulatedTime = 0.1f;
            OutboundDamage = 1f;
            PreviewWidth = 0.035f;
            HidePreviewDuringOutbound = true;
            SpinDegreesPerSecond = 1080f;
        }
    }
}
