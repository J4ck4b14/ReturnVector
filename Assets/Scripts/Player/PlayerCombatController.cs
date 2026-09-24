using System;
using ReturnVector.Weapon;
using UnityEngine;

namespace ReturnVector.Player
{
    /// <summary>
    /// Tracks whether the player currently owns their primary offensive tool.
    /// Later enemy aggression and animation can key off this state.
    /// </summary>
    public sealed class PlayerCombatController : MonoBehaviour
    {
        [SerializeField] private WeaponController weapon;

        public PlayerCombatMode Mode { get; private set; } = PlayerCombatMode.Armed;

        public bool IsArmed => Mode == PlayerCombatMode.Armed;
        public bool IsWeaponAway => Mode == PlayerCombatMode.Unarmed;

        public event Action<PlayerCombatMode, PlayerCombatMode> ModeChanged;

        private void OnEnable()
        {
            if (weapon != null)
            {
                weapon.StateChanged += HandleWeaponStateChanged;
                RefreshFromWeapon(weapon.State);
            }
        }

        private void OnDisable()
        {
            if (weapon != null)
            {
                weapon.StateChanged -= HandleWeaponStateChanged;
            }
        }

        public void Configure(WeaponController newWeapon)
        {
            if (isActiveAndEnabled && weapon != null)
            {
                weapon.StateChanged -= HandleWeaponStateChanged;
            }

            weapon = newWeapon;

            if (isActiveAndEnabled && weapon != null)
            {
                weapon.StateChanged += HandleWeaponStateChanged;
                RefreshFromWeapon(weapon.State);
            }
        }

        private void HandleWeaponStateChanged(WeaponState previous, WeaponState next)
        {
            RefreshFromWeapon(next);
        }

        private void RefreshFromWeapon(WeaponState state)
        {
            PlayerCombatMode next =
                state == WeaponState.Held ||
                state == WeaponState.ThrowAnticipation
                    ? PlayerCombatMode.Armed
                    : state == WeaponState.Disabled
                        ? PlayerCombatMode.Disabled
                        : PlayerCombatMode.Unarmed;

            if (next == Mode)
            {
                return;
            }

            PlayerCombatMode previous = Mode;
            Mode = next;
            ModeChanged?.Invoke(previous, next);
        }
    }
}
