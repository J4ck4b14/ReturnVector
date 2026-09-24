using UnityEngine;

namespace ReturnVector.Weapon
{
    [CreateAssetMenu(
        fileName = "SO_RecallTuning",
        menuName = "RETURN VECTOR/Recall Tuning")]
    /// <summary>
    /// Simulation, steering, damage and catch values for recall travel.
    /// </summary>
    public sealed class WeaponRecallTuning : ScriptableObject
    {
        [Header("Travel")]
        [Min(0.01f)] public float InitialSpeed = 16f;
        [Min(0.01f)] public float MaxSpeed = 38f;
        [Min(0f)] public float Acceleration = 58f;
        [Min(1f)] public float TurnDegreesPerSecond = 820f;
        [Min(1f)] public float NearCatchTurnMultiplier = 1.75f;
        [Min(0.01f)] public float NearCatchDistance = 3f;

        [Header("Collision")]
        [Min(0.001f)] public float CollisionRadius = 0.12f;
        [Min(0f)] public float SurfaceBackoff = 0.015f;
        public LayerMask CollisionMask = ~0;
        [Min(0f)] public float EnemyImpactPauseSeconds = 0.018f;

        [Header("Combat")]
        [Min(0f)] public float RecallDamage = 1.25f;

        [Header("Catch")]
        [Min(0.01f)] public float CatchRadius = 0.45f;
        [Min(0f)] public float CatchSeconds = 0.055f;

        [Header("Simulation")]
        [Range(30, 240)] public int SimulationHz = 120;
        [Range(1, 24)] public int MaxSimulationStepsPerFrame = 12;
        [Min(0.01f)] public float MaxAccumulatedTime = 0.1f;

        [Header("Visual")]
        [Min(0f)] public float RecallSpinDegreesPerSecond = 1500f;

        [Header("Debug Path")]
        [Range(4, 96)] public int DebugPathSegments = 40;
        [Min(0.005f)] public float DebugPathStepSeconds = 0.035f;
        [Min(0.001f)] public float DebugPathWidth = 0.025f;

        public float SimulationStep =>
            SimulationHz > 0 ? 1f / SimulationHz : 1f / 120f;

        public void ResetDefaults()
        {
            InitialSpeed = 16f;
            MaxSpeed = 38f;
            Acceleration = 58f;
            TurnDegreesPerSecond = 820f;
            NearCatchTurnMultiplier = 1.75f;
            NearCatchDistance = 3f;
            CollisionRadius = 0.12f;
            SurfaceBackoff = 0.015f;
            CollisionMask = ~0;
            EnemyImpactPauseSeconds = 0.018f;
            RecallDamage = 1.25f;
            CatchRadius = 0.45f;
            CatchSeconds = 0.055f;
            SimulationHz = 120;
            MaxSimulationStepsPerFrame = 12;
            MaxAccumulatedTime = 0.1f;
            RecallSpinDegreesPerSecond = 1500f;
            DebugPathSegments = 40;
            DebugPathStepSeconds = 0.035f;
            DebugPathWidth = 0.025f;
        }
    }
}
