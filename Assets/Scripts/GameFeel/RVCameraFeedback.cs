using ReturnVector.Encounters;
using UnityEngine;

namespace ReturnVector.GameFeel
{
    /// <summary>
    /// Accumulates short camera impulses and framing changes from combat events.
    /// </summary>
    [DefaultExecutionOrder(-80)]
    [DisallowMultipleComponent]
    public sealed class RVCameraFeedback : MonoBehaviour
    {
        [SerializeField] private TopDownCameraFollow follow;
        [SerializeField] private RVGameFeelProfile profile;

        private float impulseRemaining;
        private float impulseDuration;
        private float impulseAmplitude;

        private float directionalRemaining;
        private float directionalDuration;
        private float directionalAmplitude;
        private Vector3 directionalVector;

        private float zoomPunch;

        private bool recallActive;
        private float recallBlend;
        private float smoothedZoom;

        public void Configure(
            TopDownCameraFollow newFollow,
            RVGameFeelProfile newProfile)
        {
            follow = newFollow;
            profile = newProfile;
        }

        public void SetRecallActive(bool active)
        {
            recallActive = active;
        }

        public void SetArenaFraming(
            float orthographicSize,
            bool immediate = false)
        {
            follow?.SetArenaFraming(
                orthographicSize,
                immediate);
        }

        public void Impulse(
            float amplitude,
            float duration,
            float zoomAmount = 0f)
        {
            impulseAmplitude =
                Mathf.Max(
                    impulseAmplitude,
                    Mathf.Max(0f, amplitude));

            impulseDuration =
                Mathf.Max(
                    impulseDuration,
                    Mathf.Max(0.01f, duration));

            impulseRemaining =
                Mathf.Max(
                    impulseRemaining,
                    impulseDuration);

            zoomPunch =
                Mathf.Max(
                    zoomPunch,
                    Mathf.Max(0f, zoomAmount));
        }

        public void DirectionalImpulse(
            Vector3 worldDirection,
            float amplitude,
            float duration,
            float zoomAmount = 0f)
        {
            worldDirection.y = 0f;

            if (worldDirection.sqrMagnitude <
                0.0001f)
            {
                Impulse(
                    amplitude,
                    duration,
                    zoomAmount);
                return;
            }

            directionalVector =
                worldDirection.normalized;

            directionalAmplitude =
                Mathf.Max(
                    directionalAmplitude,
                    Mathf.Max(0f, amplitude));

            directionalDuration =
                Mathf.Max(
                    directionalDuration,
                    Mathf.Max(0.01f, duration));

            directionalRemaining =
                Mathf.Max(
                    directionalRemaining,
                    directionalDuration);

            zoomPunch =
                Mathf.Max(
                    zoomPunch,
                    Mathf.Max(0f, zoomAmount));
        }

        private void Update()
        {
            if (follow == null ||
                profile == null)
            {
                return;
            }

            float dt =
                Time.unscaledDeltaTime;

            float recallTarget =
                recallActive ? 1f : 0f;

            float recallLerp =
                RVGameFeelMath.ExponentialBlend(
                    profile.RecallBlendSharpness,
                    dt);

            recallBlend =
                Mathf.Lerp(
                    recallBlend,
                    recallTarget,
                    recallLerp);

            Vector3 offset =
                Vector3.zero;

            if (impulseRemaining > 0f)
            {
                impulseRemaining =
                    Mathf.Max(
                        0f,
                        impulseRemaining - dt);

                float envelope =
                    RVGameFeelMath.ImpactEnvelope(
                        impulseRemaining,
                        impulseDuration);

                float t =
                    Time.unscaledTime;

                float x =
                    Mathf.Sin(t * 83.1f) *
                    Mathf.Cos(t * 31.7f);

                float z =
                    Mathf.Sin(
                        t * 67.3f + 1.7f) *
                    Mathf.Cos(t * 29.9f);

                offset +=
                    new Vector3(
                        x,
                        0f,
                        z) *
                    impulseAmplitude *
                    envelope;

                if (impulseRemaining <= 0f)
                {
                    impulseAmplitude = 0f;
                    impulseDuration = 0f;
                }
            }

            if (directionalRemaining > 0f)
            {
                directionalRemaining =
                    Mathf.Max(
                        0f,
                        directionalRemaining - dt);

                float envelope =
                    RVGameFeelMath.ImpactEnvelope(
                        directionalRemaining,
                        directionalDuration);

                offset +=
                    directionalVector *
                    directionalAmplitude *
                    envelope;

                if (directionalRemaining <= 0f)
                {
                    directionalAmplitude = 0f;
                    directionalDuration = 0f;
                    directionalVector = Vector3.zero;
                }
            }

            float zoomTarget =
                -profile.RecallZoomIn *
                recallBlend -
                zoomPunch;

            float zoomLerp =
                RVGameFeelMath.ExponentialBlend(
                    profile.ZoomSharpness,
                    dt);

            smoothedZoom =
                Mathf.Lerp(
                    smoothedZoom,
                    zoomTarget,
                    zoomLerp);

            zoomPunch =
                Mathf.MoveTowards(
                    zoomPunch,
                    0f,
                    dt * 2.5f);

            follow.ApplyFeedback(
                offset,
                smoothedZoom);
        }

        private void OnDisable()
        {
            if (follow != null)
            {
                follow.ApplyFeedback(
                    Vector3.zero,
                    0f);
            }
        }
    }
}
