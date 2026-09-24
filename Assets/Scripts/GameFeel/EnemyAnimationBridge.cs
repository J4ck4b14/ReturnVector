using ReturnVector.Enemies;
using UnityEngine;

namespace ReturnVector.GameFeel
{
    /// <summary>
    /// Feeds enemy runtime state into an optional Animator controller.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyAnimationBridge : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private EnemyHealth health;
        [SerializeField] private EnemyMotor motor;
        [SerializeField] private RusherEnemyAI rusher;
        [SerializeField] private ControllerEnemyAI controller;
        [SerializeField] private ReturnWardenAI warden;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int WindupHash = Animator.StringToHash("Windup");
        private static readonly int PhaseTwoHash = Animator.StringToHash("PhaseTwo");
        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int DeadHash = Animator.StringToHash("Dead");

        private void OnEnable()
        {
            if (health != null)
            {
                health.Damaged += HandleDamaged;
                health.Died += HandleDied;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Damaged -= HandleDamaged;
                health.Died -= HandleDied;
            }
        }

        public void Configure(
            Animator newAnimator,
            EnemyHealth newHealth,
            EnemyMotor newMotor,
            RusherEnemyAI newRusher = null,
            ControllerEnemyAI newController = null,
            ReturnWardenAI newWarden = null)
        {
            if (isActiveAndEnabled && health != null)
            {
                health.Damaged -= HandleDamaged;
                health.Died -= HandleDied;
            }

            animator = newAnimator;
            health = newHealth;
            motor = newMotor;
            rusher = newRusher;
            controller = newController;
            warden = newWarden;

            if (isActiveAndEnabled && health != null)
            {
                health.Damaged += HandleDamaged;
                health.Died += HandleDied;
            }
        }

        private void Update()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetFloat(
                SpeedHash,
                motor != null
                    ? motor.Speed
                    : 0f);

            bool windup =
                (rusher != null &&
                 rusher.State == RusherEnemyState.Windup) ||
                (controller != null &&
                 controller.IsTelegraphing) ||
                (warden != null &&
                 warden.IsTelegraphing);

            animator.SetBool(
                WindupHash,
                windup);

            animator.SetBool(
                PhaseTwoHash,
                warden != null &&
                warden.IsPhaseTwo);
        }

        private void HandleDamaged(
            float currentHealth,
            ReturnVector.Combat.DamageInfo damage)
        {
            if (animator != null)
            {
                animator.SetTrigger(HitHash);
            }
        }

        private void HandleDied(
            ReturnVector.Combat.DamageInfo damage)
        {
            if (animator != null)
            {
                animator.SetTrigger(DeadHash);
            }
        }
    }
}
