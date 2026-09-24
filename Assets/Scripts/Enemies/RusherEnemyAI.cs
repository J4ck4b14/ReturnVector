using ReturnVector.Combat;
using ReturnVector.Player;
using UnityEngine;

namespace ReturnVector.Enemies
{
    /// <summary>
    /// Closes distance faster while the player is weaponless and resolves a short-range strike.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RusherEnemyAI : MonoBehaviour
    {
        [SerializeField] private EnemyMotor motor;
        [SerializeField] private EnemyHealth health;
        [SerializeField] private RusherEnemyTuning tuning;
        [SerializeField] private Transform player;
        [SerializeField] private PlayerTacticalStateSource tacticalState;

        private RusherEnemyState state;
        private float stateTimer;

        public RusherEnemyState State => state;
        public bool IsUsingExposedPressure =>
            tacticalState != null &&
            tacticalState.IsExposed;

        public void Configure(
            EnemyMotor newMotor,
            EnemyHealth newHealth,
            RusherEnemyTuning newTuning,
            Transform newPlayer,
            PlayerTacticalStateSource newTacticalState)
        {
            motor = newMotor;
            health = newHealth;
            tuning = newTuning;
            player = newPlayer;
            tacticalState = newTacticalState;
            state = RusherEnemyState.Pursuit;
            stateTimer = 0f;
        }

        private void Update()
        {
            if (player == null ||
                tuning == null ||
                health == null ||
                !health.CanReceiveDamage)
            {
                return;
            }

            float dt = Time.deltaTime;

            switch (state)
            {
                case RusherEnemyState.Pursuit:
                    TickPursuit(dt);
                    break;

                case RusherEnemyState.Windup:
                    TickWindup(dt);
                    break;

                case RusherEnemyState.Recovery:
                    TickRecovery(dt);
                    break;
            }
        }

        private void TickPursuit(float deltaTime)
        {
            // Weapon absence is the Rusher's pressure window.
            bool exposed =
                tacticalState != null &&
                tacticalState.IsExposed;

            float speed =
                exposed
                    ? tuning.ExposedMoveSpeed
                    : tuning.ArmedMoveSpeed;

            float distance =
                FlatDistance(
                    transform.position,
                    player.position);

            if (distance <= tuning.AttackRange)
            {
                // Windup fixes the attack timing while leaving room for the player to move out.
                state = RusherEnemyState.Windup;
                stateTimer =
                    exposed
                        ? tuning.ExposedWindup
                        : tuning.ArmedWindup;
                motor?.Stop();
                return;
            }

            motor?.MoveToward(
                player.position,
                speed,
                deltaTime,
                tuning.AttackRange * 0.85f);
        }

        private void TickWindup(float deltaTime)
        {
            motor?.Stop();
            motor?.FaceTarget(player.position, deltaTime);

            stateTimer -= deltaTime;
            if (stateTimer > 0f)
            {
                return;
            }

            float distance =
                FlatDistance(
                    transform.position,
                    player.position);

            if (distance <= tuning.AttackRange * 1.15f)
            {
                PlayerHealth target =
                    player.GetComponentInParent<PlayerHealth>();

                if (target != null &&
                    target.CanReceiveDamage)
                {
                    Vector3 direction =
                        player.position - transform.position;
                    direction.y = 0f;

                    DamageInfo damage =
                        new DamageInfo(
                            tuning.AttackDamage,
                            player.position,
                            direction.sqrMagnitude > 0.0001f
                                ? direction.normalized
                                : transform.forward,
                            gameObject,
                            gameObject,
                            AttackPhase.Unknown);

                    target.ReceiveDamage(in damage);
                }
            }

            state = RusherEnemyState.Recovery;
            stateTimer = tuning.RecoverySeconds;
        }

        private void TickRecovery(float deltaTime)
        {
            motor?.Stop();
            motor?.FaceTarget(player.position, deltaTime);

            stateTimer -= deltaTime;
            if (stateTimer <= 0f)
            {
                state = RusherEnemyState.Pursuit;
            }
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
