using System;
using ReturnVector.Core;

namespace ReturnVector.Weapon
{
    /// <summary>
    /// Owns the legal transitions across the weapon lifecycle.
    /// </summary>
    public sealed class WeaponStateMachine
    {
        private readonly StateMachine<WeaponState> stateMachine =
            new StateMachine<WeaponState>(WeaponState.Held);

        public WeaponState Current => stateMachine.Current;

        public event Action<WeaponState, WeaponState> Transitioned
        {
            add => stateMachine.Transitioned += value;
            remove => stateMachine.Transitioned -= value;
        }

        public bool TryTransition(WeaponState next)
        {
            if (!IsLegal(Current, next))
            {
                return false;
            }

            return stateMachine.TryTransition(next);
        }

        public void ResetToHeld()
        {
            stateMachine.Force(WeaponState.Held);
        }

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
