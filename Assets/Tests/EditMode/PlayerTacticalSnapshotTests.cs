using NUnit.Framework;
using ReturnVector.Player;

// Script summary: Edit Mode coverage for PlayerTacticalSnapshot.

namespace ReturnVector.Tests
{
    /// <summary>
    /// Edit Mode coverage for PlayerTacticalSnapshot.
    /// </summary>
    public sealed class PlayerTacticalSnapshotTests
    {
        /// <summary>
        /// Verifies that unarmed locomotion is exposed.
        /// </summary>
        [Test]
        public void UnarmedLocomotion_IsExposed()
        {
            PlayerTacticalSnapshot snapshot = new PlayerTacticalSnapshot(
                PlayerCombatMode.Unarmed,
                PlayerMovementState.Locomotion);

            Assert.IsTrue(snapshot.IsWeaponAway);
            Assert.IsFalse(snapshot.IsEvading);
            Assert.IsTrue(snapshot.IsExposed);
        }

        /// <summary>
        /// Verifies that unarmed dodge is not exposed window.
        /// </summary>
        [Test]
        public void UnarmedDodge_IsNotExposedWindow()
        {
            PlayerTacticalSnapshot snapshot = new PlayerTacticalSnapshot(
                PlayerCombatMode.Unarmed,
                PlayerMovementState.Dodging);

            Assert.IsTrue(snapshot.IsWeaponAway);
            Assert.IsTrue(snapshot.IsEvading);
            Assert.IsFalse(snapshot.IsExposed);
        }

        /// <summary>
        /// Verifies that armed locomotion is not weapon away.
        /// </summary>
        [Test]
        public void ArmedLocomotion_IsNotWeaponAway()
        {
            PlayerTacticalSnapshot snapshot = new PlayerTacticalSnapshot(
                PlayerCombatMode.Armed,
                PlayerMovementState.Locomotion);

            Assert.IsFalse(snapshot.IsWeaponAway);
            Assert.IsFalse(snapshot.IsExposed);
        }
    }
}
