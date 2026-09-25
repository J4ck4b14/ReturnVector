using ReturnVector.Combat;
using ReturnVector.Core;
using ReturnVector.GameFeel;
using ReturnVector.Player;
using UnityEngine;

// Script summary: Keeps its shield toward the player and uses a readable shove when the player stays close.

namespace ReturnVector.Enemies
{
    /// <summary>
    /// Keeps its shield toward the player and uses a readable shove when the player stays close.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShieldedEnemyAI : MonoBehaviour, IEnemyAttackSource
    {
        private enum AttackState
        {
            Pursuit = 0,
            Windup = 1,
            Recovery = 2
        }

        // Enemy variables
        [SerializeField] private EnemyMotor motor;
        [SerializeField] private EnemyHealth health;
        [SerializeField] private Transform player;
        [SerializeField] private PlayerTacticalStateSource tacticalState;
        [SerializeField, Min(0f)] private float moveSpeed = 2.2f;
        [SerializeField, Min(0f)] private float preferredDistance = 2.2f;
        [SerializeField, Min(0f)] private float attackRange = 1.55f;
        [SerializeField, Min(0f)] private float attackCooldown = 1.35f;
        [SerializeField, Min(0f)] private float attackWindup = 0.42f;
        [SerializeField, Min(0f)] private float attackRecovery = 0.62f;
        [SerializeField, Min(0f)] private float attackDamage = 1.1f;

        // Runtime state variables
        private AttackState state;
        private float cooldownRemaining;
        private float stateTimer;
        private float stateDuration;
        private Vector3 attackDirection = Vector3.forward;

        public EnemyAttackStage AttackStage =>
            state == AttackState.Windup
                ? EnemyAttackStage.Windup
                : state == AttackState.Recovery
                    ? EnemyAttackStage.Recovery
                    : EnemyAttackStage.None;

        public EnemyAttackStyle AttackStyle => EnemyAttackStyle.Shield;

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
            state = AttackState.Pursuit;
        }

        /// <summary>
        /// Advances the component for the current frame.
        /// </summary>
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

            switch (state)
            {
                case AttackState.Pursuit:
                    TickPursuit(dt);
                    break;

                case AttackState.Windup:
                    TickWindup(dt);
                    break;

                case AttackState.Recovery:
                    TickRecovery(dt);
                    break;
            }
        }

        /// <summary>
        /// Advances the the pursuit state for the current frame.
        /// </summary>
        private void TickPursuit(float deltaTime)
        {
            float distance =
                FlatDistance(transform.position, player.position);

            Vector3 movementTarget = player.position;
            Vector3 facingTarget = player.position;
            float stoppingDistance = preferredDistance;

            if (ExtremeTactics.TryGetReturnFrame(
                    player,
                    tacticalState,
                    out ExtremeTactics.ReturnFrame returnFrame))
            {
                float corridorDistance =
                    Mathf.Clamp(
                        returnFrame.Length * 0.46f,
                        2.1f,
                        4.2f);

                float side =
                    ExtremeTactics.RoleSide(this);

                movementTarget =
                    returnFrame.PlayerPoint +
                    returnFrame.Direction * corridorDistance +
                    returnFrame.Perpendicular * side * 0.52f;

                stoppingDistance = 0.45f;
                facingTarget = returnFrame.WeaponPoint;
            }

            float targetDistance =
                FlatDistance(transform.position, movementTarget);

            if (targetDistance > stoppingDistance + 0.25f)
            {
                motor?.MoveToward(
                    movementTarget,
                    moveSpeed *
                    GameDifficulty.Current.EnemyMoveSpeedMultiplier,
                    deltaTime,
                    stoppingDistance);
            }
            else
            {
                motor?.Stop();
            }

            motor?.FaceTarget(facingTarget, deltaTime);

            if (distance <= attackRange &&
                cooldownRemaining <= 0f)
            {
                attackDirection = FlatDirectionToPlayer();
                state = AttackState.Windup;
                stateDuration =
                    attackWindup *
                    GameDifficulty.Current.EnemyWindupMultiplier;
                stateTimer = stateDuration;
                motor?.Stop();
            }
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

            ResolveShove();
            state = AttackState.Recovery;
            stateDuration =
                attackRecovery *
                GameDifficulty.Current.EnemyRecoveryMultiplier;
            stateTimer = stateDuration;
            cooldownRemaining =
                attackCooldown *
                GameDifficulty.Current.EnemyCooldownMultiplier;
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
                state = AttackState.Pursuit;
                stateDuration = 0f;
            }
        }

        /// <summary>
        /// Resolves the shove.
        /// </summary>
        private void ResolveShove()
        {
            if (FlatDistance(transform.position, player.position) >
                attackRange * 1.12f)
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
                    attackDamage *
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
