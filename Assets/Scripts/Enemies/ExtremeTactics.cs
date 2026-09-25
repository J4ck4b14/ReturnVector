using ReturnVector.Core;
using ReturnVector.Player;
using UnityEngine;

// Script summary: Shared Extreme-mode reads for the temporary return corridor created by a throw.

namespace ReturnVector.Enemies
{
    /// <summary>
    /// Shared Extreme-mode reads for the temporary return corridor created by a throw.
    /// </summary>
    public static class ExtremeTactics
    {
        public readonly struct ReturnFrame
        {
            /// <summary>
            /// Creates a new ReturnFrame with the supplied values.
            /// </summary>
            public ReturnFrame(
                Vector3 playerPoint,
                Vector3 weaponPoint,
                Vector3 direction,
                Vector3 perpendicular,
                float length)
            {
                PlayerPoint = playerPoint;
                WeaponPoint = weaponPoint;
                Direction = direction;
                Perpendicular = perpendicular;
                Length = length;
            }

            public Vector3 PlayerPoint { get; }
            public Vector3 WeaponPoint { get; }
            public Vector3 Direction { get; }
            public Vector3 Perpendicular { get; }
            public float Length { get; }
        }

        /// <summary>
        /// Attempts to build the current Extreme-mode return corridor and reports whether it succeeded.
        /// </summary>
        public static bool TryGetReturnFrame(
            Transform player,
            PlayerTacticalStateSource tacticalState,
            out ReturnFrame frame)
        {
            frame = default;

            if (!GameDifficulty.IsExtreme ||
                player == null ||
                tacticalState == null ||
                !tacticalState.CanReadReturnLine ||
                tacticalState.Weapon == null)
            {
                return false;
            }

            Vector3 playerPoint = player.position;
            Vector3 weaponPoint = tacticalState.Weapon.transform.position;
            weaponPoint.y = playerPoint.y;

            Vector3 line = weaponPoint - playerPoint;
            line.y = 0f;
            float length = line.magnitude;

            if (length < 1.35f)
            {
                return false;
            }

            Vector3 direction = line / length;
            Vector3 perpendicular =
                new Vector3(-direction.z, 0f, direction.x);

            frame = new ReturnFrame(
                playerPoint,
                weaponPoint,
                direction,
                perpendicular,
                length);

            return true;
        }

        /// <summary>
        /// Returns a stable side offset used by Extreme-mode coordination.
        /// </summary>
        public static float RoleSide(Component component)
        {
            if (component == null)
            {
                return 1f;
            }

            return (component.GetInstanceID() & 1) == 0
                ? 1f
                : -1f;
        }
    }
}
