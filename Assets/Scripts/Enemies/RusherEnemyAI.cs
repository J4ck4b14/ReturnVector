using ReturnVector.Combat;
using ReturnVector.Core;
using ReturnVector.GameFeel;
using ReturnVector.Player;
using UnityEngine;

// Script summary: Closes distance faster while the player is weaponless and resolves a telegraphed short-range strike.

namespace ReturnVector.Enemies
{
    /// <summary>
    /// Closes distance faster while the player is weaponless and resolves a telegraphed short-range strike.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RusherEnemyAI : MonoBehaviour, IEnemyAttackSource
    {
        // Enemy variables
        [SerializeField] private EnemyMotor motor;
        [SerializeField] private EnemyHealth health;
        [SerializeField] private RusherEnemyTuning tuning;
        [SerializeField] private Transform player;
        [SerializeField] private PlayerTacticalStateSource tacticalState;

        // Runtime state variables
        private RusherEnemyState state;
        private float stateTimer;
        private float stateDuration;
        private Vector3 attackDirection = Vector3.forward;

        public RusherEnemyState State => state;
        public bool IsUsingExposedPressure =>
            tacticalState != null && tacticalState.IsExposed;

        public EnemyAttackStage AttackStage =>
            state == RusherEnemyState.Windup
                ? EnemyAttackStage.Windup
                : state == RusherEnemyState.Recovery
                    ? EnemyAttackStage.Recovery
                    : EnemyAttackStage.None;

        public EnemyAttackStyle AttackStyle => EnemyAttackStyle.Melee;

        public float AttackProgress =>
            stateDuration <= 0.0001f
                ? 0f
                : Mathf.Clamp01(1f - stateTimer / stateDuration);

        public Vector3 AttackDirection => attackDirection;

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
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
            stateDuration = 0f;
        }

        /// <summary>
        /// Advances the component for the current frame.
        /// </summary>
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

        /// <summary>
        /// Advances the the pursuit state for the current frame.
        /// </summary>
        private void TickPursuit(float deltaTime)
        {
            bool exposed =
                tacticalState != null &&
                tacticalState.IsExposed;

            GameDifficulty.Profile difficulty = GameDifficulty.Current;
            float speed =
                (exposed
                    ? tuning.ExposedMoveSpeed
                    : tuning.ArmedMoveSpeed) *
                difficulty.EnemyMoveSpeedMultiplier;

            float distance =
                FlatDistance(transform.position, player.position);

            if (distance <= tuning.AttackRange)
            {
                attackDirection = FlatDirectionToPlayer();
                state = RusherEnemyState.Windup;
                stateDuration =
                    (exposed
                        ? tuning.ExposedWindup
                        : tuning.ArmedWindup) *
                    difficulty.EnemyWindupMultiplier;
                stateTimer = stateDuration;
                motor?.Stop();
                return;
            }

            motor?.MoveToward(
                player.position,
                speed,
                deltaTime,
                tuning.AttackRange * 0.85f);
        }

        /// <summary>
        /// Advances the the windup state for the current frame.
        /// </summary>
        private void TickWindup(float deltaTime)
        {
            motor?.Stop();
            motor?.FaceTarget(player.position, deltaTime);

            attackDirection = FlatDirectionToPlayer();
            stateTimer -= deltaTime;
            if (stateTimer > 0f)
            {
                return;
            }

            ResolveStrike();

            state = RusherEnemyState.Recovery;
            stateDuration =
                tuning.RecoverySeconds *
                GameDifficulty.Current.EnemyRecoveryMultiplier;
            stateTimer = stateDuration;
        }

        /// <summary>
        /// Advances the the recovery state for the current frame.
        /// </summary>
        private void TickRecovery(float deltaTime)
        {
            motor?.Stop();
            motor?.FaceTarget(player.position, deltaTime);

            stateTimer -= deltaTime;
            if (stateTimer <= 0f)
            {
                state = RusherEnemyState.Pursuit;
                stateDuration = 0f;
            }
        }

        /// <summary>
        /// Resolves the strike.
        /// </summary>
        private void ResolveStrike()
        {
            float distance =
                FlatDistance(transform.position, player.position);

            if (distance > tuning.AttackRange * 1.15f)
            {
                return;
            }

            PlayerHealth target =
                player.GetComponentInParent<PlayerHealth>();

            if (target == null || !target.CanReceiveDamage)
            {
                return;
            }

            DamageInfo damage =
                new DamageInfo(
                    tuning.AttackDamage *
                    GameDifficulty.Current.EnemyDamageMultiplier,
                    player.position,
                    attackDirection,
                    gameObject,
                    gameObject,
                    AttackPhase.Unknown);

            target.ReceiveDamage(in damage);
        }

        /// <summary>
        /// Returns the flat direction to player.
        /// </summary>
        private Vector3 FlatDirectionToPlayer()
        {
            Vector3 direction = player.position - transform.position;
            direction.y = 0f;

            return direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : transform.forward;
        }

        /// <summary>
        /// Returns the flat distance.
        /// </summary>
        private static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}
