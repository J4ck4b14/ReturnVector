using UnityEngine;

// Script summary: Pure distance and speed helpers for outbound weapon simulation.

namespace ReturnVector.Weapon
{
    /// <summary>
    /// Pure distance and speed helpers for outbound weapon simulation.
    /// </summary>
    public static class OutboundTravelMath
    {
        /// <summary>
        /// Returns a movement distance clamped to the remaining travel distance.
        /// </summary>
        public static float StepDistance(
            float speed,
            float deltaTime,
            float remainingDistance)
        {
            if (speed <= 0f ||
                deltaTime <= 0f ||
                remainingDistance <= 0f)
            {
                return 0f;
            }

            return Mathf.Min(
                speed * deltaTime,
                remainingDistance);
        }
    }
}
