using UnityEngine;

// Script summary: Shared run difficulty values. Hard preserves the authored combat values; Extreme keeps those values and adds coordinated systemic pressure.

namespace ReturnVector.Core
{
    public enum RunDifficulty
    {
        VeryEasy = 0,
        Easy = 1,
        Normal = 2,
        Hard = 3,
        Extreme = 4
    }

    /// <summary>
    /// Shared run difficulty values. Hard preserves the authored combat values;
    /// Extreme keeps those values and adds coordinated systemic pressure.
    /// </summary>
    public static class GameDifficulty
    {
        public readonly struct Profile
        {
            /// <summary>
            /// Creates a new Profile with the supplied values.
            /// </summary>
            public Profile(
                float playerHealth,
                float enemyHealthMultiplier,
                float enemyMoveSpeedMultiplier,
                float enemyDamageMultiplier,
                float enemyWindupMultiplier,
                float enemyRecoveryMultiplier,
                float enemyCooldownMultiplier,
                float projectileSpeedMultiplier,
                float playerMoveSpeedMultiplier,
                float dodgeDistanceMultiplier,
                float dodgeCooldownMultiplier,
                float spawnFraction,
                float bossHealthMultiplier,
                int bossPhaseCount,
                float encounterHealFraction,
                float scoreMultiplier)
            {
                PlayerHealth = playerHealth;
                EnemyHealthMultiplier = enemyHealthMultiplier;
                EnemyMoveSpeedMultiplier = enemyMoveSpeedMultiplier;
                EnemyDamageMultiplier = enemyDamageMultiplier;
                EnemyWindupMultiplier = enemyWindupMultiplier;
                EnemyRecoveryMultiplier = enemyRecoveryMultiplier;
                EnemyCooldownMultiplier = enemyCooldownMultiplier;
                ProjectileSpeedMultiplier = projectileSpeedMultiplier;
                PlayerMoveSpeedMultiplier = playerMoveSpeedMultiplier;
                DodgeDistanceMultiplier = dodgeDistanceMultiplier;
                DodgeCooldownMultiplier = dodgeCooldownMultiplier;
                SpawnFraction = spawnFraction;
                BossHealthMultiplier = bossHealthMultiplier;
                BossPhaseCount = bossPhaseCount;
                EncounterHealFraction = encounterHealFraction;
                ScoreMultiplier = scoreMultiplier;
            }

            // Player variables
            public float PlayerHealth { get; }

            // Enemy variables
            public float EnemyHealthMultiplier { get; }
            public float EnemyMoveSpeedMultiplier { get; }
            public float EnemyDamageMultiplier { get; }
            public float EnemyWindupMultiplier { get; }
            public float EnemyRecoveryMultiplier { get; }
            public float EnemyCooldownMultiplier { get; }
            public float ProjectileSpeedMultiplier { get; }

            // Player movement variables
            public float PlayerMoveSpeedMultiplier { get; }
            public float DodgeDistanceMultiplier { get; }
            public float DodgeCooldownMultiplier { get; }

            // Encounter variables
            public float SpawnFraction { get; }

            // Warden variables
            public float BossHealthMultiplier { get; }
            public int BossPhaseCount { get; }

            // Run variables
            public float EncounterHealFraction { get; }
            public float ScoreMultiplier { get; }
        }

        // Difficulty variables
        private static RunDifficulty selected = RunDifficulty.Normal;
        private static bool showMenuOnLoad = true;

        public static RunDifficulty Selected => selected;
        public static bool ShowMenuOnLoad => showMenuOnLoad;
        public static Profile Current => GetProfile(selected);
        public static int MaxBossPhases => Current.BossPhaseCount;
        public static bool IsExtreme => selected == RunDifficulty.Extreme;

        public static float ShieldFrontRecallDamageMultiplier =>
            selected == RunDifficulty.VeryEasy
                ? 0.65f
                : selected == RunDifficulty.Easy
                    ? 0.30f
                    : 0f;

        public static string SelectedLabel => Label(selected);

        /// <summary>
        /// Resets run difficulty and menu state when the runtime subsystem is recreated.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession()
        {
            selected = RunDifficulty.Normal;
            showMenuOnLoad = true;
        }

        /// <summary>
        /// Starts a run using the currently selected difficulty.
        /// </summary>
        public static void BeginRun(RunDifficulty difficulty)
        {
            selected = difficulty;
            showMenuOnLoad = false;
        }

        /// <summary>
        /// Keeps the selected difficulty active for the next scene reload.
        /// </summary>
        public static void RestartCurrentRun()
        {
            showMenuOnLoad = false;
        }

        /// <summary>
        /// Marks the next scene load to open on the main menu.
        /// </summary>
        public static void ReturnToMenu()
        {
            showMenuOnLoad = true;
        }

        /// <summary>
        /// Checks whether an encounter spawn remains active on the selected difficulty.
        /// </summary>
        public static bool ShouldSpawnEntry(
            int entryIndex,
            int entryCount,
            ReturnVector.Enemies.EnemyArchetype archetype)
        {
            if (archetype == ReturnVector.Enemies.EnemyArchetype.ReturnWarden ||
                entryCount <= 1)
            {
                return true;
            }

            int desired = Mathf.Clamp(
                Mathf.CeilToInt(entryCount * Current.SpawnFraction),
                1,
                entryCount);

            if (desired >= entryCount)
            {
                return true;
            }

            if (desired == 1)
            {
                return entryIndex == 0;
            }

            for (int slot = 0; slot < desired; slot++)
            {
                int selectedIndex =
                    Mathf.RoundToInt(
                        slot * (entryCount - 1f) /
                        (desired - 1f));

                if (entryIndex == selectedIndex)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the display label for the requested difficulty.
        /// </summary>
        public static string Label(RunDifficulty difficulty)
        {
            switch (difficulty)
            {
                case RunDifficulty.VeryEasy:
                    return "VERY EASY";
                case RunDifficulty.Easy:
                    return "EASY";
                case RunDifficulty.Hard:
                    return "HARD";
                case RunDifficulty.Extreme:
                    return "EXTREME";
                default:
                    return "NORMAL";
            }
        }

        /// <summary>
        /// Returns the recommendation text for the requested difficulty.
        /// </summary>
        public static string Recommendation(RunDifficulty difficulty)
        {
            switch (difficulty)
            {
                case RunDifficulty.VeryEasy:
                    return "Recommended for non-players";
                case RunDifficulty.Easy:
                    return "Recommended for casual players";
                case RunDifficulty.Hard:
                    return "A challenging experience for players";
                case RunDifficulty.Extreme:
                    return "For players who have mastered RETURN VECTOR";
                default:
                    return "The usual experience for players";
            }
        }

        /// <summary>
        /// Returns the tuning profile for the requested difficulty.
        /// </summary>
        private static Profile GetProfile(RunDifficulty difficulty)
        {
            switch (difficulty)
            {
                case RunDifficulty.VeryEasy:
                    return new Profile(
                        20f, 0.50f, 0.54f, 0.35f,
                        1.70f, 1.65f, 1.80f, 0.62f,
                        1.08f, 1.10f, 0.62f, 0.50f,
                        0.58f, 1, 1.00f, 0.55f);

                case RunDifficulty.Easy:
                    return new Profile(
                        15f, 0.72f, 0.74f, 0.58f,
                        1.40f, 1.35f, 1.45f, 0.78f,
                        1.05f, 1.05f, 0.80f, 0.75f,
                        0.75f, 1, 0.35f, 0.78f);

                case RunDifficulty.Normal:
                    return new Profile(
                        12f, 0.90f, 0.90f, 0.80f,
                        1.15f, 1.12f, 1.15f, 0.90f,
                        1.00f, 1.00f, 0.92f, 1.00f,
                        0.90f, 2, 0.10f, 1.00f);

                case RunDifficulty.Extreme:
                    // Extreme keeps Hard's combat values and adds pressure through
                    // coordination and return-line awareness.
                    return new Profile(
                        10f, 1.00f, 1.00f, 1.00f,
                        1.00f, 1.00f, 1.00f, 1.00f,
                        1.00f, 1.00f, 1.00f, 1.00f,
                        1.00f, 3, 0f, 1.50f);

                default:
                    return new Profile(
                        10f, 1.00f, 1.00f, 1.00f,
                        1.00f, 1.00f, 1.00f, 1.00f,
                        1.00f, 1.00f, 1.00f, 1.00f,
                        1.00f, 3, 0f, 1.25f);
            }
        }
    }
}
