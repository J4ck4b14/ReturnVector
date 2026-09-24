using System.Collections.Generic;

namespace ReturnVector.Debugging
{
    /// <summary>
    /// Tiny bounded history of recent combat-debug events.
    /// </summary>
    public sealed class RVDebugEventBuffer
    {
        private readonly List<RVDebugEvent> events =
            new List<RVDebugEvent>(64);

        public int Capacity { get; private set; }
        public int Count => events.Count;
        public IReadOnlyList<RVDebugEvent> Events => events;

        public RVDebugEventBuffer(int capacity)
        {
            Capacity = capacity < 1 ? 1 : capacity;
        }

        public void SetCapacity(int capacity)
        {
            Capacity = capacity < 1 ? 1 : capacity;

            int overflow = events.Count - Capacity;
            if (overflow > 0)
            {
                events.RemoveRange(0, overflow);
            }
        }

        public void Add(in RVDebugEvent debugEvent)
        {
            if (events.Count >= Capacity)
            {
                events.RemoveAt(0);
            }

            events.Add(debugEvent);
        }

        public void Clear()
        {
            events.Clear();
        }
    }
}
