using System;
using UnityEngine;

namespace ReturnVector.Player
{
    /// <summary>
    /// Consolidates weapon ownership and movement state into a stable read model.
    /// Provides a stable read model for enemy AI and encounter logic.
    /// </summary>
    public sealed class PlayerTacticalStateSource : MonoBehaviour
    {
        [SerializeField] private PlayerCombatController combat;
        [SerializeField] private PlayerMov movement;

        private PlayerTacticalSnapshot current;

        public PlayerTacticalSnapshot Current => current;
        public bool IsWeaponAway => current.IsWeaponAway;
        public bool IsEvading => current.IsEvading;
        public bool IsExposed => current.IsExposed;

        public event Action<PlayerTacticalSnapshot> Changed;

        private void OnEnable()
        {
            Subscribe();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

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

        private void HandleCombatModeChanged(
            PlayerCombatMode previous,
            PlayerCombatMode next)
        {
            Refresh();
        }

        private void HandleMovementStateChanged(
            PlayerMovementState previous,
            PlayerMovementState next)
        {
            Refresh();
        }

        private void Refresh()
        {
            PlayerTacticalSnapshot next = new PlayerTacticalSnapshot(
                combat != null ? combat.Mode : PlayerCombatMode.Armed,
                movement != null ? movement.State : PlayerMovementState.Locomotion);

            bool changed =
                next.CombatMode != current.CombatMode ||
                next.MovementState != current.MovementState;

            current = next;

            if (changed)
            {
                Changed?.Invoke(current);
            }
        }
    }
}
