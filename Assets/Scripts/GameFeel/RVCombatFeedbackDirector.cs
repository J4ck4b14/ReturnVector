using ReturnVector.Player;
using ReturnVector.Weapon;
using UnityEngine;

// Script summary: Coordinates hit-stop, camera response and visual feedback from gameplay events.

namespace ReturnVector.GameFeel
{
    /// <summary>
    /// Coordinates hit-stop, camera response and visual feedback from gameplay events.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RVCombatFeedbackDirector : MonoBehaviour
    {
        // Weapon variables
        [SerializeField] private RVGameFeelProfile profile;
        [SerializeField] private RVHitStopController hitStop;
        [SerializeField] private RVCameraFeedback cameraFeedback;
        [SerializeField] private PlayerThrowController throwController;
        [SerializeField] private PlayerRecallController recallController;
        [SerializeField] private OutboundWeaponMotor outboundMotor;
        [SerializeField] private RecallWeaponMotor recallMotor;
        [SerializeField] private PlayerHealth playerHealth;

        // Feedback variables
        private bool bound;

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
        }

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
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

        /// <summary>
        /// Subscribes this presentation component to its gameplay events.
        /// </summary>
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

        /// <summary>
        /// Unsubscribes this presentation component from its gameplay events.
        /// </summary>
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

        /// <summary>
        /// Responds when the outbound throw is released.
        /// </summary>
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

        /// <summary>
        /// Responds when recall is requested.
        /// </summary>
        private void HandleRecallRequested()
        {
            cameraFeedback?.SetRecallActive(
                true);
        }

        // Outbound feedback stays lighter so recall impacts retain the stronger beat.
        /// <summary>
        /// Responds to an outbound weapon impact.
        /// </summary>
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
        /// <summary>
        /// Responds to a recall weapon impact.
        /// </summary>
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

        /// <summary>
        /// Responds when recall is blocked by geometry or an authored interaction.
        /// </summary>
        private void HandleRecallBlocked()
        {
            cameraFeedback?.SetRecallActive(
                false);
        }

        /// <summary>
        /// Responds when the baton enters its final catch movement.
        /// </summary>
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
        /// <summary>
        /// Responds when the baton catch completes.
        /// </summary>
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

        /// <summary>
        /// Responds when the player takes damage.
        /// </summary>
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
