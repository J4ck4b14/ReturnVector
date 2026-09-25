using ReturnVector.Weapon;
using UnityEngine;

// Script summary: Applies presentation-only compression, flash and catch tension to the weapon visual.

namespace ReturnVector.GameFeel
{
    /// <summary>
    /// Applies presentation-only compression, flash and catch tension to the weapon visual.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WeaponFeedbackVisual : MonoBehaviour
    {
        // Feedback variables
        [SerializeField] private Transform visualRoot;
        [SerializeField] private RVRendererFlash flash;
        [SerializeField] private OutboundWeaponMotor outboundMotor;
        [SerializeField] private RecallWeaponMotor recallMotor;
        [SerializeField] private RVGameFeelProfile profile;

        // Weapon variables
        private Vector3 baseScale;
        private float impactKick;
        private float catchTension;

        /// <summary>
        /// Caches required references and prepares runtime state before the object starts running.
        /// </summary>
        private void Awake()
        {
            if (visualRoot != null)
            {
                baseScale =
                    visualRoot.localScale;
            }
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

            if (visualRoot != null)
            {
                visualRoot.localScale =
                    baseScale;
            }
        }

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(
            Transform newVisualRoot,
            RVRendererFlash newFlash,
            OutboundWeaponMotor newOutboundMotor,
            RecallWeaponMotor newRecallMotor,
            RVGameFeelProfile newProfile)
        {
            Unbind();

            visualRoot = newVisualRoot;
            flash = newFlash;
            outboundMotor = newOutboundMotor;
            recallMotor = newRecallMotor;
            profile = newProfile;

            if (visualRoot != null)
            {
                baseScale =
                    visualRoot.localScale;
            }

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
            }
        }

        /// <summary>
        /// Unsubscribes this presentation component from its gameplay events.
        /// </summary>
        private void Unbind()
        {
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

            impactKick =
                Mathf.MoveTowards(
                    impactKick,
                    0f,
                    dt * 8f);

            catchTension =
                Mathf.MoveTowards(
                    catchTension,
                    0f,
                    dt * 9f);

            Vector3 target =
                Vector3.Scale(
                    baseScale,
                    new Vector3(
                        1f + impactKick * 0.18f +
                        catchTension * 0.12f,
                        1f - impactKick * 0.12f -
                        catchTension * 0.08f,
                        1f + catchTension * 0.22f));

            float blend =
                1f -
                Mathf.Exp(
                    -22f *
                    dt);

            visualRoot.localScale =
                Vector3.Lerp(
                    visualRoot.localScale,
                    target,
                    blend);
        }

        /// <summary>
        /// Responds to an outbound weapon impact.
        /// </summary>
        private void HandleOutboundImpact(
            WeaponImpactInfo impact)
        {
            impactKick = 1f;

            flash?.Flash(
                profile != null
                    ? profile.WeaponFlashSeconds
                    : 0.05f,
                new Color(
                    1f,
                    0.72f,
                    0.42f,
                    1f));
        }

        /// <summary>
        /// Responds to a recall weapon impact.
        /// </summary>
        private void HandleRecallImpact(
            WeaponImpactInfo impact)
        {
            impactKick = 1f;

            flash?.Flash(
                profile != null
                    ? profile.WeaponFlashSeconds
                    : 0.05f,
                new Color(
                    0.45f,
                    0.95f,
                    1f,
                    1f));
        }

        /// <summary>
        /// Responds when the baton enters its final catch movement.
        /// </summary>
        private void HandleCatchStarted()
        {
            catchTension = 1f;
        }

        /// <summary>
        /// Responds when the baton catch completes.
        /// </summary>
        private void HandleCatchCompleted()
        {
            catchTension = 0.7f;

            flash?.Flash(
                profile != null
                    ? profile.WeaponFlashSeconds
                    : 0.05f,
                Color.white);
        }
    }
}
