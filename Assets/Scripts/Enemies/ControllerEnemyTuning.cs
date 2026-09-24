using UnityEngine;
using UnityEngine.Serialization;

namespace ReturnVector.Enemies
{
    [CreateAssetMenu(
        fileName = "SO_ControllerTuning",
        menuName = "RETURN VECTOR/Enemies/Controller Tuning")]
    /// <summary>
    /// Movement and ranged-attack values for the Controller.
    /// </summary>
    public sealed class ControllerEnemyTuning : ScriptableObject
    {
        [Min(0f)] public float PreferredDistance = 7f;
        [Min(0f)] public float MoveSpeed = 2.5f;
        [Min(0f)] public float FireInterval = 2.2f;
        [Min(0f)] public float ExposedFireInterval = 1.35f;
        [Min(0f)] public float TelegraphSeconds = 0.58f;
        [Min(0f)] public float RecoverySeconds = 0.36f;
        [Min(0f)] public float ProjectileSpeed = 11f;
        [Min(0f)] public float ProjectileLifetime = 2.8f;
        [Min(0f)] public float HitRadius = 0.42f;
        [FormerlySerializedAs("DeflectionDegrees")]
        [Min(0f)] public float ProjectileDamage = 1.15f;

        public void ResetDefaults()
        {
            PreferredDistance = 7f;
            MoveSpeed = 2.5f;
            FireInterval = 2.2f;
            ExposedFireInterval = 1.35f;
            TelegraphSeconds = 0.58f;
            RecoverySeconds = 0.36f;
            ProjectileSpeed = 11f;
            ProjectileLifetime = 2.8f;
            HitRadius = 0.42f;
            ProjectileDamage = 1.15f;
        }
    }
}
