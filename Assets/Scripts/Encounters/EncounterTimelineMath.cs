
// Script summary: Pure timing helpers used by encounter progression.

namespace ReturnVector.Encounters
{
    /// <summary>
    /// Pure timing helpers used by encounter progression.
    /// </summary>
    public static class EncounterTimelineMath
    {
        /// <summary>
        /// Checks whether a spawn entry has reached its authored delay.
        /// </summary>
        public static bool IsSpawnDue(
            float phaseElapsed,
            float spawnDelay)
        {
            return phaseElapsed >= spawnDelay;
        }

        /// <summary>
        /// Checks whether the current encounter phase may advance.
        /// </summary>
        public static bool CanAdvancePhase(
            bool allEntriesSpawned,
            int liveEnemyCount,
            float phaseElapsed,
            float minimumDuration)
        {
            return
                allEntriesSpawned &&
                liveEnemyCount <= 0 &&
                phaseElapsed >= minimumDuration;
        }
    }
}
