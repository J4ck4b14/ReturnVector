using UnityEngine;

namespace ReturnVector.Core
{
    public enum RunDifficulty
    {
        VeryEasy = 0,
        Easy = 1,
        Normal = 2,
        Hard = 3
    }

    /// <summary>
    /// Shared run difficulty values. Hard preserves the authored combat values;
    /// the other modes relax pressure without changing the core rules.
    /// </summary>
    public static class GameDifficulty
    {
        public readonly struct Profile
        {
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

            public float PlayerHealth { get; }
            public float EnemyHealthMultiplier { get; }
            public float EnemyMoveSpeedMultiplier { get; }
            public float EnemyDamageMultiplier { get; }
            public float EnemyWindupMultiplier { get; }
            public float EnemyRecoveryMultiplier { get; }
            public float EnemyCooldownMultiplier { get; }
            public float ProjectileSpeedMultiplier { get; }
            public float PlayerMoveSpeedMultiplier { get; }
            public float DodgeDistanceMultiplier { get; }
            public float DodgeCooldownMultiplier { get; }
            public float SpawnFraction { get; }
            public float BossHealthMultiplier { get; }
            public int BossPhaseCount { get; }
            public float EncounterHealFraction { get; }
            public float ScoreMultiplier { get; }
        }

        private static RunDifficulty selected = RunDifficulty.Normal;
        private static bool showMenuOnLoad = true;

        public static RunDifficulty Selected => selected;
        public static bool ShowMenuOnLoad => showMenuOnLoad;
        public static Profile Current => GetProfile(selected);
        public static int MaxBossPhases => Current.BossPhaseCount;
        public static float ShieldFrontRecallDamageMultiplier =>
            selected == RunDifficulty.VeryEasy
                ? 0.65f
                : selected == RunDifficulty.Easy
                    ? 0.30f
                    : 0f;

        public static string SelectedLabel => Label(selected);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession()
        {
            selected = RunDifficulty.Normal;
            showMenuOnLoad = true;
        }

        public static void BeginRun(RunDifficulty difficulty)
        {
            selected = difficulty;
            showMenuOnLoad = false;
        }

        public static void RestartCurrentRun()
        {
            showMenuOnLoad = false;
        }

        public static void ReturnToMenu()
        {
            showMenuOnLoad = true;
        }

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
                default:
                    return "NORMAL";
            }
        }

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
                default:
                    return "The usual experience for players";
            }
        }

        private static Profile GetProfile(RunDifficulty difficulty)
        {
            switch (difficulty)
            {
                case RunDifficulty.VeryEasy:
                    return new Profile(
                        20f,
                        0.50f,
                        0.54f,
                        0.35f,
                        1.70f,
                        1.65f,
                        1.80f,
                        0.62f,
                        1.08f,
                        1.10f,
                        0.62f,
                        0.50f,
                        0.58f,
                        1,
                        1.00f,
                        0.55f);

                case RunDifficulty.Easy:
                    return new Profile(
                        15f,
                        0.72f,
                        0.74f,
                        0.58f,
                        1.40f,
                        1.35f,
                        1.45f,
                        0.78f,
                        1.05f,
                        1.05f,
                        0.80f,
                        0.75f,
                        0.75f,
                        1,
                        0.35f,
                        0.78f);

                case RunDifficulty.Normal:
                    return new Profile(
                        12f,
                        0.90f,
                        0.90f,
                        0.80f,
                        1.15f,
                        1.12f,
                        1.15f,
                        0.90f,
                        1.00f,
                        1.00f,
                        0.92f,
                        1.00f,
                        0.90f,
                        2,
                        0.10f,
                        1.00f);

                default:
                    return new Profile(
                        10f,
                        1.00f,
                        1.00f,
                        1.00f,
                        1.00f,
                        1.00f,
                        1.00f,
                        1.00f,
                        1.00f,
                        1.00f,
                        1.00f,
                        1.00f,
                        1.00f,
                        3,
                        0f,
                        1.25f);
            }
        }
    }
}
