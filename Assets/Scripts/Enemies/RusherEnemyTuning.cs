using UnityEngine;

namespace ReturnVector.Enemies
{
    [CreateAssetMenu(
        fileName = "SO_RusherTuning",
        menuName = "RETURN VECTOR/Enemies/Rusher Tuning")]
    /// <summary>
    /// Movement and attack values for the Rusher.
    /// </summary>
    public sealed class RusherEnemyTuning : ScriptableObject
    {
        [Min(0f)] public float ArmedMoveSpeed = 3.8f;
        [Min(0f)] public float ExposedMoveSpeed = 6.2f;
        [Min(0f)] public float AttackRange = 1.25f;
        [Min(0f)] public float ArmedWindup = 0.48f;
        [Min(0f)] public float ExposedWindup = 0.22f;
        [Min(0f)] public float RecoverySeconds = 0.70f;
        [Min(0f)] public float AttackDamage = 1.5f;

        public void ResetDefaults()
        {
            ArmedMoveSpeed = 3.8f;
            ExposedMoveSpeed = 6.2f;
            AttackRange = 1.25f;
            ArmedWindup = 0.48f;
            ExposedWindup = 0.22f;
            RecoverySeconds = 0.70f;
            AttackDamage = 1.5f;
        }
    }
}
