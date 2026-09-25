using NUnit.Framework;
using ReturnVector.Core;
using ReturnVector.Enemies;

// Script summary: Contains Edit Mode coverage for Game Difficulty.

namespace ReturnVector.Tests
{
    public sealed class GameDifficultyTests
    {
        /// <summary>
        /// Cleans up objects created by the test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            GameDifficulty.BeginRun(RunDifficulty.Normal);
            GameDifficulty.ReturnToMenu();
        }

        /// <summary>
        /// Verifies that boss phase count matches difficulty contract.
        /// </summary>
        [Test]
        public void BossPhaseCount_MatchesDifficultyContract()
        {
            GameDifficulty.BeginRun(RunDifficulty.VeryEasy);
            Assert.AreEqual(1, GameDifficulty.MaxBossPhases);

            GameDifficulty.BeginRun(RunDifficulty.Easy);
            Assert.AreEqual(1, GameDifficulty.MaxBossPhases);

            GameDifficulty.BeginRun(RunDifficulty.Normal);
            Assert.AreEqual(2, GameDifficulty.MaxBossPhases);

            GameDifficulty.BeginRun(RunDifficulty.Hard);
            Assert.AreEqual(3, GameDifficulty.MaxBossPhases);

            GameDifficulty.BeginRun(RunDifficulty.Extreme);
            Assert.AreEqual(3, GameDifficulty.MaxBossPhases);
        }


        /// <summary>
        /// Verifies that extreme preseRVes hard combat values but rAIses score weight.
        /// </summary>
        [Test]
        public void Extreme_PreservesHardCombatValuesButRaisesScoreWeight()
        {
            GameDifficulty.BeginRun(RunDifficulty.Hard);
            GameDifficulty.Profile hard = GameDifficulty.Current;

            GameDifficulty.BeginRun(RunDifficulty.Extreme);
            GameDifficulty.Profile extreme = GameDifficulty.Current;

            Assert.IsTrue(GameDifficulty.IsExtreme);
            Assert.AreEqual(hard.PlayerHealth, extreme.PlayerHealth);
            Assert.AreEqual(hard.EnemyHealthMultiplier, extreme.EnemyHealthMultiplier);
            Assert.AreEqual(hard.EnemyMoveSpeedMultiplier, extreme.EnemyMoveSpeedMultiplier);
            Assert.AreEqual(hard.EnemyDamageMultiplier, extreme.EnemyDamageMultiplier);
            Assert.AreEqual(hard.EnemyWindupMultiplier, extreme.EnemyWindupMultiplier);
            Assert.AreEqual(hard.EnemyRecoveryMultiplier, extreme.EnemyRecoveryMultiplier);
            Assert.AreEqual(hard.EnemyCooldownMultiplier, extreme.EnemyCooldownMultiplier);
            Assert.AreEqual(hard.ProjectileSpeedMultiplier, extreme.ProjectileSpeedMultiplier);
            Assert.Greater(extreme.ScoreMultiplier, hard.ScoreMultiplier);
        }

        /// <summary>
        /// Verifies that very easy reduces multi enemy spawn sets deterministically.
        /// </summary>
        [Test]
        public void VeryEasy_ReducesMultiEnemySpawnSetsDeterministically()
        {
            GameDifficulty.BeginRun(RunDifficulty.VeryEasy);

            Assert.IsTrue(
                GameDifficulty.ShouldSpawnEntry(
                    0,
                    4,
                    EnemyArchetype.Rusher));

            Assert.IsFalse(
                GameDifficulty.ShouldSpawnEntry(
                    1,
                    4,
                    EnemyArchetype.Rusher));

            Assert.IsTrue(
                GameDifficulty.ShouldSpawnEntry(
                    3,
                    4,
                    EnemyArchetype.Controller));
        }
    }
}
