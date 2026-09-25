using UnityEngine;

// Script summary: Small wrapper for development-only world debug drawing.

namespace ReturnVector.Debugging
{
    /// <summary>
    /// Small wrapper for development-only world debug drawing.
    /// </summary>
    public static class RVDebugDraw
    {
        /// <summary>
        /// Draws a development debug line when the corresponding setting is enabled.
        /// </summary>
        public static void Line(
            Vector3 from,
            Vector3 to,
            Color color,
            float duration = 0f)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.DrawLine(from, to, color, duration);
#endif
        }

        /// <summary>
        /// Draws a development surface normal when the corresponding setting is enabled.
        /// </summary>
        public static void Normal(
            Vector3 point,
            Vector3 normal,
            float length,
            Color color,
            float duration = 0f)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UnityEngine.Debug.DrawRay(point, normal.normalized * length, color, duration);
#endif
        }
    }
}
