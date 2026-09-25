using UnityEngine;

// Script summary: Owns brief global time-scale punches used on high-value impacts.

namespace ReturnVector.GameFeel
{
    /// <summary>
    /// Owns brief global time-scale punches used on high-value impacts.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RVHitStopController : MonoBehaviour
    {
        // Feedback variables
        private float remaining;
        private float requestedScale = 1f;
        private float baseFixedDeltaTime;
        private bool active;

        public bool IsActive => active;
        public float Remaining => remaining;

        /// <summary>
        /// Caches required references and prepares runtime state before the object starts running.
        /// </summary>
        private void Awake()
        {
            baseFixedDeltaTime =
                Mathf.Max(0.0001f, Time.fixedDeltaTime);
        }

        /// <summary>
        /// Advances the component for the current frame.
        /// </summary>
        private void Update()
        {
            if (!active)
            {
                return;
            }

            remaining -= Time.unscaledDeltaTime;

            if (remaining <= 0f)
            {
                RestoreTime();
            }
        }

        /// <summary>
        /// Unsubscribes from runtime events when the component is disabled.
        /// </summary>
        private void OnDisable()
        {
            RestoreTime();
        }

        /// <summary>
        /// Requests a short global hit-stop effect.
        /// </summary>
        public void Request(
            float duration,
            float timeScale)
        {
            if (duration <= 0f)
            {
                return;
            }

            float scale =
                Mathf.Clamp(
                    timeScale,
                    0.01f,
                    1f);

            remaining =
                Mathf.Max(
                    remaining,
                    duration);

            requestedScale =
                active
                    ? Mathf.Min(
                        requestedScale,
                        scale)
                    : scale;

            active = true;

            Time.timeScale =
                requestedScale;

            Time.fixedDeltaTime =
                baseFixedDeltaTime *
                requestedScale;
        }

        /// <summary>
        /// Restores the normal time scale after hit stop.
        /// </summary>
        private void RestoreTime()
        {
            if (!active)
            {
                return;
            }

            active = false;
            remaining = 0f;
            requestedScale = 1f;

            Time.timeScale = 1f;
            Time.fixedDeltaTime =
                baseFixedDeltaTime;
        }
    }
}
