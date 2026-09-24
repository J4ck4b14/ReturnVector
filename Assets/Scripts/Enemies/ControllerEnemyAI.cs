using ReturnVector.Player;
using ReturnVector.Weapon;
using UnityEngine;

namespace ReturnVector.Enemies
{
    /// <summary>
    /// Keeps range and fires disruption shots at the active weapon line.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ControllerEnemyAI : MonoBehaviour
    {
        [SerializeField] private EnemyMotor motor;
        [SerializeField] private EnemyHealth health;
        [SerializeField] private ControllerEnemyTuning tuning;
        [SerializeField] private Transform player;
        [SerializeField] private PlayerTacticalStateSource tacticalState;
        [SerializeField] private WeaponController weapon;
        [SerializeField] private WeaponInterferenceController interference;

        private float fireCooldown;
        private float telegraphRemaining;
        private bool telegraphing;

        public bool IsTelegraphing => telegraphing;

        public void Configure(
            EnemyMotor newMotor,
            EnemyHealth newHealth,
            ControllerEnemyTuning newTuning,
            Transform newPlayer,
            PlayerTacticalStateSource newTacticalState,
            WeaponController newWeapon,
            WeaponInterferenceController newInterference)
        {
            motor = newMotor;
            health = newHealth;
            tuning = newTuning;
            player = newPlayer;
            tacticalState = newTacticalState;
            weapon = newWeapon;
            interference = newInterference;

            fireCooldown =
                newTuning != null
                    ? newTuning.FireInterval * 0.55f
                    : 1f;
            telegraphRemaining = 0f;
            telegraphing = false;
        }

        private void Update()
        {
            if (player == null ||
                weapon == null ||
                tuning == null ||
                health == null ||
                !health.CanReceiveDamage)
            {
                return;
            }

            float dt = Time.deltaTime;

            MaintainRange(dt);

            if (telegraphing)
            {
                TickTelegraph(dt);
                return;
            }

            fireCooldown -= dt;

            // A disruption attempt starts only while there is an active travel line to contest.
            if (fireCooldown <= 0f &&
                CanInterfereWithWeapon())
            {
                telegraphing = true;
                telegraphRemaining =
                    tuning.TelegraphSeconds;
            }
        }

        private void MaintainRange(float deltaTime)
        {
            Vector3 toPlayer =
                player.position -
                transform.position;
            toPlayer.y = 0f;

            float distance = toPlayer.magnitude;

            if (distance >
                tuning.PreferredDistance + 1f)
            {
                motor?.MoveToward(
                    player.position,
                    tuning.MoveSpeed,
                    deltaTime,
                    tuning.PreferredDistance);
                return;
            }

            if (distance <
                tuning.PreferredDistance - 1f &&
                distance > 0.0001f)
            {
                motor?.MoveDirection(
                    -toPlayer.normalized,
                    tuning.MoveSpeed,
                    deltaTime);
                return;
            }

            motor?.Stop();
            motor?.FaceTarget(player.position, deltaTime);
        }

        private void TickTelegraph(float deltaTime)
        {
            motor?.Stop();

            if (weapon != null)
            {
                motor?.FaceTarget(
                    weapon.transform.position,
                    deltaTime);
            }

            telegraphRemaining -= deltaTime;
            if (telegraphRemaining > 0f)
            {
                return;
            }

            telegraphing = false;
            FireDisruptionShot();

            bool exposed =
                tacticalState != null &&
                tacticalState.IsWeaponAway;

            fireCooldown =
                exposed
                    ? tuning.ExposedFireInterval
                    : tuning.FireInterval;
        }

        private bool CanInterfereWithWeapon()
        {
            return weapon.State == WeaponState.Outbound ||
                   weapon.State == WeaponState.Returning;
        }

        private void FireDisruptionShot()
        {
            if (weapon == null ||
                interference == null ||
                !CanInterfereWithWeapon())
            {
                return;
            }

            Vector3 origin =
                transform.position +
                Vector3.up * 0.3f;

            // The shot commits to the weapon's current position at fire time.
            Vector3 direction =
                weapon.transform.position - origin;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            GameObject projectileObject =
                GameObject.CreatePrimitive(
                    PrimitiveType.Sphere);
            projectileObject.name =
                "Controller_DisruptionShot";
            projectileObject.transform.position =
                origin;
            projectileObject.transform.localScale =
                Vector3.one * 0.28f;

            Collider collider =
                projectileObject.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            EnemyDisruptionProjectile projectile =
                projectileObject.AddComponent<
                    EnemyDisruptionProjectile>();

            projectile.Configure(
                weapon,
                interference,
                direction,
                transform.position,
                tuning.ProjectileSpeed,
                tuning.HitRadius,
                tuning.ProjectileLifetime,
                tuning.DeflectionDegrees);
        }
    }
}
