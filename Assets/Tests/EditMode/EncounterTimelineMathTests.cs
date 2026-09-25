using NUnit.Framework;
using ReturnVector.Encounters;

// Script summary: Edit Mode coverage for EncounterTimelineMath.

namespace ReturnVector.Tests
{
    /// <summary>
    /// Edit Mode coverage for EncounterTimelineMath.
    /// </summary>
    public sealed class EncounterTimelineMathTests
    {
        /// <summary>
        /// Verifies that spawn is not due before its delay.
        /// </summary>
        [Test]
        public void Spawn_IsNotDueBeforeItsDelay()
        {
            Assert.IsFalse(
                EncounterTimelineMath.IsSpawnDue(
                    0.49f,
                    0.5f));

            Assert.IsTrue(
                EncounterTimelineMath.IsSpawnDue(
                    0.5f,
                    0.5f));
        }

        /// <summary>
        /// Verifies that phase cannot advance while enemy lives.
        /// </summary>
        [Test]
        public void Phase_CannotAdvanceWhileEnemyLives()
        {
            Assert.IsFalse(
                EncounterTimelineMath.CanAdvancePhase(
                    true,
                    1,
                    10f,
                    1f));
        }

        /// <summary>
        /// Verifies that phase cannot advance before every spawn was issued.
        /// </summary>
        [Test]
        public void Phase_CannotAdvanceBeforeEverySpawnWasIssued()
        {
            Assert.IsFalse(
                EncounterTimelineMath.CanAdvancePhase(
                    false,
                    0,
                    10f,
                    1f));
        }

        /// <summary>
        /// Verifies that phase advances only after minimum duration.
        /// </summary>
        [Test]
        public void Phase_AdvancesOnlyAfterMinimumDuration()
        {
            Assert.IsFalse(
                EncounterTimelineMath.CanAdvancePhase(
                    true,
                    0,
                    0.9f,
                    1f));

            Assert.IsTrue(
                EncounterTimelineMath.CanAdvancePhase(
                    true,
                    0,
                    1f,
                    1f));
        }
    }
}
