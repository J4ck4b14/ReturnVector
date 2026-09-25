using System;
using ReturnVector.Input;
using ReturnVector.Weapon;
using UnityEngine;

// Script summary: Converts recall input into the explicit Returning state and hands motion to RecallWeaponMotor. Recall may begin while the weapon is outbound, parked or embedded.

namespace ReturnVector.Player
{
    /// <summary>
    /// Converts recall input into the explicit Returning state and hands motion to RecallWeaponMotor.
    /// Recall may begin while the weapon is outbound, parked or embedded.
    /// </summary>
    public sealed class PlayerRecallController : MonoBehaviour
    {
        // Weapon variables
        [SerializeField] private RVInputReader input;
        [SerializeField] private WeaponController weapon;
        [SerializeField] private OutboundWeaponMotor outboundMotor;
        [SerializeField] private RecallWeaponMotor recallMotor;
        [SerializeField] private WeaponRecallConstraint recallConstraint;

        public event Action RecallRequested;
        public event Action RecallDenied;

        /// <summary>
        /// Subscribes to runtime events when the component becomes active.
        /// </summary>
        private void OnEnable()
        {
            if (input != null)
            {
                input.RecallPressed += HandleRecallPressed;
            }
        }

        /// <summary>
        /// Unsubscribes from runtime events when the component is disabled.
        /// </summary>
        private void OnDisable()
        {
            if (input != null)
            {
                input.RecallPressed -= HandleRecallPressed;
            }
        }

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(
            RVInputReader newInput,
            WeaponController newWeapon,
            OutboundWeaponMotor newOutboundMotor,
            RecallWeaponMotor newRecallMotor)
        {
            if (isActiveAndEnabled && input != null)
            {
                input.RecallPressed -= HandleRecallPressed;
            }

            input = newInput;
            weapon = newWeapon;
            outboundMotor = newOutboundMotor;
            recallMotor = newRecallMotor;
            recallConstraint =
                weapon != null
                    ? weapon.GetComponent<WeaponRecallConstraint>()
                    : null;

            if (isActiveAndEnabled && input != null)
            {
                input.RecallPressed += HandleRecallPressed;
            }
        }

        /// <summary>
        /// Resets the weapon to hand.
        /// </summary>
        public void ResetWeaponToHand()
        {
            if (weapon == null)
            {
                return;
            }

            outboundMotor?.Abort();
            recallMotor?.Abort();

            if (recallConstraint == null)
            {
                recallConstraint =
                    weapon.GetComponent<WeaponRecallConstraint>();
            }

            recallConstraint?.Release();
            weapon.ResetToHeld();
        }

        /// <summary>
        /// Responds when recall input is pressed.
        /// </summary>
        private void HandleRecallPressed()
        {
            if (weapon == null || recallMotor == null || !recallMotor.CanBegin)
            {
                return;
            }

            if (recallConstraint == null)
            {
                recallConstraint =
                    weapon.GetComponent<WeaponRecallConstraint>();
            }

            if (recallConstraint != null &&
                !recallConstraint.CanRecall)
            {
                RecallDenied?.Invoke();
                return;
            }

            WeaponState state = weapon.State;
            if (state != WeaponState.Outbound &&
                state != WeaponState.Parked &&
                state != WeaponState.Embedded)
            {
                return;
            }

            Vector3 initialDirection = GetInitialDirection(state);

            if (!weapon.BeginRecall())
            {
                return;
            }

            if (state == WeaponState.Outbound && outboundMotor != null)
            {
                outboundMotor.Abort();
            }

            if (!recallMotor.Begin(initialDirection))
            {
                weapon.ResetToHeld();
                return;
            }

            RecallRequested?.Invoke();
        }

        /// <summary>
        /// Returns the recall direction used when return travel begins.
        /// </summary>
        private Vector3 GetInitialDirection(WeaponState state)
        {
            if (state == WeaponState.Outbound &&
                outboundMotor != null &&
                outboundMotor.Direction.sqrMagnitude > 0.0001f)
            {
                // Mid-flight recall keeps the outbound tangent for a brief moment,
                // which creates the characteristic curved reversal.
                return outboundMotor.Direction;
            }

            Transform target =
                weapon.HeldAnchor != null
                    ? weapon.HeldAnchor
                    : weapon.Owner;

            if (target != null)
            {
                Vector3 towardTarget = target.position - weapon.transform.position;
                towardTarget.y = 0f;

                if (towardTarget.sqrMagnitude > 0.0001f)
                {
                    // Parked and embedded recalls begin by pulling away from the resting point.
                    // In particular, this prevents an embedded weapon from trying to travel
                    // farther into the surface that stopped it.
                    return towardTarget.normalized;
                }
            }

            Vector3 forward = weapon.transform.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 0.0001f
                ? forward.normalized
                : Vector3.forward;
        }
    }
}
