using UnityEngine;

// Script summary: Smooth top-down follow camera with additive combat feedback offsets.

namespace ReturnVector.Encounters
{
    /// <summary>
    /// Smooth top-down follow camera with additive combat feedback offsets.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TopDownCameraFollow : MonoBehaviour
    {
        // Encounter variables
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 worldOffset =
            new Vector3(0f, 13f, -7f);
        [SerializeField, Min(0f)] private float followSharpness = 14f;

        private Vector3 feedbackOffset;
        private float feedbackZoom;
        private Camera attachedCamera;
        private float baseOrthographicSize;
        private float targetOrthographicSize;

        /// <summary>
        /// Caches required references and prepares runtime state before the object starts running.
        /// </summary>
        private void Awake()
        {
            attachedCamera =
                GetComponent<Camera>();

            if (attachedCamera != null)
            {
                baseOrthographicSize =
                    attachedCamera.orthographicSize;

                targetOrthographicSize =
                    baseOrthographicSize;
            }
        }

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(
            Transform newTarget,
            Vector3 offset,
            float sharpness = 14f)
        {
            target = newTarget;
            worldOffset = offset;
            followSharpness = Mathf.Max(0f, sharpness);

            attachedCamera =
                GetComponent<Camera>();

            if (attachedCamera != null)
            {
                baseOrthographicSize =
                    attachedCamera.orthographicSize;
                targetOrthographicSize = baseOrthographicSize;
            }

            if (target != null)
            {
                transform.position =
                    target.position + worldOffset;
            }
        }

        /// <summary>
        /// Applies the feedback.
        /// </summary>
        public void ApplyFeedback(
            Vector3 positionOffset,
            float orthographicSizeOffset)
        {
            feedbackOffset =
                positionOffset;

            feedbackZoom =
                orthographicSizeOffset;
        }

        /// <summary>
        /// Sets the arena framing.
        /// </summary>
        public void SetArenaFraming(
            float orthographicSize,
            bool immediate = false)
        {
            targetOrthographicSize =
                Mathf.Max(0.1f, orthographicSize);

            if (immediate)
            {
                baseOrthographicSize =
                    targetOrthographicSize;
            }
        }

        /// <summary>
        /// Updates presentation after regular frame logic has completed.
        /// </summary>
        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desired =
                target.position +
                worldOffset +
                feedbackOffset;

            float framingBlend =
                1f -
                Mathf.Exp(
                    -6f *
                    Time.unscaledDeltaTime);

            baseOrthographicSize =
                Mathf.Lerp(
                    baseOrthographicSize,
                    targetOrthographicSize,
                    framingBlend);

            if (followSharpness <= 0f)
            {
                transform.position = desired;

                if (attachedCamera != null &&
                    attachedCamera.orthographic)
                {
                    attachedCamera.orthographicSize =
                        Mathf.Max(
                            0.1f,
                            baseOrthographicSize +
                            feedbackZoom);
                }

                return;
            }

            float blend =
                1f -
                Mathf.Exp(
                    -followSharpness *
                    Time.deltaTime);

            transform.position =
                Vector3.Lerp(
                    transform.position,
                    desired,
                    blend);

            if (attachedCamera != null &&
                attachedCamera.orthographic)
            {
                attachedCamera.orthographicSize =
                    Mathf.Max(
                        0.1f,
                        baseOrthographicSize +
                        feedbackZoom);
            }
        }
    }
}
