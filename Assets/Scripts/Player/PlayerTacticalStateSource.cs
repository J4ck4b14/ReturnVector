using System;
using UnityEngine;

// Script summary: Consolidates weapon ownership and movement state into a stable read model. Provides a stable read model for enemy AI and encounter logic.

namespace ReturnVector.Player
{
    /// <summary>
    /// Consolidates weapon ownership and movement state into a stable read model.
    /// Provides a stable read model for enemy AI and encounter logic.
    /// </summary>
    public sealed class PlayerTacticalStateSource : MonoBehaviour
    {
        // Player variables
        [SerializeField] private PlayerCombatController combat;
        [SerializeField] private PlayerMov movement;

        private PlayerTacticalSnapshot current;

        public PlayerTacticalSnapshot Current => current;
        public bool IsWeaponAway => current.IsWeaponAway;
        public bool IsEvading => current.IsEvading;
        public bool IsExposed => current.IsExposed;
        public float WeaponAwaySeconds { get; private set; }
        public ReturnVector.Weapon.WeaponController Weapon =>
            combat != null ? combat.Weapon : null;
        public bool CanReadReturnLine =>
            IsWeaponAway &&
            WeaponAwaySeconds >= 0.35f &&
            Weapon != null;

        public event Action<PlayerTacticalSnapshot> Changed;

        /// <summary>
        /// Subscribes to runtime events when the component becomes active.
        /// </summary>
        private void OnEnable()
        {
            Subscribe();
            Refresh();
        }

        /// <summary>
        /// Unsubscribes from runtime events when the component is disabled.
        /// </summary>
        private void OnDisable()
        {
            Unsubscribe();
        }

        /// <summary>
        /// Advances the component for the current frame.
        /// </summary>
        private void Update()
        {
            if (current.IsWeaponAway)
            {
                WeaponAwaySeconds += Mathf.Max(0f, Time.deltaTime);
            }
            else
            {
                WeaponAwaySeconds = 0f;
            }
        }

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(
            PlayerCombatController newCombat,
            PlayerMov newMovement)
        {
            if (isActiveAndEnabled)
            {
                Unsubscribe();
            }

            combat = newCombat;
            movement = newMovement;

            if (isActiveAndEnabled)
            {
                Subscribe();
                Refresh();
            }
        }

        /// <summary>
        /// Subscribes to the runtime events used by this component.
        /// </summary>
        private void Subscribe()
        {
            if (combat != null)
            {
                combat.ModeChanged += HandleCombatModeChanged;
            }

            if (movement != null)
            {
                movement.StateChanged += HandleMovementStateChanged;
            }
        }

        /// <summary>
        /// Unsubscribes from the runtime events used by this component.
        /// </summary>
        private void Unsubscribe()
        {
            if (combat != null)
            {
                combat.ModeChanged -= HandleCombatModeChanged;
            }

            if (movement != null)
            {
                movement.StateChanged -= HandleMovementStateChanged;
            }
        }

        /// <summary>
        /// Responds when the player combat mode changes.
        /// </summary>
        private void HandleCombatModeChanged(
            PlayerCombatMode previous,
            PlayerCombatMode next)
        {
            Refresh();
        }

        /// <summary>
        /// Responds when the player movement state changes.
        /// </summary>
        private void HandleMovementStateChanged(
            PlayerMovementState previous,
            PlayerMovementState next)
        {
            Refresh();
        }

        /// <summary>
        /// Refreshes the component from its current runtime sources.
        /// </summary>
        private void Refresh()
        {
            PlayerTacticalSnapshot next = new PlayerTacticalSnapshot(
                combat != null ? combat.Mode : PlayerCombatMode.Armed,
                movement != null ? movement.State : PlayerMovementState.Locomotion);

            bool changed =
                next.CombatMode != current.CombatMode ||
                next.MovementState != current.MovementState;

            current = next;

            if (!current.IsWeaponAway)
            {
                WeaponAwaySeconds = 0f;
            }

            if (changed)
            {
                Changed?.Invoke(current);
            }
        }
    }
}
