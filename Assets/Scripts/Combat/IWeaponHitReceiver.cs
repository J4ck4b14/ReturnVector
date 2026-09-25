
// Script summary: Optional richer combat contract for targets whose armor or facing changes how the weapon itself should respond.

namespace ReturnVector.Combat
{
    /// <summary>
    /// Optional richer combat contract for targets whose armor or facing
    /// changes how the weapon itself should respond.
    /// </summary>
    public interface IWeaponHitReceiver
    {
        /// <summary>
        /// Resolves the weapon hit.
        /// </summary>
        WeaponHitResult ResolveWeaponHit(in DamageInfo damage);
    }
}
