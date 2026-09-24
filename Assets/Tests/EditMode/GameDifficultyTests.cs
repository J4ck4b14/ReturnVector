using NUnit.Framework;
using ReturnVector.Core;
using ReturnVector.Enemies;

namespace ReturnVector.Tests
{
    public sealed class GameDifficultyTests
    {
        [TearDown]
        public void TearDown()
        {
            GameDifficulty.BeginRun(RunDifficulty.Normal);
            GameDifficulty.ReturnToMenu();
        }

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
        }

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
