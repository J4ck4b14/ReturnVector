using NUnit.Framework;
using ReturnVector.Combat;
using ReturnVector.Core;
using ReturnVector.Enemies;
using ReturnVector.Weapon;
using UnityEngine;

namespace ReturnVector.Tests
{
    /// <summary>
    /// Edit Mode coverage for ReturnWardenHealth.
    /// </summary>
    public sealed class ReturnWardenHealthTests
    {
        [Test]
        public void OutboundHit_IsReducedAndPinsWeapon()
        {
            GameObject owner =
                new GameObject("Owner");

            GameObject weaponObject =
                new GameObject("Weapon");

            WeaponController weapon =
                weaponObject.AddComponent<
                    WeaponController>();

            weapon.Configure(
                owner.transform,
                null);

            WeaponRecallConstraint constraint =
                weaponObject.AddComponent<
                    WeaponRecallConstraint>();

            constraint.Configure(weapon);

            ReturnWardenTuning tuning =
                ScriptableObject.CreateInstance<
                    ReturnWardenTuning>();

            tuning.ResetDefaults();

            GameObject boss =
                new GameObject("Boss");

            ReturnWardenHealth health =
                boss.AddComponent<
                    ReturnWardenHealth>();

            health.ConfigureBoss(
                tuning,
                constraint);

            DamageInfo damage =
                new DamageInfo(
                    1f,
                    Vector3.zero,
                    Vector3.forward,
                    owner,
                    weaponObject,
                    AttackPhase.Outbound);

            WeaponHitResult result =
                health.ResolveWeaponHit(
                    in damage);

            Assert.IsTrue(
                result.BlocksWeapon);

            Assert.IsTrue(
                constraint.IsPinned);

            Assert.AreEqual(
                tuning.MaxHealth -
                tuning.OutboundDamageMultiplier,
                health.CurrentHealth,
                0.001f);

            Object.DestroyImmediate(
                tuning);
            Object.DestroyImmediate(
                boss);
            Object.DestroyImmediate(
                weaponObject);
            Object.DestroyImmediate(
                owner);
        }

        [Test]
        public void RecallHit_IsAmplifiedAndPierces()
        {
            ReturnWardenTuning tuning =
                ScriptableObject.CreateInstance<
                    ReturnWardenTuning>();

            tuning.ResetDefaults();

            GameObject boss =
                new GameObject("Boss");

            ReturnWardenHealth health =
                boss.AddComponent<
                    ReturnWardenHealth>();

            health.ConfigureBoss(
                tuning,
                null);

            DamageInfo damage =
                new DamageInfo(
                    1f,
                    Vector3.zero,
                    Vector3.back,
                    null,
                    null,
                    AttackPhase.Recall);

            WeaponHitResult result =
                health.ResolveWeaponHit(
                    in damage);

            Assert.IsTrue(
                result.DamagedTarget);

            Assert.IsFalse(
                result.BlocksWeapon);

            Assert.AreEqual(
                tuning.MaxHealth -
                tuning.RecallDamageMultiplier,
                health.CurrentHealth,
                0.001f);

            Object.DestroyImmediate(
                tuning);
            Object.DestroyImmediate(
                boss);
        }


        [Test]
        public void VeryEasy_BossDoesNotEnterSecondPhase()
        {
            GameDifficulty.BeginRun(RunDifficulty.VeryEasy);

            ReturnWardenTuning tuning =
                ScriptableObject.CreateInstance<ReturnWardenTuning>();
            tuning.ResetDefaults();
            tuning.MaxHealth = 2f;

            GameObject boss = new GameObject("Boss");
            ReturnWardenHealth health =
                boss.AddComponent<ReturnWardenHealth>();
            health.ConfigureBoss(tuning, null);

            DamageInfo lethal =
                new DamageInfo(
                    10f,
                    Vector3.zero,
                    Vector3.back,
                    null,
                    null,
                    AttackPhase.Recall);

            health.ResolveWeaponHit(in lethal);

            Assert.IsFalse(health.IsPhaseTwo);
            Assert.IsFalse(health.IsPhaseThree);
            Assert.IsFalse(health.CanReceiveDamage);

            Object.DestroyImmediate(tuning);
            Object.DestroyImmediate(boss);

            GameDifficulty.BeginRun(RunDifficulty.Normal);
            GameDifficulty.ReturnToMenu();
        }

        [Test]
        public void LethalPhaseTwoHit_StartsPhaseThreeAtFullHealth()
        {
            GameDifficulty.BeginRun(RunDifficulty.Hard);

            ReturnWardenTuning tuning =
                ScriptableObject.CreateInstance<
                    ReturnWardenTuning>();

            tuning.ResetDefaults();
            tuning.MaxHealth = 4f;
            tuning.PhaseTwoHealthRatio = 0.5f;

            GameObject boss =
                new GameObject("Boss");

            ReturnWardenHealth health =
                boss.AddComponent<
                    ReturnWardenHealth>();

            health.ConfigureBoss(
                tuning,
                null);

            bool died = false;
            health.Died += _ => died = true;

            DamageInfo phaseOneDamage =
                new DamageInfo(
                    1.2f,
                    Vector3.zero,
                    Vector3.back,
                    null,
                    null,
                    AttackPhase.Recall);

            health.ResolveWeaponHit(
                in phaseOneDamage);

            Assert.IsTrue(
                health.IsPhaseTwo);

            health.CompletePhaseTransition();

            DamageInfo lethalPhaseTwoDamage =
                new DamageInfo(
                    2f,
                    Vector3.zero,
                    Vector3.back,
                    null,
                    null,
                    AttackPhase.Recall);

            health.ResolveWeaponHit(
                in lethalPhaseTwoDamage);

            Assert.IsTrue(
                health.IsPhaseThree);

            Assert.IsTrue(
                health.IsTransitioning);

            Assert.AreEqual(
                tuning.MaxHealth,
                health.CurrentHealth,
                0.001f);

            Assert.IsFalse(
                died);

            health.CompletePhaseTransition();

            DamageInfo finalDamage =
                new DamageInfo(
                    10f,
                    Vector3.zero,
                    Vector3.back,
                    null,
                    null,
                    AttackPhase.Recall);

            health.ResolveWeaponHit(
                in finalDamage);

            Assert.IsTrue(
                died);

            Object.DestroyImmediate(
                tuning);

            Object.DestroyImmediate(
                boss);

            GameDifficulty.BeginRun(RunDifficulty.Normal);
            GameDifficulty.ReturnToMenu();
        }

    }
}
