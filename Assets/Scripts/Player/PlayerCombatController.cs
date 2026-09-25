using System;
using ReturnVector.Weapon;
using UnityEngine;

// Script summary: Tracks whether the player currently owns their primary offensive tool. Later enemy aggression and animation can key off this state.

namespace ReturnVector.Player
{
    /// <summary>
    /// Tracks whether the player currently owns their primary offensive tool.
    /// Later enemy aggression and animation can key off this state.
    /// </summary>
    public sealed class PlayerCombatController : MonoBehaviour
    {
        // Weapon variables
        [SerializeField] private WeaponController weapon;

        public PlayerCombatMode Mode { get; private set; } = PlayerCombatMode.Armed;

        public WeaponController Weapon => weapon;
        public bool IsArmed => Mode == PlayerCombatMode.Armed;
        public bool IsWeaponAway => Mode == PlayerCombatMode.Unarmed;

        public event Action<PlayerCombatMode, PlayerCombatMode> ModeChanged;

        /// <summary>
        /// Subscribes to runtime events when the component becomes active.
        /// </summary>
        private void OnEnable()
        {
            if (weapon != null)
            {
                weapon.StateChanged += HandleWeaponStateChanged;
                RefreshFromWeapon(weapon.State);
            }
        }

        /// <summary>
        /// Unsubscribes from runtime events when the component is disabled.
        /// </summary>
        private void OnDisable()
        {
            if (weapon != null)
            {
                weapon.StateChanged -= HandleWeaponStateChanged;
            }
        }

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
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

        /// <summary>
        /// Responds when the weapon lifecycle state changes.
        /// </summary>
        private void HandleWeaponStateChanged(WeaponState previous, WeaponState next)
        {
            RefreshFromWeapon(next);
        }

        /// <summary>
        /// Refreshes the from weapon.
        /// </summary>
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
