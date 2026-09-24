using UnityEngine;

namespace ReturnVector.GameFeel
{
    /// <summary>
    /// Pure easing and damping helpers used by presentation systems.
    /// </summary>
    public static class RVGameFeelMath
    {
        public static float ExponentialBlend(
            float sharpness,
            float unscaledDeltaTime)
        {
            if (sharpness <= 0f ||
                unscaledDeltaTime <= 0f)
            {
                return 0f;
            }

            return
                1f -
                Mathf.Exp(
                    -sharpness *
                    unscaledDeltaTime);
        }

        public static float ImpactEnvelope(
            float remaining,
            float duration)
        {
            if (duration <= 0f ||
                remaining <= 0f)
            {
                return 0f;
            }

            float normalized =
                Mathf.Clamp01(
                    remaining /
                    duration);

            return normalized *
                   normalized;
        }
    }
}
