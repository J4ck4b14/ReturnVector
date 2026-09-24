using NUnit.Framework;
using ReturnVector.Combat;
using ReturnVector.Core;
using ReturnVector.Enemies;
using UnityEngine;

namespace ReturnVector.Tests
{
    /// <summary>
    /// Edit Mode coverage for ShieldedEnemyHealth.
    /// </summary>
    public sealed class ShieldedEnemyHealthTests
    {
        [SetUp]
        public void SetUp()
        {
            GameDifficulty.BeginRun(RunDifficulty.Normal);
        }

        [TearDown]
        public void TearDown()
        {
            GameDifficulty.BeginRun(RunDifficulty.Normal);
            GameDifficulty.ReturnToMenu();
        }

        [Test]
        public void FrontalOutboundHit_IsDeflectedWithoutDamage()
        {
            GameObject enemy = new GameObject("Shielded");
            ShieldedEnemyHealth health =
                enemy.AddComponent<ShieldedEnemyHealth>();
            health.ConfigureShield(5f, 0.25f, 1.6f, 30f);

            DamageInfo damage =
                new DamageInfo(
                    1f,
                    Vector3.zero,
                    Vector3.back,
                    null,
                    null,
                    AttackPhase.Outbound);

            WeaponHitResult result =
                health.ResolveWeaponHit(in damage);

            Assert.IsFalse(result.DamagedTarget);
            Assert.IsTrue(result.DeflectsWeapon);
            Assert.IsFalse(result.BlocksWeapon);
            Assert.AreEqual(5f, health.CurrentHealth, 0.001f);

            Object.DestroyImmediate(enemy);
        }

        [Test]
        public void RearRecallHit_ReceivesRecallMultiplier()
        {
            GameObject enemy = new GameObject("Shielded");
            ShieldedEnemyHealth health =
                enemy.AddComponent<ShieldedEnemyHealth>();
            health.ConfigureShield(5f, 0.25f, 1.6f, 30f);

            DamageInfo damage =
                new DamageInfo(
                    1f,
                    Vector3.zero,
                    Vector3.forward,
                    null,
                    null,
                    AttackPhase.Recall);

            WeaponHitResult result =
                health.ResolveWeaponHit(in damage);

            Assert.IsTrue(result.DamagedTarget);
            Assert.AreEqual(3.4f, health.CurrentHealth, 0.001f);

            Object.DestroyImmediate(enemy);
        }


        [Test]
        public void DisabledShield_AllowsFrontalOneHitDamage()
        {
            GameObject enemy = new GameObject("Shielded");
            ShieldedEnemyHealth health =
                enemy.AddComponent<ShieldedEnemyHealth>();

            health.ConfigureShield(1f, 0.25f, 1.6f, 30f);
            health.SetShieldActive(false);

            DamageInfo damage =
                new DamageInfo(
                    1f,
                    Vector3.zero,
                    Vector3.back,
                    null,
                    null,
                    AttackPhase.Outbound);

            WeaponHitResult result =
                health.ResolveWeaponHit(in damage);

            Assert.IsTrue(result.DamagedTarget);
            Assert.IsFalse(health.CanReceiveDamage);

            Object.DestroyImmediate(enemy);
        }


        [Test]
        public void VeryEasy_FrontalRecallDealsReducedDamage()
        {
            GameDifficulty.BeginRun(RunDifficulty.VeryEasy);

            GameObject enemy = new GameObject("Shielded");
            ShieldedEnemyHealth health =
                enemy.AddComponent<ShieldedEnemyHealth>();
            health.ConfigureShield(5f, 0.25f, 1.6f, 30f);

            DamageInfo damage =
                new DamageInfo(
                    1f,
                    Vector3.zero,
                    Vector3.back,
                    null,
                    null,
                    AttackPhase.Recall);

            WeaponHitResult result =
                health.ResolveWeaponHit(in damage);

            Assert.IsTrue(result.DamagedTarget);
            Assert.AreEqual(4.35f, health.CurrentHealth, 0.001f);

            Object.DestroyImmediate(enemy);
        }

        [Test]
        public void FrontalRecall_DoesNotReceiveDamage()
        {
            GameObject enemy = new GameObject("Shielded");
            ShieldedEnemyHealth health =
                enemy.AddComponent<ShieldedEnemyHealth>();
            health.ConfigureShield(5f, 0.25f, 1.6f, 30f);

            DamageInfo damage =
                new DamageInfo(
                    1f,
                    Vector3.zero,
                    Vector3.back,
                    null,
                    null,
                    AttackPhase.Recall);

            WeaponHitResult result =
                health.ResolveWeaponHit(in damage);

            Assert.IsFalse(result.DamagedTarget);
            Assert.IsFalse(result.DeflectsWeapon);
            Assert.AreEqual(5f, health.CurrentHealth, 0.001f);

            Object.DestroyImmediate(enemy);
        }
    }
}
