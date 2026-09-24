using ReturnVector.Weapon;
using UnityEngine;

namespace ReturnVector.Debugging
{
    /// <summary>
    /// Single sampled point from the weapon's travelled path.
    /// </summary>
    public readonly struct RVWeaponTraceSample
    {
        public readonly Vector3 Position;
        public readonly WeaponState State;
        public readonly float Time;

        public RVWeaponTraceSample(
            Vector3 position,
            WeaponState state,
            float time)
        {
            Position = position;
            State = state;
            Time = time;
        }
    }
}
