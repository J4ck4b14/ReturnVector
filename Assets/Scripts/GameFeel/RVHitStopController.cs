using UnityEngine;

namespace ReturnVector.GameFeel
{
    /// <summary>
    /// Owns brief global time-scale punches used on high-value impacts.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RVHitStopController : MonoBehaviour
    {
        private float remaining;
        private float requestedScale = 1f;
        private float baseFixedDeltaTime;
        private bool active;

        public bool IsActive => active;
        public float Remaining => remaining;

        private void Awake()
        {
            baseFixedDeltaTime =
                Mathf.Max(0.0001f, Time.fixedDeltaTime);
        }

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

        private void OnDisable()
        {
            RestoreTime();
        }

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
