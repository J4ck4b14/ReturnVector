using System;
using ReturnVector.Combat;
using ReturnVector.Core;
using ReturnVector.GameFeel;
using ReturnVector.Player;
using ReturnVector.Weapon;
using UnityEngine;

namespace ReturnVector.Enemies
{
    /// <summary>
    /// Return Warden behaviour. Distance and phase select its telegraphed attack vocabulary.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ReturnWardenAI : MonoBehaviour, IEnemyAttackSource
    {
        [SerializeField] private EnemyMotor motor;
        [SerializeField] private ReturnWardenHealth health;
        [SerializeField] private ReturnWardenTuning tuning;
        [SerializeField] private Transform player;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerMov playerMovement;
        [SerializeField] private WeaponRecallConstraint recallConstraint;

        private ReturnWardenState state;
        private ReturnWardenAttackKind currentAttack;
        private float stateTimer;
        private float stateDuration;
        private float attackCooldown;
        private int attackSerial;
        private int transitionTargetPhase;

        private Vector3 attackDirection = Vector3.forward;
        private float chargeRemainingDistance;
        private bool chargeHitPlayer;

        public ReturnWardenState State => state;
        public ReturnWardenAttackKind CurrentAttack => currentAttack;
        public bool IsPhaseTwo => health != null && health.IsPhaseTwo;
        public bool IsPhaseThree => health != null && health.IsPhaseThree;
        public bool IsPressuringPinnedWeapon =>
            recallConstraint != null && recallConstraint.IsPinned;

        public EnemyAttackStage AttackStage =>
            state == ReturnWardenState.Windup
                ? EnemyAttackStage.Windup
                : state == ReturnWardenState.Active
                    ? EnemyAttackStage.Active
                    : state == ReturnWardenState.Recovery
                        ? EnemyAttackStage.Recovery
                        : EnemyAttackStage.None;

        public EnemyAttackStyle AttackStyle =>
            currentAttack == ReturnWardenAttackKind.Charge
                ? EnemyAttackStyle.BossCharge
                : currentAttack == ReturnWardenAttackKind.Shockwave
                    ? EnemyAttackStyle.BossShockwave
                    : EnemyAttackStyle.BossSlam;

        public float AttackProgress =>
            stateDuration <= 0.0001f
                ? 0f
                : Mathf.Clamp01(1f - stateTimer / stateDuration);

        public Vector3 AttackDirection => attackDirection;

        public event Action PhaseTransitionStarted;
        public event Action PhaseTransitionCompleted;
        public event Action PhaseThreeTransitionStarted;
        public event Action PhaseThreeTransitionCompleted;
        public event Action ShockwaveReleased;

        public void Configure(
            EnemyMotor newMotor,
            ReturnWardenHealth newHealth,
            ReturnWardenTuning newTuning,
            Transform newPlayer,
            PlayerHealth newPlayerHealth,
            WeaponRecallConstraint newRecallConstraint)
        {
            UnsubscribeFromHealth();

            motor = newMotor;
            health = newHealth;
            tuning = newTuning;
            player = newPlayer;
            playerHealth = newPlayerHealth;
            playerMovement =
                newPlayer != null
                    ? newPlayer.GetComponentInParent<PlayerMov>()
                    : null;
            recallConstraint = newRecallConstraint;

            state = ReturnWardenState.Pursuit;
            currentAttack = ReturnWardenAttackKind.None;
            stateTimer = 0f;
            stateDuration = 0f;
            attackCooldown = 0.4f;
            attackSerial = 0;
            transitionTargetPhase = 0;

            SubscribeToHealth();
        }

        private void OnEnable()
        {
            SubscribeToHealth();
        }

        private void OnDisable()
        {
            UnsubscribeFromHealth();
        }

        private void Update()
        {
            if (health == null || !health.CanReceiveDamage)
            {
                state = ReturnWardenState.Dead;
                currentAttack = ReturnWardenAttackKind.None;
                motor?.Stop();
                recallConstraint?.Release();
                return;
            }

            if (player == null || tuning == null)
            {
                return;
            }

            float dt = Time.deltaTime;
            attackCooldown = Mathf.Max(0f, attackCooldown - dt);

            switch (state)
            {
                case ReturnWardenState.Pursuit:
                    TickPursuit(dt);
                    break;

                case ReturnWardenState.Windup:
                    TickWindup(dt);
                    break;

                case ReturnWardenState.Active:
                    TickActive(dt);
                    break;

                case ReturnWardenState.Recovery:
                    TickRecovery(dt);
                    break;

                case ReturnWardenState.Transforming:
                    TickTransformation(dt);
                    break;
            }
        }

        private void TickPursuit(float deltaTime)
        {
            float distance =
                FlatDistance(
                    transform.position,
                    player.position);

            if (attackCooldown <= 0f)
            {
                ReturnWardenAttackKind next =
                    ChooseAttack(distance);

                if (next != ReturnWardenAttackKind.None)
                {
                    BeginAttack(next);
                    return;
                }
            }

            bool pinned =
                recallConstraint != null &&
                recallConstraint.IsPinned;

            float speed =
                pinned
                    ? tuning.PinnedPressureSpeed
                    : tuning.BaseMoveSpeed;

            if (health.IsPhaseTwo)
            {
                speed *= tuning.PhaseTwoSpeedMultiplier;
            }
            else if (health.IsPhaseThree)
            {
                speed *=
                    tuning.PhaseTwoSpeedMultiplier *
                    tuning.PhaseThreeSpeedMultiplier;
            }

            speed *=
                GameDifficulty.Current.EnemyMoveSpeedMultiplier;

            motor?.MoveToward(
                player.position,
                speed,
                deltaTime,
                tuning.PreferredDistance);
        }

        private ReturnWardenAttackKind ChooseAttack(float distance)
        {
            bool advancedPhase =
                health.IsPhaseTwo ||
                health.IsPhaseThree;

            int shockwaveStride =
                health.IsPhaseThree
                    ? 2
                    : 3;

            if (advancedPhase &&
                attackSerial > 0 &&
                attackSerial % shockwaveStride == shockwaveStride - 1 &&
                distance <= tuning.ShockwaveRange)
            {
                return ReturnWardenAttackKind.Shockwave;
            }

            if (distance <= tuning.SlamRange)
            {
                return ReturnWardenAttackKind.Slam;
            }

            if (distance <= tuning.ChargeTriggerRange)
            {
                return ReturnWardenAttackKind.Charge;
            }

            return ReturnWardenAttackKind.None;
        }

        private void BeginAttack(ReturnWardenAttackKind attack)
        {
            currentAttack = attack;
            attackDirection = FlatDirectionToPlayer();
            state = ReturnWardenState.Windup;
            motor?.Stop();

            switch (attack)
            {
                case ReturnWardenAttackKind.Slam:
                    stateDuration =
                        health.CurrentPhase >= 2
                            ? tuning.PhaseTwoSlamWindup
                            : tuning.SlamWindup;
                    break;

                case ReturnWardenAttackKind.Charge:
                    stateDuration =
                        health.CurrentPhase >= 2
                            ? tuning.PhaseTwoChargeWindup
                            : tuning.ChargeWindup;
                    break;

                case ReturnWardenAttackKind.Shockwave:
                    stateDuration = tuning.ShockwaveWindup;
                    break;
            }

            if (health.IsPhaseThree)
            {
                stateDuration *= 0.86f;
            }

            stateDuration *=
                GameDifficulty.Current.EnemyWindupMultiplier;

            stateTimer = stateDuration;
        }

        private void TickWindup(float deltaTime)
        {
            motor?.Stop();

            if (currentAttack != ReturnWardenAttackKind.Charge)
            {
                motor?.FaceTarget(player.position, deltaTime);
                attackDirection = FlatDirectionToPlayer();
            }
            else
            {
                Vector3 lookPoint =
                    transform.position +
                    attackDirection * 4f;

                motor?.FaceTarget(
                    lookPoint,
                    deltaTime);
            }

            stateTimer -= deltaTime;
            if (stateTimer <= 0f)
            {
                ResolveOrEnterActiveAttack();
            }
        }

        private void ResolveOrEnterActiveAttack()
        {
            attackSerial++;

            switch (currentAttack)
            {
                case ReturnWardenAttackKind.Slam:
                    ResolveSlam();
                    BeginRecovery(
                        tuning.SlamRecovery,
                        tuning.SlamCooldown);
                    break;

                case ReturnWardenAttackKind.Charge:
                    state = ReturnWardenState.Active;
                    chargeRemainingDistance = tuning.ChargeDistance;
                    chargeHitPlayer = false;

                    float chargeSpeed = CurrentChargeSpeed;
                    stateDuration =
                        chargeSpeed > 0.0001f
                            ? tuning.ChargeDistance / chargeSpeed
                            : 0.01f;

                    stateTimer = stateDuration;
                    break;

                case ReturnWardenAttackKind.Shockwave:
                    ResolveShockwave();
                    BeginRecovery(
                        tuning.ShockwaveRecovery,
                        tuning.ShockwaveCooldown);
                    break;
            }
        }

        private void TickActive(float deltaTime)
        {
            if (currentAttack != ReturnWardenAttackKind.Charge)
            {
                BeginRecovery(0.2f, 0.5f);
                return;
            }

            float travel =
                Mathf.Min(
                    chargeRemainingDistance,
                    CurrentChargeSpeed * deltaTime);

            float effectiveSpeed =
                deltaTime > 0.0001f
                    ? travel / deltaTime
                    : 0f;

            motor?.MoveDirection(
                attackDirection,
                effectiveSpeed,
                deltaTime);

            chargeRemainingDistance =
                Mathf.Max(
                    0f,
                    chargeRemainingDistance - travel);

            stateTimer -= deltaTime;

            if (!chargeHitPlayer &&
                FlatDistance(
                    transform.position,
                    player.position) <=
                tuning.ChargeHitRadius)
            {
                chargeHitPlayer = true;
                DamagePlayer(
                    tuning.ChargeDamage,
                    attackDirection);
            }

            if (chargeRemainingDistance <= 0f ||
                stateTimer <= 0f)
            {
                BeginRecovery(
                    tuning.ChargeRecovery,
                    tuning.ChargeCooldown);
            }
        }

        private void TickRecovery(float deltaTime)
        {
            motor?.Stop();
            stateTimer -= deltaTime;

            if (stateTimer <= 0f)
            {
                state = ReturnWardenState.Pursuit;
                currentAttack = ReturnWardenAttackKind.None;
                stateDuration = 0f;
            }
        }

        private void TickTransformation(float deltaTime)
        {
            motor?.Stop();
            stateTimer -= deltaTime;

            if (stateTimer > 0f)
            {
                return;
            }

            health?.CompletePhaseTransition();
            state = ReturnWardenState.Pursuit;
            currentAttack = ReturnWardenAttackKind.None;
            stateDuration = 0f;
            attackCooldown =
                transitionTargetPhase == 3
                    ? 0.55f
                    : 0.12f;

            if (transitionTargetPhase == 3)
            {
                PhaseThreeTransitionCompleted?.Invoke();
            }
            else
            {
                PhaseTransitionCompleted?.Invoke();
            }

            transitionTargetPhase = 0;
        }

        private void BeginRecovery(
            float recovery,
            float cooldown)
        {
            float recoveryMultiplier = 1f;
            float cooldownMultiplier = 1f;

            if (health.IsPhaseThree)
            {
                recoveryMultiplier =
                    tuning.PhaseTwoRecoveryMultiplier *
                    tuning.PhaseThreeRecoveryMultiplier;

                cooldownMultiplier =
                    tuning.PhaseTwoCooldownMultiplier *
                    tuning.PhaseThreeCooldownMultiplier;
            }
            else if (health.IsPhaseTwo)
            {
                recoveryMultiplier =
                    tuning.PhaseTwoRecoveryMultiplier;

                cooldownMultiplier =
                    tuning.PhaseTwoCooldownMultiplier;
            }

            state = ReturnWardenState.Recovery;
            GameDifficulty.Profile difficulty = GameDifficulty.Current;

            stateDuration =
                Mathf.Max(
                    0.01f,
                    recovery *
                    recoveryMultiplier *
                    difficulty.EnemyRecoveryMultiplier);

            stateTimer = stateDuration;
            attackCooldown =
                Mathf.Max(
                    0f,
                    cooldown *
                    cooldownMultiplier *
                    difficulty.EnemyCooldownMultiplier);

            motor?.Stop();
        }

        private void ResolveSlam()
        {
            if (FlatDistance(
                    transform.position,
                    player.position) <=
                tuning.SlamRange * 1.08f)
            {
                DamagePlayer(
                    tuning.SlamDamage,
                    attackDirection);
            }
        }

        private void ResolveShockwave()
        {
            ShockwaveReleased?.Invoke();

            if (FlatDistance(
                    transform.position,
                    player.position) >
                tuning.ShockwaveRange)
            {
                return;
            }

            Vector3 direction =
                FlatDirectionToPlayer();

            DamagePlayer(
                tuning.ShockwaveDamage,
                direction);

            playerMovement?.ApplyPush(
                direction,
                tuning.ShockwavePushDistance);
        }

        private void DamagePlayer(
            float amount,
            Vector3 direction)
        {
            if (playerHealth == null ||
                !playerHealth.CanReceiveDamage)
            {
                return;
            }

            float multiplier =
                health != null && health.IsPhaseThree
                    ? tuning.PhaseThreeDamageMultiplier
                    : health != null && health.IsPhaseTwo
                        ? tuning.PhaseTwoDamageMultiplier
                        : 1f;

            DamageInfo damage =
                new DamageInfo(
                    amount *
                    multiplier *
                    GameDifficulty.Current.EnemyDamageMultiplier,
                    player.position,
                    direction,
                    gameObject,
                    gameObject,
                    AttackPhase.Unknown);

            playerHealth.ReceiveDamage(in damage);
        }

        private void HandlePhaseTwoStarted()
        {
            BeginTransformation(
                2,
                tuning != null
                    ? tuning.PhaseTransitionDuration
                    : 2.35f);

            PhaseTransitionStarted?.Invoke();
        }

        private void HandlePhaseThreeStarted()
        {
            BeginTransformation(
                3,
                tuning != null
                    ? tuning.PhaseThreeTransitionDuration
                    : 2.65f);

            PhaseThreeTransitionStarted?.Invoke();
        }

        private void BeginTransformation(
            int targetPhase,
            float duration)
        {
            transitionTargetPhase = targetPhase;
            state = ReturnWardenState.Transforming;
            currentAttack = ReturnWardenAttackKind.None;
            stateDuration =
                Mathf.Max(
                    0.25f,
                    duration);

            stateTimer = stateDuration;
            attackCooldown = 0f;
            motor?.Stop();
            recallConstraint?.Release();
        }

        private void SubscribeToHealth()
        {
            if (health == null)
            {
                return;
            }

            health.PhaseTwoStarted -=
                HandlePhaseTwoStarted;

            health.PhaseTwoStarted +=
                HandlePhaseTwoStarted;

            health.PhaseThreeStarted -=
                HandlePhaseThreeStarted;

            health.PhaseThreeStarted +=
                HandlePhaseThreeStarted;
        }

        private void UnsubscribeFromHealth()
        {
            if (health == null)
            {
                return;
            }

            health.PhaseTwoStarted -=
                HandlePhaseTwoStarted;

            health.PhaseThreeStarted -=
                HandlePhaseThreeStarted;
        }

        private float CurrentChargeSpeed
        {
            get
            {
                float speed =
                    tuning.ChargeSpeed;

                if (health != null &&
                    health.IsPhaseTwo)
                {
                    speed *=
                        tuning.PhaseTwoChargeSpeedMultiplier;
                }
                else if (health != null &&
                         health.IsPhaseThree)
                {
                    speed *=
                        tuning.PhaseTwoChargeSpeedMultiplier *
                        tuning.PhaseThreeChargeSpeedMultiplier;
                }

                return
                    speed *
                    GameDifficulty.Current.EnemyMoveSpeedMultiplier;
            }
        }

        private Vector3 FlatDirectionToPlayer()
        {
            Vector3 direction =
                player.position -
                transform.position;

            direction.y = 0f;

            return
                direction.sqrMagnitude > 0.0001f
                    ? direction.normalized
                    : transform.forward;
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
