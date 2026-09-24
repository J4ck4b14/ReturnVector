using UnityEngine;

namespace ReturnVector.Weapon
{
    /// <summary>
    /// Pure distance and speed helpers for outbound weapon simulation.
    /// </summary>
    public static class OutboundTravelMath
    {
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
