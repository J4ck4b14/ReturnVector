using UnityEngine;

namespace ReturnVector.Enemies
{
    /// <summary>
    /// Geometry helpers for swept disruption-projectile checks.
    /// </summary>
    public static class EnemyProjectileMath
    {
        public static float DistancePointToSegment(
            Vector3 point,
            Vector3 segmentStart,
            Vector3 segmentEnd)
        {
            Vector3 segment =
                segmentEnd - segmentStart;

            float lengthSquared =
                segment.sqrMagnitude;

            if (lengthSquared <= 0.000001f)
            {
                return Vector3.Distance(
                    point,
                    segmentStart);
            }

            float t =
                Vector3.Dot(
                    point - segmentStart,
                    segment) /
                lengthSquared;

            t = Mathf.Clamp01(t);

            Vector3 closest =
                segmentStart + segment * t;

            return Vector3.Distance(
                point,
                closest);
        }
    }
}
