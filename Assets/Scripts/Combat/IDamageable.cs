namespace ReturnVector.Combat
{
    /// <summary>
    /// Minimal damage contract shared by player, enemies and debug targets.
    /// </summary>
    public interface IDamageable
    {
        bool CanReceiveDamage { get; }
        void ReceiveDamage(in DamageInfo damage);
    }
}
