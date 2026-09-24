using ReturnVector.Weapon;
using UnityEngine;

namespace ReturnVector.GameFeel
{
    /// <summary>
    /// Applies presentation-only compression, flash and catch tension to the weapon visual.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WeaponFeedbackVisual : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private RVRendererFlash flash;
        [SerializeField] private OutboundWeaponMotor outboundMotor;
        [SerializeField] private RecallWeaponMotor recallMotor;
        [SerializeField] private RVGameFeelProfile profile;

        private Vector3 baseScale;
        private float impactKick;
        private float catchTension;

        private void Awake()
        {
            if (visualRoot != null)
            {
                baseScale =
                    visualRoot.localScale;
            }
        }

        private void OnEnable()
        {
            Bind();
        }

        private void OnDisable()
        {
            Unbind();

            if (visualRoot != null)
            {
                visualRoot.localScale =
                    baseScale;
            }
        }

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

        private void HandleCatchStarted()
        {
            catchTension = 1f;
        }

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
