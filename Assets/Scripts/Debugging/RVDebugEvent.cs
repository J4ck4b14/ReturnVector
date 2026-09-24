using ReturnVector.Combat;
using UnityEngine;

namespace ReturnVector.Debugging
{
    /// <summary>
    /// Immutable entry stored by the combat debug recorder.
    /// </summary>
    public readonly struct RVDebugEvent
    {
        public readonly int Sequence;
        public readonly float Time;
        public readonly RVDebugEventKind Kind;
        public readonly AttackPhase Phase;
        public readonly Vector3 Point;
        public readonly string Detail;

        public RVDebugEvent(
            int sequence,
            float time,
            RVDebugEventKind kind,
            AttackPhase phase,
            Vector3 point,
            string detail)
        {
            Sequence = sequence;
            Time = time;
            Kind = kind;
            Phase = phase;
            Point = point;
            Detail = detail ?? string.Empty;
        }

        public override string ToString()
        {
            string phaseText =
                Phase == AttackPhase.Unknown
                    ? string.Empty
                    : $" [{Phase}]";

            return
                $"#{Sequence:0000} {Time,7:0.000}s " +
                $"{Kind}{phaseText} — {Detail}";
        }
    }
}
