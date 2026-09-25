using NUnit.Framework;
using ReturnVector.Weapon;

// Script summary: Edit Mode coverage for WeaponStateMachine.

namespace ReturnVector.Tests
{
    /// <summary>
    /// Edit Mode coverage for WeaponStateMachine.
    /// </summary>
    public sealed class WeaponStateMachineTests
    {
        /// <summary>
        /// Verifies that starts held.
        /// </summary>
        [Test]
        public void StartsHeld()
        {
            WeaponStateMachine machine = new WeaponStateMachine();
            Assert.AreEqual(WeaponState.Held, machine.Current);
        }

        /// <summary>
        /// Verifies that core loop through parked is legal.
        /// </summary>
        [Test]
        public void CoreLoop_ThroughParked_IsLegal()
        {
            WeaponStateMachine machine = new WeaponStateMachine();

            Assert.IsTrue(machine.TryTransition(WeaponState.ThrowAnticipation));
            Assert.IsTrue(machine.TryTransition(WeaponState.Outbound));
            Assert.IsTrue(machine.TryTransition(WeaponState.Parked));
            Assert.IsTrue(machine.TryTransition(WeaponState.Returning));
            Assert.IsTrue(machine.TryTransition(WeaponState.Catching));
            Assert.IsTrue(machine.TryTransition(WeaponState.Held));
        }

        /// <summary>
        /// Verifies that core loop through embedded is legal.
        /// </summary>
        [Test]
        public void CoreLoop_ThroughEmbedded_IsLegal()
        {
            WeaponStateMachine machine = new WeaponStateMachine();

            Assert.IsTrue(machine.TryTransition(WeaponState.ThrowAnticipation));
            Assert.IsTrue(machine.TryTransition(WeaponState.Outbound));
            Assert.IsTrue(machine.TryTransition(WeaponState.Embedded));
            Assert.IsTrue(machine.TryTransition(WeaponState.Returning));
            Assert.IsTrue(machine.TryTransition(WeaponState.Catching));
            Assert.IsTrue(machine.TryTransition(WeaponState.Held));
        }

        /// <summary>
        /// Verifies that held cannot skip directly to returning.
        /// </summary>
        [Test]
        public void Held_CannotSkipDirectlyToReturning()
        {
            WeaponStateMachine machine = new WeaponStateMachine();

            Assert.IsFalse(machine.TryTransition(WeaponState.Returning));
            Assert.AreEqual(WeaponState.Held, machine.Current);
        }

        /// <summary>
        /// Verifies that returning can be blocked into embedded.
        /// </summary>
        [Test]
        public void Returning_CanBeBlockedIntoEmbedded()
        {
            WeaponStateMachine machine = new WeaponStateMachine();

            machine.TryTransition(WeaponState.ThrowAnticipation);
            machine.TryTransition(WeaponState.Outbound);
            machine.TryTransition(WeaponState.Returning);

            Assert.IsTrue(machine.TryTransition(WeaponState.Embedded));
        }

        /// <summary>
        /// Verifies that parked can recall and catch.
        /// </summary>
        [Test]
        public void Parked_CanRecallAndCatch()
        {
            WeaponStateMachine machine = new WeaponStateMachine();

            machine.TryTransition(WeaponState.ThrowAnticipation);
            machine.TryTransition(WeaponState.Outbound);
            machine.TryTransition(WeaponState.Parked);
            machine.TryTransition(WeaponState.Returning);

            Assert.IsTrue(machine.TryTransition(WeaponState.Catching));
            Assert.IsTrue(machine.TryTransition(WeaponState.Held));
        }
    }
}
