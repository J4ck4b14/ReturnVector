namespace ReturnVector.Encounters
{
    /// <summary>
    /// Pure timing helpers used by encounter progression.
    /// </summary>
    public static class EncounterTimelineMath
    {
        public static bool IsSpawnDue(
            float phaseElapsed,
            float spawnDelay)
        {
            return phaseElapsed >= spawnDelay;
        }

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
