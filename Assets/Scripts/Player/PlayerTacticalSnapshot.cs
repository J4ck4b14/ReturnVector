namespace ReturnVector.Player
{
    /// <summary>
    /// Read-only combat and movement state exposed to AI and encounter systems.
    /// </summary>
    public readonly struct PlayerTacticalSnapshot
    {
        public readonly PlayerCombatMode CombatMode;
        public readonly PlayerMovementState MovementState;

        public bool IsWeaponAway => CombatMode == PlayerCombatMode.Unarmed;
        public bool IsEvading => MovementState == PlayerMovementState.Dodging;
        public bool IsExposed => IsWeaponAway && !IsEvading;

        public PlayerTacticalSnapshot(
            PlayerCombatMode combatMode,
            PlayerMovementState movementState)
        {
            CombatMode = combatMode;
            MovementState = movementState;
        }
    }
}
