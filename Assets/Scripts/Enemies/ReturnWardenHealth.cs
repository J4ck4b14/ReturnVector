using ReturnVector.Combat;
using ReturnVector.Weapon;
using UnityEngine;

namespace ReturnVector.Enemies
{
    /// <summary>
    /// Return Warden hit rules: weak outbound damage, pin activation and amplified recall damage.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ReturnWardenHealth : EnemyHealth
    {
        [SerializeField] private ReturnWardenTuning tuning;
        [SerializeField] private WeaponRecallConstraint recallConstraint;

        public float HealthRatio =>
            MaxHealth <= 0f
                ? 0f
                : CurrentHealth / MaxHealth;

        public bool IsPhaseTwo =>
            tuning != null &&
            HealthRatio <= tuning.PhaseTwoHealthRatio;

        public void ConfigureBoss(
            ReturnWardenTuning newTuning,
            WeaponRecallConstraint newRecallConstraint)
        {
            tuning = newTuning;
            recallConstraint = newRecallConstraint;

            Configure(
                tuning != null
                    ? tuning.MaxHealth
                    : 16f);
        }

        public override WeaponHitResult ResolveWeaponHit(
            in DamageInfo damage)
        {
            if (!CanReceiveDamage)
            {
                return new WeaponHitResult(false, false);
            }

            if (tuning == null)
            {
                return base.ResolveWeaponHit(in damage);
            }

            if (damage.Phase == AttackPhase.Outbound)
            {
                DamageInfo reduced =
                    ScaleDamage(
                        damage,
                        tuning.OutboundDamageMultiplier);

                bool damaged =
                    ApplyDamage(in reduced);

                if (!CanReceiveDamage)
                {
                    recallConstraint?.Release();
                }

                if (CanReceiveDamage &&
                    recallConstraint != null)
                {
                    float pinDistance =
                        tuning.PinRepositionDistance *
                        (IsPhaseTwo
                            ? tuning.PhaseTwoPinDistanceMultiplier
                            : 1f);

                    recallConstraint.Pin(
                        pinDistance,
                        tuning.PinFailSafeSeconds);
                }

                return new WeaponHitResult(
                    damagedTarget: damaged,
                    blocksWeapon: true);
            }

            if (damage.Phase == AttackPhase.Recall)
            {
                DamageInfo amplified =
                    ScaleDamage(
                        damage,
                        tuning.RecallDamageMultiplier);

                bool damaged =
                    ApplyDamage(in amplified);

                if (!CanReceiveDamage)
                {
                    recallConstraint?.Release();
                }

                return damaged
                    ? WeaponHitResult.DamageAndPierce
                    : new WeaponHitResult(false, false);
            }

            bool ordinary =
                ApplyDamage(in damage);

            if (!CanReceiveDamage)
            {
                recallConstraint?.Release();
            }

            return ordinary
                ? WeaponHitResult.DamageAndPierce
                : new WeaponHitResult(false, false);
        }

        public override void ReceiveDamage(
            in DamageInfo damage)
        {
            ResolveWeaponHit(in damage);
        }

        private static DamageInfo ScaleDamage(
            in DamageInfo original,
            float multiplier)
        {
            return new DamageInfo(
                original.Amount *
                    Mathf.Max(0f, multiplier),
                original.Point,
                original.Direction,
                original.Instigator,
                original.Source,
                original.Phase);
        }
    }
}
