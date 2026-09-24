using ReturnVector.Combat;
using ReturnVector.Player;
using UnityEngine;

namespace ReturnVector.Enemies
{
    /// <summary>
    /// Fixed-line ranged projectile with swept collision against the player and environment.
    /// </summary>
    public sealed class EnemyProjectile : MonoBehaviour
    {
        private const int HitBufferSize = 16;

        private readonly RaycastHit[] hitBuffer = new RaycastHit[HitBufferSize];

        private PlayerHealth playerHealth;
        private Vector3 direction;
        private float speed;
        private float lifetime;
        private float hitRadius;
        private float damage;
        private GameObject instigator;

        public void Configure(
            PlayerHealth newPlayerHealth,
            Vector3 worldDirection,
            float projectileSpeed,
            float projectileLifetime,
            float projectileHitRadius,
            float projectileDamage,
            GameObject source)
        {
            playerHealth = newPlayerHealth;
            direction = worldDirection;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector3.forward;
            }

            direction.Normalize();
            speed = Mathf.Max(0f, projectileSpeed);
            lifetime = Mathf.Max(0f, projectileLifetime);
            hitRadius = Mathf.Max(0.01f, projectileHitRadius);
            damage = Mathf.Max(0f, projectileDamage);
            instigator = source;

            transform.rotation =
                Quaternion.LookRotation(direction, Vector3.up);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            lifetime -= dt;

            if (lifetime <= 0f ||
                playerHealth == null ||
                !playerHealth.CanReceiveDamage)
            {
                Destroy(gameObject);
                return;
            }

            float travel = speed * Mathf.Max(0f, dt);
            if (travel <= 0.000001f)
            {
                return;
            }

            Vector3 start = transform.position;

            int hitCount =
                Physics.SphereCastNonAlloc(
                    start,
                    hitRadius,
                    direction,
                    hitBuffer,
                    travel,
                    ~0,
                    QueryTriggerInteraction.Ignore);

            SortHits(hitCount);

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = hitBuffer[i];
                Collider collider = hit.collider;

                if (collider == null || IsInstigatorCollider(collider))
                {
                    continue;
                }

                transform.position =
                    start + direction * Mathf.Max(0f, hit.distance - 0.01f);

                PlayerHealth target =
                    collider.GetComponentInParent<PlayerHealth>();

                if (target != null && target == playerHealth)
                {
                    DamageInfo damageInfo =
                        new DamageInfo(
                            damage,
                            hit.point,
                            direction,
                            instigator,
                            gameObject,
                            AttackPhase.Unknown);

                    target.ReceiveDamage(in damageInfo);
                }

                Destroy(gameObject);
                return;
            }

            transform.position = start + direction * travel;
        }

        private bool IsInstigatorCollider(Collider collider)
        {
            if (instigator == null || collider == null)
            {
                return false;
            }

            Transform candidate = collider.transform;
            Transform owner = instigator.transform;

            return candidate == owner || candidate.IsChildOf(owner);
        }

        private void SortHits(int count)
        {
            int safeCount = Mathf.Min(count, hitBuffer.Length);

            for (int i = 1; i < safeCount; i++)
            {
                RaycastHit key = hitBuffer[i];
                int j = i - 1;

                while (j >= 0 && hitBuffer[j].distance > key.distance)
                {
                    hitBuffer[j + 1] = hitBuffer[j];
                    j--;
                }

                hitBuffer[j + 1] = key;
            }
        }
    }
}
