
// Script summary: Explicit lifecycle states for the persistent weapon.

namespace ReturnVector.Weapon
{
    /// <summary>
    /// Explicit lifecycle states for the persistent weapon.
    /// </summary>
    public enum WeaponState
    {
        Held = 0,
        ThrowAnticipation = 1,
        Outbound = 2,
        Parked = 3,
        Embedded = 4,
        Returning = 5,
        Catching = 6,
        Disabled = 7
    }
}
