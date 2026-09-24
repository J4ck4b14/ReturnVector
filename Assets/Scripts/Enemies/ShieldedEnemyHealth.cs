using System;
using ReturnVector.Combat;
using ReturnVector.Core;
using UnityEngine;

namespace ReturnVector.Enemies
{
    [DisallowMultipleComponent]
    public sealed class ShieldedEnemyHealth : EnemyHealth
    {
        [SerializeField, Range(-1f, 1f)] private float frontalDotThreshold = 0.25f;
        [SerializeField, Min(1f)] private float rearRecallMultiplier = 1.6f;
        [SerializeField, Min(0f)] private float shieldDeflectionDegrees = 24f;
        private bool shieldActive = true;

        public event Action<DamageInfo> ShieldBlocked;
        public event Action<DamageInfo> RearRecallPunished;

        public void ConfigureShield(
            float health,
            float frontThreshold = 0.25f,
            float recallMultiplier = 1.6f,
            float deflectionDegrees = 24f)
        {
            Configure(health);
            frontalDotThreshold =
                Mathf.Clamp(frontThreshold, -1f, 1f);
            rearRecallMultiplier =
                Mathf.Max(1f, recallMultiplier);
            shieldDeflectionDegrees =
                Mathf.Max(0f, deflectionDegrees);
            shieldActive = true;
        }

        public void SetShieldActive(bool active)
        {
            shieldActive = active;
        }

        public override WeaponHitResult ResolveWeaponHit(
            in DamageInfo damage)
        {
            if (!CanReceiveDamage)
            {
                return new WeaponHitResult(false, false);
            }

            if (!shieldActive)
            {
                return base.ResolveWeaponHit(in damage);
            }

            bool fromFront =
                IsIncomingFromFront(damage.Direction);

            if (fromFront)
            {
                ShieldBlocked?.Invoke(damage);

                // Frontal outbound contact redirects the weapon and preserves the shielded target.
                if (damage.Phase == AttackPhase.Outbound)
                {
                    return WeaponHitResult.Deflect(
                        shieldDeflectionDegrees);
                }

                float assistedRecall =
                    GameDifficulty.ShieldFrontRecallDamageMultiplier;

                if (damage.Phase == AttackPhase.Recall &&
                    assistedRecall > 0f)
                {
                    DamageInfo softened =
                        new DamageInfo(
                            damage.Amount * assistedRecall,
                            damage.Point,
                            damage.Direction,
                            damage.Instigator,
                            damage.Source,
                            damage.Phase);

                    bool assistedDamaged = ApplyDamage(in softened);
                    return assistedDamaged
                        ? WeaponHitResult.DamageAndPierce
                        : new WeaponHitResult(false, false);
                }

                // Normal and Hard keep the full directional shield requirement.
                return new WeaponHitResult(false, false);
            }

            float amount = damage.Amount;

            if (damage.Phase == AttackPhase.Recall)
            {
                amount *= rearRecallMultiplier;
            }

            DamageInfo adjusted =
                new DamageInfo(
                    amount,
                    damage.Point,
                    damage.Direction,
                    damage.Instigator,
                    damage.Source,
                    damage.Phase);

            bool damaged = ApplyDamage(in adjusted);

            if (damaged &&
                damage.Phase == AttackPhase.Recall)
            {
                RearRecallPunished?.Invoke(adjusted);
            }

            return damaged
                ? WeaponHitResult.DamageAndPierce
                : new WeaponHitResult(false, false);
        }

        public override void ReceiveDamage(in DamageInfo damage)
        {
            ResolveWeaponHit(in damage);
        }

        private bool IsIncomingFromFront(
            Vector3 weaponTravelDirection)
        {
            Vector3 travel = weaponTravelDirection;
            travel.y = 0f;

            if (travel.sqrMagnitude < 0.0001f)
            {
                return false;
            }

            // Weapon travels toward the target. The direction from target
            // toward the source is therefore the inverse of travel.
            Vector3 towardSource = -travel.normalized;
            Vector3 forward = transform.forward;
            forward.y = 0f;

            if (forward.sqrMagnitude < 0.0001f)
            {
                return false;
            }

            float dot =
                Vector3.Dot(
                    forward.normalized,
                    towardSource);

            return dot >= frontalDotThreshold;
        }
    }
}
