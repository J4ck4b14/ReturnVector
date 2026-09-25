using System;

// Script summary: Small explicit state container used by gameplay-specific owners.

namespace ReturnVector.Core
{
    /// <summary>
    /// Small explicit state container used by gameplay-specific owners.
    /// </summary>
    public sealed class StateMachine<TState> where TState : struct, Enum
    {
        public TState Current { get; private set; }

        public event Action<TState, TState> Transitioned;

        /// <summary>
        /// Creates a new StateMachine with the supplied values.
        /// </summary>
        public StateMachine(TState initialState)
        {
            Current = initialState;
        }

        /// <summary>
        /// Attempts the requested state transition and reports whether it succeeded.
        /// </summary>
        public bool TryTransition(TState nextState)
        {
            if (Current.Equals(nextState))
            {
                return false;
            }

            TState previous = Current;
            Current = nextState;
            Transitioned?.Invoke(previous, nextState);
            return true;
        }

        /// <summary>
        /// Changes state immediately without applying normal transition validation.
        /// </summary>
        public void Force(TState nextState)
        {
            if (Current.Equals(nextState))
            {
                return;
            }

            TState previous = Current;
            Current = nextState;
            Transitioned?.Invoke(previous, nextState);
        }
    }
}
