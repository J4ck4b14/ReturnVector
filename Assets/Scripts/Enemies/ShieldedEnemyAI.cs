using ReturnVector.Combat;
using ReturnVector.Player;
using UnityEngine;

namespace ReturnVector.Enemies
{
    [DisallowMultipleComponent]
    public sealed class ShieldedEnemyAI : MonoBehaviour
    {
        [SerializeField] private EnemyMotor motor;
        [SerializeField] private EnemyHealth health;
        [SerializeField] private Transform player;
        [SerializeField] private PlayerTacticalStateSource tacticalState;
        [SerializeField, Min(0f)] private float moveSpeed = 2.2f;
        [SerializeField, Min(0f)] private float preferredDistance = 2.2f;
        [SerializeField, Min(0f)] private float attackRange = 1.55f;
        [SerializeField, Min(0f)] private float attackCooldown = 1.35f;
        [SerializeField, Min(0f)] private float attackDamage = 1.1f;

        private float cooldownRemaining;

        public void Configure(
            EnemyMotor newMotor,
            EnemyHealth newHealth,
            Transform newPlayer,
            PlayerTacticalStateSource newTacticalState,
            float speed = 2.2f,
            float distance = 2.2f)
        {
            motor = newMotor;
            health = newHealth;
            player = newPlayer;
            tacticalState = newTacticalState;
            moveSpeed = Mathf.Max(0f, speed);
            preferredDistance = Mathf.Max(0f, distance);
            cooldownRemaining = 0f;
        }

        private void Update()
        {
            if (player == null ||
                health == null ||
                !health.CanReceiveDamage)
            {
                return;
            }

            float dt = Time.deltaTime;
            cooldownRemaining =
                Mathf.Max(0f, cooldownRemaining - dt);

            float distance =
                FlatDistance(
                    transform.position,
                    player.position);

            if (distance > preferredDistance + 0.25f)
            {
                motor?.MoveToward(
                    player.position,
                    moveSpeed,
                    dt,
                    preferredDistance);
            }
            else
            {
                motor?.Stop();
                motor?.FaceTarget(player.position, dt);
            }

            // The shield keeps its front on the player. That is the point:
            // a return line from behind is safer than repeatedly throwing forward.
            motor?.FaceTarget(player.position, dt);

            if (distance <= attackRange &&
                cooldownRemaining <= 0f)
            {
                AttackPlayer();
                cooldownRemaining = attackCooldown;
            }
        }

        private void AttackPlayer()
        {
            PlayerHealth target =
                player.GetComponentInParent<PlayerHealth>();

            if (target == null ||
                !target.CanReceiveDamage)
            {
                return;
            }

            Vector3 direction =
                player.position - transform.position;
            direction.y = 0f;

            DamageInfo damage =
                new DamageInfo(
                    attackDamage,
                    player.position,
                    direction.sqrMagnitude > 0.0001f
                        ? direction.normalized
                        : transform.forward,
                    gameObject,
                    gameObject,
                    AttackPhase.Unknown);

            target.ReceiveDamage(in damage);
        }

        private static float FlatDistance(
            Vector3 a,
            Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}
