
// Script summary: Minimal damage contract shared by player, enemies and debug targets.

namespace ReturnVector.Combat
{
    /// <summary>
    /// Minimal damage contract shared by player, enemies and debug targets.
    /// </summary>
    public interface IDamageable
    {
        bool CanReceiveDamage { get; }
        /// <summary>
        /// Applies an incoming damage payload to this target.
        /// </summary>
        void ReceiveDamage(in DamageInfo damage);
    }
}
