using ReturnVector.Core;
using ReturnVector.GameFeel;
using ReturnVector.Player;
using UnityEngine;

// Script summary: Holds distance, commits a visible shot line, then fires at the player. Fire cadence increases while the player is weaponless.

namespace ReturnVector.Enemies
{
    /// <summary>
    /// Holds distance, commits a visible shot line, then fires at the player.
    /// Fire cadence increases while the player is weaponless.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ControllerEnemyAI : MonoBehaviour, IEnemyAttackSource
    {
        private enum AttackState
        {
            Ready = 0,
            Windup = 1,
            Recovery = 2
        }

        // Enemy variables
        [SerializeField] private EnemyMotor motor;
        [SerializeField] private EnemyHealth health;
        [SerializeField] private ControllerEnemyTuning tuning;
        [SerializeField] private Transform player;
        [SerializeField] private PlayerTacticalStateSource tacticalState;
        [SerializeField] private Material projectileMaterial;

        // Runtime state variables
        private AttackState state;
        private float fireCooldown;
        private float stateTimer;
        private float stateDuration;
        private Vector3 lockedDirection = Vector3.forward;

        public bool IsTelegraphing => state == AttackState.Windup;

        public EnemyAttackStage AttackStage =>
            state == AttackState.Windup
                ? EnemyAttackStage.Windup
                : state == AttackState.Recovery
                    ? EnemyAttackStage.Recovery
                    : EnemyAttackStage.None;

        public EnemyAttackStyle AttackStyle => EnemyAttackStyle.Ranged;

        public float AttackProgress =>
            stateDuration <= 0.0001f
                ? 0f
                : Mathf.Clamp01(1f - stateTimer / stateDuration);

        public Vector3 AttackDirection => lockedDirection;

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(
            EnemyMotor newMotor,
            EnemyHealth newHealth,
            ControllerEnemyTuning newTuning,
            Transform newPlayer,
            PlayerTacticalStateSource newTacticalState,
            Material newProjectileMaterial)
        {
            motor = newMotor;
            health = newHealth;
            tuning = newTuning;
            player = newPlayer;
            tacticalState = newTacticalState;
            projectileMaterial = newProjectileMaterial;

            fireCooldown =
                newTuning != null
                    ? newTuning.FireInterval * 0.55f *
                      GameDifficulty.Current.EnemyCooldownMultiplier
                    : 1f;
            state = AttackState.Ready;
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
                case AttackState.Windup:
                    TickWindup(dt);
                    return;

                case AttackState.Recovery:
                    TickRecovery(dt);
                    return;
            }

            MaintainRange(dt);
            fireCooldown -= dt;

            if (fireCooldown <= 0f)
            {
                BeginShot();
            }
        }

        /// <summary>
        /// Moves the Controller toward its preferred firing distance.
        /// </summary>
        private void MaintainRange(float deltaTime)
        {
            if (ExtremeTactics.TryGetReturnFrame(
                    player,
                    tacticalState,
                    out ExtremeTactics.ReturnFrame returnFrame))
            {
                float side =
                    ExtremeTactics.RoleSide(this);

                float flankDistance =
                    Mathf.Clamp(
                        tuning.PreferredDistance,
                        4.2f,
                        6.2f);

                Vector3 flankPoint =
                    returnFrame.PlayerPoint +
                    returnFrame.Perpendicular * side * flankDistance -
                    returnFrame.Direction * 0.9f;

                float flankGap =
                    FlatDistance(transform.position, flankPoint);

                if (flankGap > 0.8f)
                {
                    motor?.MoveToward(
                        flankPoint,
                        tuning.MoveSpeed *
                        GameDifficulty.Current.EnemyMoveSpeedMultiplier,
                        deltaTime,
                        0.55f);
                }
                else
                {
                    motor?.Stop();
                }

                motor?.FaceTarget(player.position, deltaTime);
                return;
            }

            Vector3 toPlayer = player.position - transform.position;
            toPlayer.y = 0f;
            float distance = toPlayer.magnitude;

            if (distance > tuning.PreferredDistance + 1f)
            {
                motor?.MoveToward(
                    player.position,
                    tuning.MoveSpeed *
                    GameDifficulty.Current.EnemyMoveSpeedMultiplier,
                    deltaTime,
                    tuning.PreferredDistance);
                return;
            }

            if (distance < tuning.PreferredDistance - 1f &&
                distance > 0.0001f)
            {
                motor?.MoveAwayFrom(
                    player.position,
                    tuning.MoveSpeed *
                    GameDifficulty.Current.EnemyMoveSpeedMultiplier,
                    deltaTime);
                return;
            }

            motor?.Stop();
            motor?.FaceTarget(player.position, deltaTime);
        }

        /// <summary>
        /// Starts the shot.
        /// </summary>
        private void BeginShot()
        {
            Vector3 origin = transform.position + Vector3.up * 0.3f;
            lockedDirection = player.position - origin;
            lockedDirection.y = 0f;

            if (lockedDirection.sqrMagnitude < 0.0001f)
            {
                lockedDirection = transform.forward;
            }

            lockedDirection.Normalize();
            state = AttackState.Windup;
            stateDuration =
                tuning.TelegraphSeconds *
                GameDifficulty.Current.EnemyWindupMultiplier;
            stateTimer = stateDuration;
            motor?.Stop();
        }

        /// <summary>
        /// Advances the the windup state for the current frame.
        /// </summary>
        private void TickWindup(float deltaTime)
        {
            motor?.Stop();

            Vector3 lookPoint =
                transform.position + lockedDirection * 4f;
            motor?.FaceTarget(lookPoint, deltaTime);

            stateTimer -= deltaTime;
            if (stateTimer > 0f)
            {
                return;
            }

            FireShot();
            state = AttackState.Recovery;
            stateDuration =
                tuning.RecoverySeconds *
                GameDifficulty.Current.EnemyRecoveryMultiplier;
            stateTimer = stateDuration;

            bool exposed =
                tacticalState != null &&
                tacticalState.IsWeaponAway;

            fireCooldown =
                exposed
                    ? tuning.ExposedFireInterval *
                      GameDifficulty.Current.EnemyCooldownMultiplier
                    : tuning.FireInterval *
                      GameDifficulty.Current.EnemyCooldownMultiplier;
        }

        /// <summary>
        /// Advances the the recovery state for the current frame.
        /// </summary>
        private void TickRecovery(float deltaTime)
        {
            motor?.Stop();
            stateTimer -= deltaTime;

            if (stateTimer <= 0f)
            {
                state = AttackState.Ready;
                stateDuration = 0f;
            }
        }

        /// <summary>
        /// Spawns a ranged projectile along the committed firing direction.
        /// </summary>
        private void FireShot()
        {
            PlayerHealth targetHealth =
                player.GetComponentInParent<PlayerHealth>();

            if (targetHealth == null ||
                !targetHealth.CanReceiveDamage)
            {
                return;
            }

            Vector3 origin = transform.position + Vector3.up * 0.3f;

            GameObject projectileObject =
                GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = "Controller_Shot";
            projectileObject.transform.position = origin;
            projectileObject.transform.localScale = Vector3.one * 0.28f;

            Collider collider = projectileObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
                Destroy(collider);
            }

            Renderer renderer = projectileObject.GetComponent<Renderer>();
            if (renderer != null && projectileMaterial != null)
            {
                renderer.sharedMaterial = projectileMaterial;
            }

            EnemyProjectile projectile =
                projectileObject.AddComponent<EnemyProjectile>();

            projectile.Configure(
                targetHealth,
                lockedDirection,
                tuning.ProjectileSpeed *
                GameDifficulty.Current.ProjectileSpeedMultiplier,
                tuning.ProjectileLifetime,
                tuning.HitRadius,
                tuning.ProjectileDamage *
                GameDifficulty.Current.EnemyDamageMultiplier,
                gameObject);
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
