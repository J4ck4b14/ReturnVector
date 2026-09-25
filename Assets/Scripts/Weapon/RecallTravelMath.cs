using UnityEngine;

// Script summary: Pure steering, acceleration and catch helpers for recall travel.

namespace ReturnVector.Weapon
{
    /// <summary>
    /// Pure steering, acceleration and catch helpers for recall travel.
    /// </summary>
    public static class RecallTravelMath
    {
        /// <summary>
        /// Moves recall speed toward its configured maximum without overshooting it.
        /// </summary>
        public static float Accelerate(
            float currentSpeed,
            float maxSpeed,
            float acceleration,
            float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return Mathf.Clamp(currentSpeed, 0f, Mathf.Max(0f, maxSpeed));
            }

            return Mathf.MoveTowards(
                Mathf.Max(0f, currentSpeed),
                Mathf.Max(0f, maxSpeed),
                Mathf.Max(0f, acceleration) * deltaTime);
        }

        /// <summary>
        /// Rotates the current recall direction toward the desired direction.
        /// </summary>
        public static Vector3 Steer(
            Vector3 currentDirection,
            Vector3 desiredDirection,
            float maxDegreesPerSecond,
            float deltaTime)
        {
            Vector3 current = FlattenAndNormalize(currentDirection);
            Vector3 desired = FlattenAndNormalize(desiredDirection);

            if (desired.sqrMagnitude < 0.000001f)
            {
                return current;
            }

            if (current.sqrMagnitude < 0.000001f)
            {
                return desired;
            }

            float radians =
                Mathf.Max(0f, maxDegreesPerSecond) *
                Mathf.Deg2Rad *
                Mathf.Max(0f, deltaTime);

            Vector3 steered = Vector3.RotateTowards(
                current,
                desired,
                radians,
                0f);

            return FlattenAndNormalize(steered);
        }

        /// <summary>
        /// Returns a movement distance clamped to the remaining travel distance.
        /// </summary>
        public static float StepDistance(
            float speed,
            float deltaTime,
            float distanceToTarget)
        {
            if (speed <= 0f || deltaTime <= 0f || distanceToTarget <= 0f)
            {
                return 0f;
            }

            return Mathf.Min(speed * deltaTime, distanceToTarget);
        }

        /// <summary>
        /// Returns the recall turn rate for the current catch distance.
        /// </summary>
        public static float TurnRateForDistance(
            float baseDegreesPerSecond,
            float distanceToTarget,
            float nearCatchDistance,
            float nearCatchMultiplier)
        {
            if (nearCatchDistance <= 0f || distanceToTarget >= nearCatchDistance)
            {
                return Mathf.Max(0f, baseDegreesPerSecond);
            }

            float proximity = 1f - Mathf.Clamp01(distanceToTarget / nearCatchDistance);
            float multiplier = Mathf.Lerp(1f, Mathf.Max(1f, nearCatchMultiplier), proximity);
            return Mathf.Max(0f, baseDegreesPerSecond) * multiplier;
        }

        /// <summary>
        /// Returns the normalized easing value used during the final catch movement.
        /// </summary>
        public static float CatchEase(float normalizedTime)
        {
            float t = Mathf.Clamp01(normalizedTime);
            float inverse = 1f - t;
            return 1f - inverse * inverse * inverse;
        }

        /// <summary>
        /// Flattens a direction onto the gameplay plane and normalizes it safely.
        /// </summary>
        private static Vector3 FlattenAndNormalize(Vector3 value)
        {
            value.y = 0f;
            if (value.sqrMagnitude < 0.000001f)
            {
                return Vector3.zero;
            }

            return value.normalized;
        }
    }
}
