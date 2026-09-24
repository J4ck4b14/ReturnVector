using ReturnVector.Combat;
using ReturnVector.Player;
using ReturnVector.Weapon;
using UnityEngine;

namespace ReturnVector.Enemies
{
    /// <summary>
    /// Boss behaviour built around weapon pinning, reposition pressure and a telegraphed slam.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ReturnWardenAI : MonoBehaviour
    {
        [SerializeField] private EnemyMotor motor;
        [SerializeField] private ReturnWardenHealth health;
        [SerializeField] private ReturnWardenTuning tuning;
        [SerializeField] private Transform player;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private WeaponRecallConstraint recallConstraint;

        private ReturnWardenState state;
        private float stateTimer;
        private float slamCooldown;

        public ReturnWardenState State => state;
        public bool IsTelegraphing =>
            state == ReturnWardenState.SlamWindup;
        public bool IsPhaseTwo =>
            health != null && health.IsPhaseTwo;
        public bool IsPressuringPinnedWeapon =>
            recallConstraint != null &&
            recallConstraint.IsPinned;

        public void Configure(
            EnemyMotor newMotor,
            ReturnWardenHealth newHealth,
            ReturnWardenTuning newTuning,
            Transform newPlayer,
            PlayerHealth newPlayerHealth,
            WeaponRecallConstraint newRecallConstraint)
        {
            motor = newMotor;
            health = newHealth;
            tuning = newTuning;
            player = newPlayer;
            playerHealth = newPlayerHealth;
            recallConstraint = newRecallConstraint;

            state = ReturnWardenState.Pursuit;
            stateTimer = 0f;
            slamCooldown = 0.4f;
        }

        private void Update()
        {
            if (health == null ||
                !health.CanReceiveDamage)
            {
                state = ReturnWardenState.Dead;
                motor?.Stop();
                recallConstraint?.Release();
                return;
            }

            if (player == null ||
                tuning == null)
            {
                return;
            }

            float dt = Time.deltaTime;
            slamCooldown =
                Mathf.Max(0f, slamCooldown - dt);

            switch (state)
            {
                case ReturnWardenState.Pursuit:
                    TickPursuit(dt);
                    break;

                case ReturnWardenState.SlamWindup:
                    TickSlamWindup(dt);
                    break;

                case ReturnWardenState.SlamRecovery:
                    TickSlamRecovery(dt);
                    break;
            }
        }

        private void TickPursuit(float deltaTime)
        {
            float distance =
                FlatDistance(
                    transform.position,
                    player.position);

            bool pinned =
                recallConstraint != null &&
                recallConstraint.IsPinned;

            // Pin pressure increases pursuit speed while the player creates release distance.
            float speed =
                pinned
                    ? tuning.PinnedPressureSpeed
                    : tuning.BaseMoveSpeed;

            if (health.IsPhaseTwo)
            {
                speed *=
                    tuning.PhaseTwoSpeedMultiplier;
            }

            if (distance <= tuning.SlamRange &&
                slamCooldown <= 0f)
            {
                // Slam resolves from the player's position after the telegraph window.
                state =
                    ReturnWardenState.SlamWindup;

                stateTimer =
                    health.IsPhaseTwo
                        ? tuning.PhaseTwoWindup
                        : tuning.SlamWindup;

                motor?.Stop();
                return;
            }

            motor?.MoveToward(
                player.position,
                speed,
                deltaTime,
                tuning.PreferredDistance);
        }

        private void TickSlamWindup(
            float deltaTime)
        {
            motor?.Stop();
            motor?.FaceTarget(
                player.position,
                deltaTime);

            stateTimer -= deltaTime;

            if (stateTimer > 0f)
            {
                return;
            }

            ResolveSlam();

            state =
                ReturnWardenState.SlamRecovery;

            stateTimer =
                tuning.SlamRecovery;

            slamCooldown =
                tuning.SlamCooldown;
        }

        private void TickSlamRecovery(
            float deltaTime)
        {
            motor?.Stop();
            motor?.FaceTarget(
                player.position,
                deltaTime);

            stateTimer -= deltaTime;

            if (stateTimer <= 0f)
            {
                state =
                    ReturnWardenState.Pursuit;
            }
        }

        private void ResolveSlam()
        {
            if (playerHealth == null ||
                !playerHealth.CanReceiveDamage)
            {
                return;
            }

            float distance =
                FlatDistance(
                    transform.position,
                    player.position);

            if (distance >
                tuning.SlamRange * 1.08f)
            {
                return;
            }

            Vector3 direction =
                player.position -
                transform.position;

            direction.y = 0f;

            DamageInfo damage =
                new DamageInfo(
                    tuning.SlamDamage,
                    player.position,
                    direction.sqrMagnitude > 0.0001f
                        ? direction.normalized
                        : transform.forward,
                    gameObject,
                    gameObject,
                    AttackPhase.Unknown);

            playerHealth.ReceiveDamage(
                in damage);
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
