using UnityEngine;

namespace ReturnVector.Player
{
    /// <summary>
    /// Pure movement and dodge calculations used by PlayerMov.
    /// </summary>
    public static class PlayerMovementMath
    {
        public static Vector3 GetWorldMove(Vector2 input, float deadzone)
        {
            float sqrDeadzone = deadzone * deadzone;
            if (input.sqrMagnitude <= sqrDeadzone)
            {
                return Vector3.zero;
            }

            Vector2 clamped = Vector2.ClampMagnitude(input, 1f);
            return new Vector3(clamped.x, 0f, clamped.y);
        }

        public static Vector3 StepVelocity(
            Vector3 current,
            Vector3 desired,
            float acceleration,
            float deceleration,
            float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return current;
            }

            float rate = desired.sqrMagnitude > 0.0001f
                ? acceleration
                : deceleration;

            return Vector3.MoveTowards(
                current,
                desired,
                Mathf.Max(0f, rate) * deltaTime);
        }

        public static Vector3 ChooseDodgeDirection(
            Vector2 moveInput,
            float moveDeadzone,
            Vector3 aimDirection,
            Vector3 facingDirection)
        {
            Vector3 move = GetWorldMove(moveInput, moveDeadzone);
            if (move.sqrMagnitude > 0.0001f)
            {
                return move.normalized;
            }

            aimDirection.y = 0f;
            if (aimDirection.sqrMagnitude > 0.0001f)
            {
                return aimDirection.normalized;
            }

            facingDirection.y = 0f;
            if (facingDirection.sqrMagnitude > 0.0001f)
            {
                return facingDirection.normalized;
            }

            return Vector3.forward;
        }

        public static float DodgeDistanceDelta(
            AnimationCurve curve,
            float previousNormalizedTime,
            float nextNormalizedTime,
            float totalDistance)
        {
            float from = Mathf.Clamp01(previousNormalizedTime);
            float to = Mathf.Clamp01(nextNormalizedTime);

            if (to <= from || totalDistance <= 0f)
            {
                return 0f;
            }

            float previous = curve != null ? curve.Evaluate(from) : from;
            float next = curve != null ? curve.Evaluate(to) : to;
            return Mathf.Max(0f, next - previous) * totalDistance;
        }
    }
}
