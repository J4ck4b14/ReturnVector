using System;
using ReturnVector.Core;
using ReturnVector.Input;
using UnityEngine;

namespace ReturnVector.Player
{
    /// <summary>
    /// Top-down locomotion and evasive dodge movement.
    /// Weapon ownership selects the movement and dodge tuning used by the same input.
    /// </summary>
    public sealed class PlayerMov : MonoBehaviour
    {
        [SerializeField] private RVInputReader input;
        [SerializeField] private WorldAimProvider aim;
        [SerializeField] private PlayerCombatController combat;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private PlayerMovementTuning tuning;

        private PlayerMovementState state = PlayerMovementState.Locomotion;
        private Vector3 velocity;
        private Vector3 dodgeDirection = Vector3.forward;
        private float dodgeElapsed;
        private float dodgeDistance;
        private float dodgeCooldownRemaining;
        private float dodgeBufferRemaining;
        private Vector3 facingDirection = Vector3.forward;

        public PlayerMovementState State => state;
        public Vector3 Velocity => velocity;
        public Vector3 FacingDirection => facingDirection;
        public bool IsDodging => state == PlayerMovementState.Dodging;
        public float DodgeCooldownRemaining => dodgeCooldownRemaining;
        public float Speed => velocity.magnitude;
        public float SpeedNormalized
        {
            get
            {
                if (tuning == null || combat == null)
                {
                    return 0f;
                }

                float max = Mathf.Max(
                    0.01f,
                    tuning.MoveSpeed(combat.Mode) *
                    GameDifficulty.Current.PlayerMoveSpeedMultiplier);
                return Mathf.Clamp01(velocity.magnitude / max);
            }
        }

        public event Action<PlayerMovementState, PlayerMovementState> StateChanged;
        public event Action<Vector3> DodgeStarted;
        public event Action DodgeCompleted;

        private void Awake()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }
        }

        private void OnEnable()
        {
            if (input != null)
            {
                input.DodgePressed += HandleDodgePressed;
            }
        }

        private void OnDisable()
        {
            if (input != null)
            {
                input.DodgePressed -= HandleDodgePressed;
            }
        }

        public void Configure(
            RVInputReader newInput,
            WorldAimProvider newAim,
            PlayerCombatController newCombat,
            CharacterController newCharacterController,
            PlayerMovementTuning newTuning)
        {
            if (isActiveAndEnabled && input != null)
            {
                input.DodgePressed -= HandleDodgePressed;
            }

            input = newInput;
            aim = newAim;
            combat = newCombat;
            characterController = newCharacterController;
            tuning = newTuning;

            if (isActiveAndEnabled && input != null)
            {
                input.DodgePressed += HandleDodgePressed;
            }
        }

        public void SetDisabled(bool disabled)
        {
            SetState(disabled
                ? PlayerMovementState.Disabled
                : PlayerMovementState.Locomotion);

            if (disabled)
            {
                velocity = Vector3.zero;
                dodgeBufferRemaining = 0f;
            }
        }

        public void ApplyPush(Vector3 worldDirection, float distance)
        {
            if (state == PlayerMovementState.Disabled || distance <= 0f)
            {
                return;
            }

            Vector3 direction = worldDirection;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Move(direction.normalized * distance);
            velocity = Vector3.zero;
        }

        private void Update()
        {
            if (tuning == null || input == null)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            dodgeCooldownRemaining = Mathf.Max(
                0f,
                dodgeCooldownRemaining - deltaTime);
            dodgeBufferRemaining = Mathf.Max(
                0f,
                dodgeBufferRemaining - deltaTime);

            if (state == PlayerMovementState.Disabled)
            {
                return;
            }

            if (state == PlayerMovementState.Dodging)
            {
                TickDodge(deltaTime);
                return;
            }

            // Buffered dodge input survives the tail end of the cooldown for a more responsive transition.
            if (dodgeBufferRemaining > 0f &&
                dodgeCooldownRemaining <= 0f)
            {
                BeginDodge();
                if (state == PlayerMovementState.Dodging)
                {
                    TickDodge(deltaTime);
                    return;
                }
            }

            TickLocomotion(deltaTime);
        }

        private void TickLocomotion(float deltaTime)
        {
            PlayerCombatMode mode = combat != null
                ? combat.Mode
                : PlayerCombatMode.Armed;

            Vector3 move = PlayerMovementMath.GetWorldMove(
                input.Move,
                tuning.MoveInputDeadzone);

            // Armed state changes movement tuning; input direction itself remains unchanged.
            Vector3 desiredVelocity =
                move *
                tuning.MoveSpeed(mode) *
                GameDifficulty.Current.PlayerMoveSpeedMultiplier;

            velocity = PlayerMovementMath.StepVelocity(
                velocity,
                desiredVelocity,
                tuning.Acceleration,
                tuning.Deceleration,
                deltaTime);

            Move(velocity * deltaTime);
            UpdateFacing(deltaTime);
        }

        private void UpdateFacing(float deltaTime)
        {
            Vector3 desired = facingDirection;

            if (aim != null &&
                aim.TryGetAim(transform.position, out _, out Vector3 aimDirection))
            {
                desired = aimDirection;
            }
            else if (velocity.sqrMagnitude > 0.01f)
            {
                desired = velocity.normalized;
            }

            desired.y = 0f;
            if (desired.sqrMagnitude < 0.0001f)
            {
                return;
            }

            facingDirection = desired.normalized;
            Quaternion target = Quaternion.LookRotation(
                facingDirection,
                Vector3.up);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                target,
                tuning.TurnDegreesPerSecond * deltaTime);
        }

        private void HandleDodgePressed()
        {
            if (state == PlayerMovementState.Disabled || tuning == null)
            {
                return;
            }

            dodgeBufferRemaining = Mathf.Max(
                dodgeBufferRemaining,
                Mathf.Max(tuning.DodgeInputBuffer, 0.0001f));
        }

        private void BeginDodge()
        {
            if (state != PlayerMovementState.Locomotion ||
                dodgeCooldownRemaining > 0f)
            {
                return;
            }

            PlayerCombatMode mode = combat != null
                ? combat.Mode
                : PlayerCombatMode.Armed;

            Vector3 aimDirection = aim != null
                ? aim.LastValidDirection
                : facingDirection;

            dodgeDirection = PlayerMovementMath.ChooseDodgeDirection(
                input.Move,
                tuning.MoveInputDeadzone,
                aimDirection,
                facingDirection);

            dodgeElapsed = 0f;
            dodgeDistance =
                tuning.DodgeDistance(mode) *
                GameDifficulty.Current.DodgeDistanceMultiplier;
            dodgeCooldownRemaining =
                tuning.DodgeCooldown(mode) *
                GameDifficulty.Current.DodgeCooldownMultiplier;
            dodgeBufferRemaining = 0f;
            velocity = Vector3.zero;
            facingDirection = dodgeDirection;
            transform.rotation = Quaternion.LookRotation(
                facingDirection,
                Vector3.up);

            SetState(PlayerMovementState.Dodging);
            DodgeStarted?.Invoke(dodgeDirection);
        }

        private void TickDodge(float deltaTime)
        {
            float duration = Mathf.Max(0.01f, tuning.DodgeDuration);
            float previousNormalized = dodgeElapsed / duration;
            dodgeElapsed = Mathf.Min(duration, dodgeElapsed + deltaTime);
            float nextNormalized = dodgeElapsed / duration;

            // The curve is sampled as cumulative distance, so each frame applies only its slice.
            float distanceDelta = PlayerMovementMath.DodgeDistanceDelta(
                tuning.DodgeDistanceCurve,
                previousNormalized,
                nextNormalized,
                dodgeDistance);

            Move(dodgeDirection * distanceDelta);

            float effectiveDelta = Mathf.Max(deltaTime, 0.0001f);
            velocity = dodgeDirection * (distanceDelta / effectiveDelta);

            if (dodgeElapsed >= duration)
            {
                velocity = Vector3.zero;
                SetState(PlayerMovementState.Locomotion);
                DodgeCompleted?.Invoke();
            }
        }

        private void Move(Vector3 displacement)
        {
            displacement.y = 0f;

            if (characterController != null &&
                characterController.enabled)
            {
                characterController.Move(displacement);
            }
            else
            {
                transform.position += displacement;
            }
        }

        private void SetState(PlayerMovementState next)
        {
            if (state == next)
            {
                return;
            }

            PlayerMovementState previous = state;
            state = next;
            StateChanged?.Invoke(previous, next);
        }
    }
}
