using UnityEngine;

namespace ReturnVector.Combat
{
    /// <summary>
    /// Data passed to a combat target when a hit is resolved.
    /// Keeping the phase explicit lets enemies react differently to outbound and recall hits.
    /// </summary>
    public readonly struct DamageInfo
    {
        public readonly float Amount;
        public readonly Vector3 Point;
        public readonly Vector3 Direction;
        public readonly GameObject Instigator;
        public readonly GameObject Source;
        public readonly AttackPhase Phase;

        public DamageInfo(
            float amount,
            Vector3 point,
            Vector3 direction,
            GameObject instigator,
            GameObject source,
            AttackPhase phase)
        {
            Amount = amount;
            Point = point;
            Direction = direction;
            Instigator = instigator;
            Source = source;
            Phase = phase;
        }
    }
}
