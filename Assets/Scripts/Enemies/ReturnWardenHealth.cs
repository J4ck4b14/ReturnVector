using System;
using ReturnVector.Combat;
using ReturnVector.Core;
using ReturnVector.Weapon;
using UnityEngine;

// Script summary: Return Warden hit rules, phase thresholds and recall relationship.

namespace ReturnVector.Enemies
{
    /// <summary>
    /// Return Warden hit rules, phase thresholds and recall relationship.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ReturnWardenHealth : EnemyHealth
    {
        // Warden variables
        [SerializeField] private ReturnWardenTuning tuning;
        [SerializeField] private WeaponRecallConstraint recallConstraint;

        // Runtime state variables
        private bool phaseTwoStarted;
        private bool phaseThreeStarted;
        private int transitionTargetPhase;

        public static ReturnWardenHealth Active { get; private set; }

        public float HealthRatio =>
            MaxHealth <= 0f
                ? 0f
                : CurrentHealth / MaxHealth;

        public bool IsPhaseTwo =>
            phaseTwoStarted &&
            !phaseThreeStarted;

        public bool IsPhaseThree => phaseThreeStarted;
        public bool IsTransitioning { get; private set; }
        public int TransitionTargetPhase => transitionTargetPhase;

        public int CurrentPhase =>
            phaseThreeStarted
                ? 3
                : phaseTwoStarted
                    ? 2
                    : 1;

        public event Action PhaseTwoStarted;
        public event Action PhaseThreeStarted;

        /// <summary>
        /// Subscribes to runtime events when the component becomes active.
        /// </summary>
        private void OnEnable()
        {
            Active = this;
        }

        /// <summary>
        /// Unsubscribes from runtime events when the component is disabled.
        /// </summary>
        private void OnDisable()
        {
            if (Active == this)
            {
                Active = null;
            }
        }

        /// <summary>
        /// Assigns Warden tuning, resets phase state and configures the boss health pool.
        /// </summary>
        public void ConfigureBoss(
            ReturnWardenTuning newTuning,
            WeaponRecallConstraint newRecallConstraint)
        {
            tuning = newTuning;
            recallConstraint = newRecallConstraint;
            phaseTwoStarted = false;
            phaseThreeStarted = false;
            IsTransitioning = false;
            transitionTargetPhase = 0;

            Configure(
                tuning != null
                    ? tuning.MaxHealth
                    : 12f);
        }

        /// <summary>
        /// Completes the phase transition and updates the owning state.
        /// </summary>
        public void CompletePhaseTransition()
        {
            IsTransitioning = false;
            transitionTargetPhase = 0;
        }

        /// <summary>
        /// Resolves the weapon hit.
        /// </summary>
        public override WeaponHitResult ResolveWeaponHit(
            in DamageInfo damage)
        {
            if (!CanReceiveDamage)
            {
                return new WeaponHitResult(false, false);
            }

            if (IsTransitioning)
            {
                return WeaponHitResult.Block;
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
                    ApplyBossDamage(in reduced);

                if (!CanReceiveDamage)
                {
                    recallConstraint?.Release();
                }

                if (CanReceiveDamage &&
                    recallConstraint != null &&
                    !IsTransitioning)
                {
                    float phaseDistanceMultiplier =
                        CurrentPhase >= 2
                            ? tuning.PhaseTwoPinDistanceMultiplier
                            : 1f;

                    recallConstraint.Pin(
                        tuning.PinRepositionDistance *
                        phaseDistanceMultiplier,
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
                    ApplyBossDamage(in amplified);

                if (!CanReceiveDamage)
                {
                    recallConstraint?.Release();
                }

                return damaged
                    ? WeaponHitResult.DamageAndPierce
                    : new WeaponHitResult(false, false);
            }

            bool ordinary =
                ApplyBossDamage(in damage);

            if (!CanReceiveDamage)
            {
                recallConstraint?.Release();
            }

            return ordinary
                ? WeaponHitResult.DamageAndPierce
                : new WeaponHitResult(false, false);
        }

        /// <summary>
        /// Applies an incoming damage payload to this target.
        /// </summary>
        public override void ReceiveDamage(
            in DamageInfo damage)
        {
            ResolveWeaponHit(in damage);
        }

        /// <summary>
        /// Applies the boss damage.
        /// </summary>
        private bool ApplyBossDamage(
            in DamageInfo damage)
        {
            if (ShouldBeginPhaseThree(damage.Amount))
            {
                BeginPhaseThree();
                return true;
            }

            bool damaged =
                ApplyDamage(in damage);

            if (damaged)
            {
                CheckPhaseTwoTransition();
            }

            return damaged;
        }

        /// <summary>
        /// Checks whether the begin phase three should occur.
        /// </summary>
        private bool ShouldBeginPhaseThree(float incomingDamage)
        {
            return
                GameDifficulty.MaxBossPhases >= 3 &&
                phaseTwoStarted &&
                !phaseThreeStarted &&
                !IsTransitioning &&
                CanReceiveDamage &&
                incomingDamage > 0f &&
                incomingDamage >= CurrentHealth;
        }

        /// <summary>
        /// Checks whether the Warden has crossed the phase-two health threshold.
        /// </summary>
        private void CheckPhaseTwoTransition()
        {
            if (GameDifficulty.MaxBossPhases < 2 ||
                phaseTwoStarted ||
                !CanReceiveDamage ||
                tuning == null ||
                HealthRatio > tuning.PhaseTwoHealthRatio)
            {
                return;
            }

            phaseTwoStarted = true;
            IsTransitioning = true;
            transitionTargetPhase = 2;
            recallConstraint?.Release();
            PhaseTwoStarted?.Invoke();
        }

        /// <summary>
        /// Starts the phase three.
        /// </summary>
        private void BeginPhaseThree()
        {
            phaseThreeStarted = true;
            IsTransitioning = true;
            transitionTargetPhase = 3;
            recallConstraint?.Release();

            // Phase three is a fresh health bar with the same authored maximum.
            Configure(MaxHealth);

            PhaseThreeStarted?.Invoke();
        }

        /// <summary>
        /// Scales the damage.
        /// </summary>
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
