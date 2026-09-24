using UnityEngine;

namespace ReturnVector.Debugging
{
    /// <summary>
    /// Small wrapper for development-only world debug drawing.
    /// </summary>
    public static class RVDebugDraw
    {
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
