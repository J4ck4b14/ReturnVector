using System;

namespace ReturnVector.Core
{
    /// <summary>
    /// Small explicit state container used by gameplay-specific owners.
    /// </summary>
    public sealed class StateMachine<TState> where TState : struct, Enum
    {
        public TState Current { get; private set; }

        public event Action<TState, TState> Transitioned;

        public StateMachine(TState initialState)
        {
            Current = initialState;
        }

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
