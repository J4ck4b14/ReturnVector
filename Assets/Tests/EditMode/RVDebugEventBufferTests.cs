using NUnit.Framework;
using ReturnVector.Combat;
using ReturnVector.Debugging;
using UnityEngine;

namespace ReturnVector.Tests
{
    /// <summary>
    /// Edit Mode coverage for RVDebugEventBuffer.
    /// </summary>
    public sealed class RVDebugEventBufferTests
    {
        [Test]
        public void Buffer_DropsOldestWhenCapacityIsExceeded()
        {
            RVDebugEventBuffer buffer = new RVDebugEventBuffer(2);

            buffer.Add(new RVDebugEvent(1, 0f, RVDebugEventKind.StateTransition, AttackPhase.Unknown, Vector3.zero, "one"));
            buffer.Add(new RVDebugEvent(2, 0f, RVDebugEventKind.StateTransition, AttackPhase.Unknown, Vector3.zero, "two"));
            buffer.Add(new RVDebugEvent(3, 0f, RVDebugEventKind.StateTransition, AttackPhase.Unknown, Vector3.zero, "three"));

            Assert.AreEqual(2, buffer.Count);
            Assert.AreEqual(2, buffer.Events[0].Sequence);
            Assert.AreEqual(3, buffer.Events[1].Sequence);
        }

        [Test]
        public void ReducingCapacity_TrimsHistoryImmediately()
        {
            RVDebugEventBuffer buffer = new RVDebugEventBuffer(4);
            for (int i = 1; i <= 4; i++)
            {
                buffer.Add(new RVDebugEvent(i, 0f, RVDebugEventKind.StateTransition, AttackPhase.Unknown, Vector3.zero, i.ToString()));
            }

            buffer.SetCapacity(2);

            Assert.AreEqual(2, buffer.Count);
            Assert.AreEqual(3, buffer.Events[0].Sequence);
            Assert.AreEqual(4, buffer.Events[1].Sequence);
        }
    }
}
