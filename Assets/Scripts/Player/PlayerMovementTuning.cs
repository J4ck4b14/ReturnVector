using UnityEngine;

namespace ReturnVector.Player
{
    [CreateAssetMenu(
        fileName = "SO_PlayerMovementTuning",
        menuName = "RETURN VECTOR/Player Movement Tuning")]
    /// <summary>
    /// Movement, turn and dodge values for armed and weaponless states.
    /// </summary>
    public sealed class PlayerMovementTuning : ScriptableObject
    {
        [Header("Locomotion")]
        [Min(0.01f)] public float ArmedMoveSpeed = 6f;
        [Min(0.01f)] public float UnarmedMoveSpeed = 7f;
        [Min(0f)] public float Acceleration = 42f;
        [Min(0f)] public float Deceleration = 58f;
        [Min(1f)] public float TurnDegreesPerSecond = 1080f;
        [Range(0f, 0.95f)] public float MoveInputDeadzone = 0.08f;

        [Header("Dodge")]
        [Min(0.01f)] public float ArmedDodgeDistance = 3.1f;
        [Min(0.01f)] public float UnarmedDodgeDistance = 3.45f;
        [Min(0.01f)] public float DodgeDuration = 0.23f;
        [Min(0f)] public float ArmedDodgeCooldown = 0.52f;
        [Min(0f)] public float UnarmedDodgeCooldown = 0.40f;
        [Min(0f)] public float DodgeInputBuffer = 0.12f;
        public AnimationCurve DodgeDistanceCurve = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 3.3f),
            new Keyframe(0.30f, 0.70f, 1.1f, 1.1f),
            new Keyframe(1f, 1f, 0.15f, 0f));

        public float MoveSpeed(PlayerCombatMode mode)
        {
            return mode == PlayerCombatMode.Unarmed
                ? UnarmedMoveSpeed
                : ArmedMoveSpeed;
        }

        public float DodgeDistance(PlayerCombatMode mode)
        {
            return mode == PlayerCombatMode.Unarmed
                ? UnarmedDodgeDistance
                : ArmedDodgeDistance;
        }

        public float DodgeCooldown(PlayerCombatMode mode)
        {
            return mode == PlayerCombatMode.Unarmed
                ? UnarmedDodgeCooldown
                : ArmedDodgeCooldown;
        }

        public void ResetDefaults()
        {
            ArmedMoveSpeed = 6f;
            UnarmedMoveSpeed = 7f;
            Acceleration = 42f;
            Deceleration = 58f;
            TurnDegreesPerSecond = 1080f;
            MoveInputDeadzone = 0.08f;
            ArmedDodgeDistance = 3.1f;
            UnarmedDodgeDistance = 3.45f;
            DodgeDuration = 0.23f;
            ArmedDodgeCooldown = 0.52f;
            UnarmedDodgeCooldown = 0.40f;
            DodgeInputBuffer = 0.12f;
            DodgeDistanceCurve = new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 3.3f),
                new Keyframe(0.30f, 0.70f, 1.1f, 1.1f),
                new Keyframe(1f, 1f, 0.15f, 0f));
        }
    }
}
