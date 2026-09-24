using ReturnVector.Combat;
using UnityEngine;

namespace ReturnVector.Debugging
{
    /// <summary>
    /// Development-only target used to verify outbound and recall hit resolution.
    /// </summary>
    public sealed class DebugDamageTarget : MonoBehaviour, IDamageable
    {
        [SerializeField] private float health = 1f;
        [SerializeField] private bool disableOnDeath = true;

        public bool CanReceiveDamage => enabled && health > 0f;
        public float Health => health;

        public void Configure(float startingHealth, bool disableWhenDead = true)
        {
            health = Mathf.Max(0f, startingHealth);
            disableOnDeath = disableWhenDead;
        }

        public void ReceiveDamage(in DamageInfo damage)
        {
            if (!CanReceiveDamage)
            {
                return;
            }

            health = Mathf.Max(0f, health - damage.Amount);

            Debug.Log(
                $"[RETURN VECTOR] {name} took {damage.Amount:0.##} " +
                $"{damage.Phase} damage. Remaining health: {health:0.##}",
                this);

            if (health <= 0f && disableOnDeath)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
