using ReturnVector.Player;
using ReturnVector.Weapon;
using UnityEngine;

namespace ReturnVector.GameFeel
{
    /// <summary>
    /// Adds lightweight pose offsets for movement, throw, dodge and catch beats.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerProceduralPose : MonoBehaviour
    {
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

        private void Awake()
        {
            CaptureBasePose();
        }

        private void OnEnable()
        {
            Bind();
        }

        private void OnDisable()
        {
            Unbind();
            ResetPose();
        }

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

        private void HandleThrowReleased(
            Vector3 direction)
        {
            releaseKick = 1f;
        }

        private void HandleCatchCompleted()
        {
            catchKick = 1f;
        }

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
