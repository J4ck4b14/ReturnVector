using System;
using ReturnVector.Input;
using ReturnVector.Weapon;
using UnityEngine;

// Script summary: Owns the player's outbound throw command and anticipation window. Aim remains live during anticipation; the direction is committed on release.

namespace ReturnVector.Player
{
    /// <summary>
    /// Owns the player's outbound throw command and anticipation window.
    /// Aim remains live during anticipation; the direction is committed on release.
    /// </summary>
    public sealed class PlayerThrowController : MonoBehaviour
    {
        // Weapon variables
        [SerializeField] private RVInputReader input;
        [SerializeField] private WorldAimProvider aim;
        [SerializeField] private WeaponController weapon;
        [SerializeField] private OutboundWeaponMotor outboundMotor;
        [SerializeField] private WeaponThrowTuning tuning;

        // Player variables
        private float anticipationRemaining;

        public bool IsAnticipating =>
            weapon != null &&
            weapon.State == WeaponState.ThrowAnticipation;

        public float AnticipationRemaining => anticipationRemaining;

        public event Action ThrowAnticipationStarted;
        public event Action<Vector3> ThrowReleased;

        /// <summary>
        /// Subscribes to runtime events when the component becomes active.
        /// </summary>
        private void OnEnable()
        {
            weapon?.ConfigureHeldCollision(tuning);

            if (input != null)
            {
                input.ThrowPressed += HandleThrowPressed;
            }
        }

        /// <summary>
        /// Unsubscribes from runtime events when the component is disabled.
        /// </summary>
        private void OnDisable()
        {
            if (input != null)
            {
                input.ThrowPressed -= HandleThrowPressed;
            }
        }

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(
            RVInputReader newInput,
            WorldAimProvider newAim,
            WeaponController newWeapon,
            OutboundWeaponMotor newOutboundMotor,
            WeaponThrowTuning newTuning)
        {
            if (isActiveAndEnabled && input != null)
            {
                input.ThrowPressed -= HandleThrowPressed;
            }

            input = newInput;
            aim = newAim;
            weapon = newWeapon;
            outboundMotor = newOutboundMotor;
            tuning = newTuning;
            weapon?.ConfigureHeldCollision(tuning);

            if (isActiveAndEnabled && input != null)
            {
                input.ThrowPressed += HandleThrowPressed;
            }
        }

        /// <summary>
        /// Advances the component for the current frame.
        /// </summary>
        private void Update()
        {
            if (!IsAnticipating || tuning == null)
            {
                return;
            }

            anticipationRemaining -= Time.deltaTime;
            if (anticipationRemaining <= 0f)
            {
                ReleaseThrow();
            }
        }

        /// <summary>
        /// Responds when throw input is pressed.
        /// </summary>
        private void HandleThrowPressed()
        {
            if (weapon == null ||
                tuning == null ||
                weapon.State != WeaponState.Held)
            {
                return;
            }

            if (!weapon.BeginThrowAnticipation())
            {
                return;
            }

            anticipationRemaining = tuning.AnticipationSeconds;
            ThrowAnticipationStarted?.Invoke();

            if (anticipationRemaining <= 0f)
            {
                ReleaseThrow();
            }
        }

        /// <summary>
        /// Commits the throw direction when anticipation ends.
        /// </summary>
        private void ReleaseThrow()
        {
            if (weapon == null ||
                outboundMotor == null ||
                aim == null ||
                weapon.State != WeaponState.ThrowAnticipation)
            {
                return;
            }

            if (!weapon.PrepareOutboundLaunch())
            {
                anticipationRemaining = 0f;
                weapon.ResetToHeld();
                return;
            }

            if (!aim.TryGetAim(
                    weapon.transform.position,
                    out _,
                    out Vector3 direction))
            {
                direction = aim.LastValidDirection;
            }

            if (!weapon.LaunchOutbound())
            {
                return;
            }

            if (!outboundMotor.Begin(direction))
            {
                weapon.ResetToHeld();
                return;
            }

            anticipationRemaining = 0f;
            ThrowReleased?.Invoke(direction);
        }
    }
}
