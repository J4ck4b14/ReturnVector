using System;
using ReturnVector.Core;

// Script summary: Owns the legal transitions across the weapon lifecycle.

namespace ReturnVector.Weapon
{
    /// <summary>
    /// Owns the legal transitions across the weapon lifecycle.
    /// </summary>
    public sealed class WeaponStateMachine
    {
        // Runtime state variables
        private readonly StateMachine<WeaponState> stateMachine =
            new StateMachine<WeaponState>(WeaponState.Held);

        public WeaponState Current => stateMachine.Current;

        public event Action<WeaponState, WeaponState> Transitioned
        {
            add => stateMachine.Transitioned += value;
            remove => stateMachine.Transitioned -= value;
        }

        /// <summary>
        /// Attempts the requested state transition and reports whether it succeeded.
        /// </summary>
        public bool TryTransition(WeaponState next)
        {
            if (!IsLegal(Current, next))
            {
                return false;
            }

            return stateMachine.TryTransition(next);
        }

        /// <summary>
        /// Resets the to held.
        /// </summary>
        public void ResetToHeld()
        {
            stateMachine.Force(WeaponState.Held);
        }

        /// <summary>
        /// Checks whether the requested weapon state transition is legal.
        /// </summary>
        public static bool IsLegal(WeaponState from, WeaponState to)
        {
            switch (from)
            {
                case WeaponState.Held:
                    return to == WeaponState.ThrowAnticipation ||
                           to == WeaponState.Disabled;

                case WeaponState.ThrowAnticipation:
                    return to == WeaponState.Outbound ||
                           to == WeaponState.Held ||
                           to == WeaponState.Disabled;

                case WeaponState.Outbound:
                    return to == WeaponState.Parked ||
                           to == WeaponState.Embedded ||
                           to == WeaponState.Returning ||
                           to == WeaponState.Disabled;

                case WeaponState.Parked:
                case WeaponState.Embedded:
                    return to == WeaponState.Returning ||
                           to == WeaponState.Disabled;

                case WeaponState.Returning:
                    return to == WeaponState.Catching ||
                           to == WeaponState.Embedded ||
                           to == WeaponState.Disabled;

                case WeaponState.Catching:
                    return to == WeaponState.Held ||
                           to == WeaponState.Disabled;

                case WeaponState.Disabled:
                    return to == WeaponState.Held;

                default:
                    return false;
            }
        }
    }
}
