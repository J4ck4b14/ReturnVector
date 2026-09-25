using System;
using ReturnVector.Combat;
using UnityEngine;

// Script summary: Base enemy health and weapon-hit handling shared by standard archetypes.

namespace ReturnVector.Enemies
{
    /// <summary>
    /// Base enemy health and weapon-hit handling shared by standard archetypes.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyHealth : MonoBehaviour, IDamageable, IWeaponHitReceiver
    {
        // Enemy variables
        [SerializeField, Min(0.1f)] private float maxHealth = 3f;
        [SerializeField] private bool destroyOnDeath;

        private float currentHealth;

        public bool CanReceiveDamage =>
            LifeState == EnemyLifeState.Alive;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public EnemyLifeState LifeState { get; private set; }

        public event Action<float, DamageInfo> Damaged;
        public event Action<DamageInfo> Died;

        /// <summary>
        /// Caches required references and prepares runtime state before the object starts running.
        /// </summary>
        protected virtual void Awake()
        {
            currentHealth = maxHealth;
            LifeState = EnemyLifeState.Alive;
        }

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(float health, bool destroy = false)
        {
            maxHealth = Mathf.Max(0.1f, health);
            currentHealth = maxHealth;
            LifeState = EnemyLifeState.Alive;
            destroyOnDeath = destroy;
        }

        /// <summary>
        /// Resolves the weapon hit.
        /// </summary>
        public virtual WeaponHitResult ResolveWeaponHit(
            in DamageInfo damage)
        {
            bool damaged = ApplyDamage(in damage);
            return damaged
                ? WeaponHitResult.DamageAndPierce
                : new WeaponHitResult(false, false);
        }

        /// <summary>
        /// Applies an incoming damage payload to this target.
        /// </summary>
        public virtual void ReceiveDamage(in DamageInfo damage)
        {
            ApplyDamage(in damage);
        }

        /// <summary>
        /// Applies the damage.
        /// </summary>
        protected bool ApplyDamage(in DamageInfo damage)
        {
            if (!CanReceiveDamage ||
                damage.Amount <= 0f)
            {
                return false;
            }

            currentHealth =
                Mathf.Max(0f, currentHealth - damage.Amount);

            Damaged?.Invoke(currentHealth, damage);

            if (currentHealth <= 0f)
            {
                LifeState = EnemyLifeState.Dead;
                Died?.Invoke(damage);

                if (destroyOnDeath)
                {
                    Destroy(gameObject);
                }
                else
                {
                    DisableEnemyBehaviour();
                }
            }

            return true;
        }

        /// <summary>
        /// Disables active enemy behaviour after death.
        /// </summary>
        private void DisableEnemyBehaviour()
        {
            Collider[] colliders =
                GetComponentsInChildren<Collider>(true);

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = false;
                }
            }

            MonoBehaviour[] behaviours =
                GetComponents<MonoBehaviour>();

            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null ||
                    behaviour == this)
                {
                    continue;
                }

                string typeName =
                    behaviour.GetType().Namespace ?? string.Empty;

                if (typeName.StartsWith(
                        "ReturnVector.Enemies",
                        StringComparison.Ordinal))
                {
                    behaviour.enabled = false;
                }
            }
        }
    }
}
