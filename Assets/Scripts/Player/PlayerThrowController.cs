using System;
using ReturnVector.Input;
using ReturnVector.Weapon;
using UnityEngine;

namespace ReturnVector.Player
{
    /// <summary>
    /// Owns the player's outbound throw command and anticipation window.
    /// Aim remains live during anticipation; the direction is committed on release.
    /// </summary>
    public sealed class PlayerThrowController : MonoBehaviour
    {
        [SerializeField] private RVInputReader input;
        [SerializeField] private WorldAimProvider aim;
        [SerializeField] private WeaponController weapon;
        [SerializeField] private OutboundWeaponMotor outboundMotor;
        [SerializeField] private WeaponThrowTuning tuning;

        private float anticipationRemaining;

        public bool IsAnticipating =>
            weapon != null &&
            weapon.State == WeaponState.ThrowAnticipation;

        public float AnticipationRemaining => anticipationRemaining;

        public event Action ThrowAnticipationStarted;
        public event Action<Vector3> ThrowReleased;

        private void OnEnable()
        {
            weapon?.ConfigureHeldCollision(tuning);

            if (input != null)
            {
                input.ThrowPressed += HandleThrowPressed;
            }
        }

        private void OnDisable()
        {
            if (input != null)
            {
                input.ThrowPressed -= HandleThrowPressed;
            }
        }

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
