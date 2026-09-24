using NUnit.Framework;
using ReturnVector.Weapon;

namespace ReturnVector.Tests
{
    /// <summary>
    /// Edit Mode coverage for WeaponStateMachine.
    /// </summary>
    public sealed class WeaponStateMachineTests
    {
        [Test]
        public void StartsHeld()
        {
            WeaponStateMachine machine = new WeaponStateMachine();
            Assert.AreEqual(WeaponState.Held, machine.Current);
        }

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

        [Test]
        public void Held_CannotSkipDirectlyToReturning()
        {
            WeaponStateMachine machine = new WeaponStateMachine();

            Assert.IsFalse(machine.TryTransition(WeaponState.Returning));
            Assert.AreEqual(WeaponState.Held, machine.Current);
        }

        [Test]
        public void Returning_CanBeBlockedIntoEmbedded()
        {
            WeaponStateMachine machine = new WeaponStateMachine();

            machine.TryTransition(WeaponState.ThrowAnticipation);
            machine.TryTransition(WeaponState.Outbound);
            machine.TryTransition(WeaponState.Returning);

            Assert.IsTrue(machine.TryTransition(WeaponState.Embedded));
        }

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
