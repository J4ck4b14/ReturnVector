using ReturnVector.Combat;
using ReturnVector.Weapon;
using UnityEngine;

namespace ReturnVector.Player
{
    /// <summary>
    /// Optional production Animator bridge. Gameplay owns state; animation only consumes it.
    /// </summary>
    public sealed class PlayerAnimationBridge : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private PlayerCombatController combat;
        [SerializeField] private PlayerMov movement;
        [SerializeField] private PlayerThrowController throwController;
        [SerializeField] private PlayerRecallController recallController;
        [SerializeField] private RecallWeaponMotor recallMotor;
        [SerializeField] private PlayerHealth health;

        private static readonly int ArmedHash = Animator.StringToHash("Armed");
        private static readonly int WeaponAwayHash = Animator.StringToHash("WeaponAway");
        private static readonly int DodgingHash = Animator.StringToHash("Dodging");
        private static readonly int SpeedHash = Animator.StringToHash("SpeedNormalized");
        private static readonly int MoveXHash = Animator.StringToHash("MoveX");
        private static readonly int MoveYHash = Animator.StringToHash("MoveY");

        private static readonly int DodgeHash = Animator.StringToHash("Dodge");
        private static readonly int ThrowAnticipationHash = Animator.StringToHash("ThrowAnticipation");
        private static readonly int ThrowReleaseHash = Animator.StringToHash("ThrowRelease");
        private static readonly int RecallHash = Animator.StringToHash("Recall");
        private static readonly int CatchHash = Animator.StringToHash("Catch");
        private static readonly int HitHash = Animator.StringToHash("Hit");

        private void OnEnable()
        {
            Bind();
        }

        private void OnDisable()
        {
            Unbind();
        }

        public void Configure(
            Animator newAnimator,
            PlayerCombatController newCombat,
            PlayerMov newMovement)
        {
            Unbind();

            animator = newAnimator;
            combat = newCombat;
            movement = newMovement;

            if (isActiveAndEnabled)
            {
                Bind();
            }
        }

        public void ConfigureCombatActions(
            PlayerThrowController newThrowController,
            PlayerRecallController newRecallController,
            RecallWeaponMotor newRecallMotor,
            PlayerHealth newHealth)
        {
            Unbind();

            throwController = newThrowController;
            recallController = newRecallController;
            recallMotor = newRecallMotor;
            health = newHealth;

            if (isActiveAndEnabled)
            {
                Bind();
            }
        }

        private void Bind()
        {
            if (movement != null)
            {
                movement.DodgeStarted += HandleDodgeStarted;
            }

            if (throwController != null)
            {
                throwController.ThrowAnticipationStarted += HandleThrowAnticipation;
                throwController.ThrowReleased += HandleThrowReleased;
            }

            if (recallController != null)
            {
                recallController.RecallRequested += HandleRecall;
            }

            if (recallMotor != null)
            {
                recallMotor.CatchStarted += HandleCatch;
            }

            if (health != null)
            {
                health.Damaged += HandleHit;
            }
        }

        private void Unbind()
        {
            if (movement != null)
            {
                movement.DodgeStarted -= HandleDodgeStarted;
            }

            if (throwController != null)
            {
                throwController.ThrowAnticipationStarted -= HandleThrowAnticipation;
                throwController.ThrowReleased -= HandleThrowReleased;
            }

            if (recallController != null)
            {
                recallController.RecallRequested -= HandleRecall;
            }

            if (recallMotor != null)
            {
                recallMotor.CatchStarted -= HandleCatch;
            }

            if (health != null)
            {
                health.Damaged -= HandleHit;
            }
        }

        private void Update()
        {
            if (animator == null || combat == null || movement == null)
            {
                return;
            }

            animator.SetBool(
                ArmedHash,
                combat.Mode == PlayerCombatMode.Armed);

            animator.SetBool(
                WeaponAwayHash,
                combat.Mode == PlayerCombatMode.Unarmed);

            animator.SetBool(
                DodgingHash,
                movement.IsDodging);

            animator.SetFloat(
                SpeedHash,
                movement.SpeedNormalized,
                0.06f,
                Time.deltaTime);

            Vector3 localVelocity =
                transform.InverseTransformDirection(
                    movement.Velocity);

            float normalization =
                Mathf.Max(
                    0.01f,
                    movement.Speed);

            animator.SetFloat(
                MoveXHash,
                localVelocity.x / normalization,
                0.05f,
                Time.deltaTime);

            animator.SetFloat(
                MoveYHash,
                localVelocity.z / normalization,
                0.05f,
                Time.deltaTime);
        }

        private void HandleDodgeStarted(Vector3 direction) =>
            Trigger(DodgeHash);

        private void HandleThrowAnticipation() =>
            Trigger(ThrowAnticipationHash);

        private void HandleThrowReleased(Vector3 direction) =>
            Trigger(ThrowReleaseHash);

        private void HandleRecall() =>
            Trigger(RecallHash);

        private void HandleCatch() =>
            Trigger(CatchHash);

        private void HandleHit(float currentHealth, DamageInfo damage) =>
            Trigger(HitHash);

        private void Trigger(int hash)
        {
            if (animator != null)
            {
                animator.SetTrigger(hash);
            }
        }
    }
}
