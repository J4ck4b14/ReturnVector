using ReturnVector.Player;
using ReturnVector.Weapon;
using UnityEngine;

// Script summary: Adds lightweight pose offsets for movement, throw, dodge and catch beats.

namespace ReturnVector.GameFeel
{
    /// <summary>
    /// Adds lightweight pose offsets for movement, throw, dodge and catch beats.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerProceduralPose : MonoBehaviour
    {
        // Feedback variables
        [SerializeField] private Transform visualRoot;
        [SerializeField] private PlayerMov movement;
        [SerializeField] private PlayerCombatController combat;
        [SerializeField] private PlayerThrowController throwController;
        [SerializeField] private RecallWeaponMotor recallMotor;
        [SerializeField] private RVGameFeelProfile profile;

        private Vector3 basePosition;
        private Quaternion baseRotation;
        private Vector3 baseScale;

        private float releaseKick;
        private float catchKick;

        /// <summary>
        /// Caches required references and prepares runtime state before the object starts running.
        /// </summary>
        private void Awake()
        {
            CaptureBasePose();
        }

        /// <summary>
        /// Subscribes to runtime events when the component becomes active.
        /// </summary>
        private void OnEnable()
        {
            Bind();
        }

        /// <summary>
        /// Unsubscribes from runtime events when the component is disabled.
        /// </summary>
        private void OnDisable()
        {
            Unbind();
            ResetPose();
        }

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(
            Transform newVisualRoot,
            PlayerMov newMovement,
            PlayerCombatController newCombat,
            PlayerThrowController newThrowController,
            RecallWeaponMotor newRecallMotor,
            RVGameFeelProfile newProfile)
        {
            Unbind();

            visualRoot = newVisualRoot;
            movement = newMovement;
            combat = newCombat;
            throwController = newThrowController;
            recallMotor = newRecallMotor;
            profile = newProfile;

            CaptureBasePose();

            if (isActiveAndEnabled)
            {
                Bind();
            }
        }

        /// <summary>
        /// Subscribes this presentation component to its gameplay events.
        /// </summary>
        private void Bind()
        {
            if (throwController != null)
            {
                throwController.ThrowReleased +=
                    HandleThrowReleased;
            }

            if (recallMotor != null)
            {
                recallMotor.CatchCompleted +=
                    HandleCatchCompleted;
            }
        }

        /// <summary>
        /// Unsubscribes this presentation component from its gameplay events.
        /// </summary>
        private void Unbind()
        {
            if (throwController != null)
            {
                throwController.ThrowReleased -=
                    HandleThrowReleased;
            }

            if (recallMotor != null)
            {
                recallMotor.CatchCompleted -=
                    HandleCatchCompleted;
            }
        }

        /// <summary>
        /// Advances the component for the current frame.
        /// </summary>
        private void Update()
        {
            if (visualRoot == null ||
                profile == null)
            {
                return;
            }

            float dt =
                Time.unscaledDeltaTime;

            releaseKick =
                Mathf.MoveTowards(
                    releaseKick,
                    0f,
                    dt * 6.5f);

            catchKick =
                Mathf.MoveTowards(
                    catchKick,
                    0f,
                    dt * 7.5f);

            Vector3 localVelocity =
                movement != null
                    ? transform.InverseTransformDirection(
                        movement.Velocity)
                    : Vector3.zero;

            float normalizedSpeed =
                movement != null
                    ? movement.SpeedNormalized
                    : 0f;

            float sideLean =
                Mathf.Clamp(
                    -localVelocity.x * 1.2f,
                    -8f,
                    8f);

            float forwardLean =
                Mathf.Lerp(
                    0f,
                    6f,
                    normalizedSpeed);

            bool unarmed =
                combat != null &&
                combat.Mode ==
                PlayerCombatMode.Unarmed;

            if (unarmed)
            {
                forwardLean += 4.5f;
            }

            bool anticipating =
                throwController != null &&
                throwController.IsAnticipating;

            float anticipation =
                anticipating
                    ? 1f
                    : 0f;

            float dodge =
                movement != null &&
                movement.IsDodging
                    ? 1f
                    : 0f;

            Vector3 targetPosition =
                basePosition +
                new Vector3(
                    sideLean * 0.003f,
                    -dodge * 0.08f,
                    -anticipation * 0.12f +
                    releaseKick * 0.10f -
                    catchKick * 0.07f);

            Quaternion targetRotation =
                baseRotation *
                Quaternion.Euler(
                    forwardLean -
                    anticipation * 8f +
                    releaseKick * 11f -
                    catchKick * 8f,
                    0f,
                    sideLean);

            Vector3 targetScale =
                Vector3.Scale(
                    baseScale,
                    new Vector3(
                        1f + dodge * 0.12f,
                        1f - dodge * 0.16f,
                        1f + dodge * 0.12f));

            float blend =
                1f -
                Mathf.Exp(
                    -profile.PoseSharpness *
                    dt);

            visualRoot.localPosition =
                Vector3.Lerp(
                    visualRoot.localPosition,
                    targetPosition,
                    blend);

            visualRoot.localRotation =
                Quaternion.Slerp(
                    visualRoot.localRotation,
                    targetRotation,
                    blend);

            visualRoot.localScale =
                Vector3.Lerp(
                    visualRoot.localScale,
                    targetScale,
                    blend);
        }

        /// <summary>
        /// Responds when the outbound throw is released.
        /// </summary>
        private void HandleThrowReleased(
            Vector3 direction)
        {
            releaseKick = 1f;
        }

        /// <summary>
        /// Responds when the baton catch completes.
        /// </summary>
        private void HandleCatchCompleted()
        {
            catchKick = 1f;
        }

        /// <summary>
        /// Captures the base pose.
        /// </summary>
        private void CaptureBasePose()
        {
            if (visualRoot == null)
            {
                return;
            }

            basePosition =
                visualRoot.localPosition;

            baseRotation =
                visualRoot.localRotation;

            baseScale =
                visualRoot.localScale;
        }

        /// <summary>
        /// Resets the pose.
        /// </summary>
        private void ResetPose()
        {
            if (visualRoot == null)
            {
                return;
            }

            visualRoot.localPosition =
                basePosition;

            visualRoot.localRotation =
                baseRotation;

            visualRoot.localScale =
                baseScale;
        }
    }
}
