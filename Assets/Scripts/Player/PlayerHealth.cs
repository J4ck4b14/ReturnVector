using System;
using ReturnVector.Combat;
using UnityEngine;

namespace ReturnVector.Player
{
    /// <summary>
    /// Tracks persistent player health and publishes damage/death events.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1f)] private float maxHealth = 10f;
        [SerializeField] private bool resetOnDeath = true;

        private float currentHealth;

        public bool CanReceiveDamage => currentHealth > 0f;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        public event Action<float, DamageInfo> Damaged;
        public event Action Died;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void Configure(
            float health,
            bool shouldResetOnDeath = true)
        {
            maxHealth = Mathf.Max(1f, health);
            currentHealth = maxHealth;
            resetOnDeath = shouldResetOnDeath;
        }


        public void ResetToFull()
        {
            currentHealth = maxHealth;
        }

        public void Restore(float amount)
        {
            if (amount <= 0f ||
                currentHealth <= 0f)
            {
                return;
            }

            currentHealth =
                Mathf.Min(
                    maxHealth,
                    currentHealth + amount);
        }

        public void ReceiveDamage(in DamageInfo damage)
        {
            if (!CanReceiveDamage ||
                damage.Amount <= 0f)
            {
                return;
            }

            currentHealth =
                Mathf.Max(
                    0f,
                    currentHealth - damage.Amount);

            Damaged?.Invoke(currentHealth, damage);

            if (currentHealth <= 0f)
            {
                Died?.Invoke();

                if (resetOnDeath)
                {
                    currentHealth = maxHealth;
                }
            }
        }
    }
}
