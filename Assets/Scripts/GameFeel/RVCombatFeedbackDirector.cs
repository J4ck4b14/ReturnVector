using ReturnVector.Player;
using ReturnVector.Weapon;
using UnityEngine;

namespace ReturnVector.GameFeel
{
    /// <summary>
    /// Coordinates hit-stop, camera response and visual feedback from gameplay events.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RVCombatFeedbackDirector : MonoBehaviour
    {
        [SerializeField] private RVGameFeelProfile profile;
        [SerializeField] private RVHitStopController hitStop;
        [SerializeField] private RVCameraFeedback cameraFeedback;
        [SerializeField] private PlayerThrowController throwController;
        [SerializeField] private PlayerRecallController recallController;
        [SerializeField] private OutboundWeaponMotor outboundMotor;
        [SerializeField] private RecallWeaponMotor recallMotor;
        [SerializeField] private PlayerHealth playerHealth;

        private bool bound;

        private void OnEnable()
        {
            Bind();
        }

        private void OnDisable()
        {
            Unbind();
        }

        public void Configure(
            RVGameFeelProfile newProfile,
            RVHitStopController newHitStop,
            RVCameraFeedback newCameraFeedback,
            PlayerThrowController newThrowController,
            PlayerRecallController newRecallController,
            OutboundWeaponMotor newOutboundMotor,
            RecallWeaponMotor newRecallMotor,
            PlayerHealth newPlayerHealth)
        {
            Unbind();

            profile = newProfile;
            hitStop = newHitStop;
            cameraFeedback = newCameraFeedback;
            throwController = newThrowController;
            recallController = newRecallController;
            outboundMotor = newOutboundMotor;
            recallMotor = newRecallMotor;
            playerHealth = newPlayerHealth;

            if (isActiveAndEnabled)
            {
                Bind();
            }
        }

        private void Bind()
        {
            if (bound)
            {
                return;
            }

            if (throwController != null)
            {
                throwController.ThrowReleased +=
                    HandleThrowReleased;
            }

            if (recallController != null)
            {
                recallController.RecallRequested +=
                    HandleRecallRequested;
            }

            if (outboundMotor != null)
            {
                outboundMotor.Impacted +=
                    HandleOutboundImpact;
            }

            if (recallMotor != null)
            {
                recallMotor.Impacted +=
                    HandleRecallImpact;

                recallMotor.CatchStarted +=
                    HandleCatchStarted;

                recallMotor.CatchCompleted +=
                    HandleCatchCompleted;

                recallMotor.RecallBlocked +=
                    HandleRecallBlocked;
            }

            if (playerHealth != null)
            {
                playerHealth.Damaged +=
                    HandlePlayerDamaged;
            }

            bound = true;
        }

        private void Unbind()
        {
            if (!bound)
            {
                return;
            }

            if (throwController != null)
            {
                throwController.ThrowReleased -=
                    HandleThrowReleased;
            }

            if (recallController != null)
            {
                recallController.RecallRequested -=
                    HandleRecallRequested;
            }

            if (outboundMotor != null)
            {
                outboundMotor.Impacted -=
                    HandleOutboundImpact;
            }

            if (recallMotor != null)
            {
                recallMotor.Impacted -=
                    HandleRecallImpact;

                recallMotor.CatchStarted -=
                    HandleCatchStarted;

                recallMotor.CatchCompleted -=
                    HandleCatchCompleted;

                recallMotor.RecallBlocked -=
                    HandleRecallBlocked;
            }

            if (playerHealth != null)
            {
                playerHealth.Damaged -=
                    HandlePlayerDamaged;
            }

            bound = false;
        }

        private void HandleThrowReleased(
            Vector3 direction)
        {
            if (profile == null)
            {
                return;
            }

            cameraFeedback?.Impulse(
                profile.ThrowImpulse,
                0.09f,
                0f);
        }

        private void HandleRecallRequested()
        {
            cameraFeedback?.SetRecallActive(
                true);
        }

        // Outbound feedback stays lighter so recall impacts retain the stronger beat.
        private void HandleOutboundImpact(
            WeaponImpactInfo impact)
        {
            if (profile == null)
            {
                return;
            }

            float stop =
                impact.Blocking
                    ? profile.HeavyBlockHitStop
                    : impact.DamagedTarget
                        ? profile.OutboundHitStop
                        : 0f;

            if (stop > 0f)
            {
                hitStop?.Request(
                    stop,
                    profile.HitStopTimeScale);
            }

            float impulse =
                impact.Blocking
                    ? profile.OutboundImpactImpulse * 1.15f
                    : impact.DamagedTarget
                        ? profile.OutboundImpactImpulse
                        : profile.OutboundImpactImpulse * 0.45f;

            cameraFeedback?.Impulse(
                impulse,
                profile.ImpactImpulseSeconds,
                impact.Blocking
                    ? profile.ImpactZoomPunch
                    : profile.ImpactZoomPunch * 0.65f);

            cameraFeedback?.DirectionalImpulse(
                impact.Normal.sqrMagnitude > 0.0001f
                    ? impact.Normal
                    : Vector3.back,
                impulse * 0.45f,
                profile.ImpactImpulseSeconds,
                0f);
        }

        // Recall carries the strongest directional hit response in the normal weapon cycle.
        private void HandleRecallImpact(
            WeaponImpactInfo impact)
        {
            if (profile == null)
            {
                return;
            }

            if (impact.DamagedTarget ||
                impact.Blocking)
            {
                hitStop?.Request(
                    impact.Blocking
                        ? profile.HeavyBlockHitStop
                        : profile.RecallHitStop,
                    profile.HitStopTimeScale);
            }

            float recallImpulse =
                impact.DamagedTarget
                    ? profile.RecallImpactImpulse
                    : profile.RecallImpactImpulse * 0.55f;

            cameraFeedback?.Impulse(
                recallImpulse,
                profile.ImpactImpulseSeconds,
                impact.DamagedTarget
                    ? profile.ImpactZoomPunch
                    : profile.ImpactZoomPunch * 0.5f);

            cameraFeedback?.DirectionalImpulse(
                impact.Normal.sqrMagnitude > 0.0001f
                    ? impact.Normal
                    : Vector3.back,
                recallImpulse * 0.52f,
                profile.ImpactImpulseSeconds,
                0f);
        }

        private void HandleRecallBlocked()
        {
            cameraFeedback?.SetRecallActive(
                false);
        }

        private void HandleCatchStarted()
        {
            if (profile == null)
            {
                return;
            }

            cameraFeedback?.Impulse(
                profile.CatchImpulse * 0.55f,
                profile.CatchImpulseSeconds,
                profile.CatchZoomPunch * 0.55f);
        }

        // Catch closes the attack cycle with a short camera and timing accent.
        private void HandleCatchCompleted()
        {
            if (profile == null)
            {
                return;
            }

            hitStop?.Request(
                profile.CatchHitStop,
                profile.HitStopTimeScale);

            cameraFeedback?.SetRecallActive(
                false);

            cameraFeedback?.Impulse(
                profile.CatchImpulse,
                profile.CatchImpulseSeconds,
                profile.CatchZoomPunch);
        }

        private void HandlePlayerDamaged(
            float health,
            ReturnVector.Combat.DamageInfo damage)
        {
            if (profile == null)
            {
                return;
            }

            cameraFeedback?.Impulse(
                profile.PlayerDamageImpulse,
                profile.ImpactImpulseSeconds * 1.35f,
                profile.ImpactZoomPunch * 0.7f);

            cameraFeedback?.DirectionalImpulse(
                damage.Direction.sqrMagnitude > 0.0001f
                    ? -damage.Direction
                    : Vector3.back,
                profile.PlayerDamageImpulse * 0.55f,
                profile.ImpactImpulseSeconds * 1.35f,
                0f);
        }
    }
}
