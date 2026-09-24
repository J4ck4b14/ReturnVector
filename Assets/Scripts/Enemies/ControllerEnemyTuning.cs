using UnityEngine;

namespace ReturnVector.Enemies
{
    [CreateAssetMenu(
        fileName = "SO_ControllerTuning",
        menuName = "RETURN VECTOR/Enemies/Controller Tuning")]
    /// <summary>
    /// Tuning values for Controller movement, telegraph and interference fire.
    /// </summary>
    public sealed class ControllerEnemyTuning : ScriptableObject
    {
        [Min(0f)] public float PreferredDistance = 7f;
        [Min(0f)] public float MoveSpeed = 2.5f;
        [Min(0f)] public float FireInterval = 2.2f;
        [Min(0f)] public float ExposedFireInterval = 1.35f;
        [Min(0f)] public float TelegraphSeconds = 0.45f;
        [Min(0f)] public float ProjectileSpeed = 10f;
        [Min(0f)] public float ProjectileLifetime = 2.5f;
        [Min(0f)] public float HitRadius = 0.42f;
        [Min(0f)] public float DeflectionDegrees = 34f;

        public void ResetDefaults()
        {
            PreferredDistance = 7f;
            MoveSpeed = 2.5f;
            FireInterval = 2.2f;
            ExposedFireInterval = 1.35f;
            TelegraphSeconds = 0.45f;
            ProjectileSpeed = 10f;
            ProjectileLifetime = 2.5f;
            HitRadius = 0.42f;
            DeflectionDegrees = 34f;
        }
    }
}
