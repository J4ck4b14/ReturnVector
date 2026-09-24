using UnityEngine;

namespace ReturnVector.Enemies
{
    /// <summary>
    /// Planar enemy locomotion shared by the prototype AI behaviours.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyMotor : MonoBehaviour
    {
        [SerializeField] private CharacterController controller;
        [SerializeField, Min(0f)] private float turnDegreesPerSecond = 720f;

        public Vector3 Velocity { get; private set; }
        public float Speed => Velocity.magnitude;

        public void Configure(
            CharacterController newController,
            float turnRate = 720f)
        {
            controller = newController;
            turnDegreesPerSecond = Mathf.Max(0f, turnRate);
        }

        public void MoveToward(
            Vector3 worldTarget,
            float speed,
            float deltaTime,
            float stoppingDistance = 0f)
        {
            Vector3 toTarget =
                worldTarget - transform.position;
            toTarget.y = 0f;

            float distance = toTarget.magnitude;
            if (distance <= Mathf.Max(0f, stoppingDistance) ||
                speed <= 0f)
            {
                Velocity = Vector3.zero;
                FaceDirection(
                    distance > 0.0001f
                        ? toTarget.normalized
                        : transform.forward,
                    deltaTime);
                return;
            }

            Vector3 direction = toTarget / distance;
            float travel =
                Mathf.Min(
                    speed * Mathf.Max(0f, deltaTime),
                    Mathf.Max(
                        0f,
                        distance - stoppingDistance));

            Velocity =
                deltaTime > 0f
                    ? direction * (travel / deltaTime)
                    : Vector3.zero;

            if (controller != null &&
                controller.enabled)
            {
                controller.Move(direction * travel);
            }
            else
            {
                transform.position += direction * travel;
            }

            FaceDirection(direction, deltaTime);
        }

        public void MoveDirection(
            Vector3 worldDirection,
            float speed,
            float deltaTime)
        {
            Vector3 direction = worldDirection;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f ||
                speed <= 0f)
            {
                Velocity = Vector3.zero;
                return;
            }

            direction.Normalize();
            Vector3 motion =
                direction *
                speed *
                Mathf.Max(0f, deltaTime);

            Velocity =
                deltaTime > 0f
                    ? motion / deltaTime
                    : Vector3.zero;

            if (controller != null &&
                controller.enabled)
            {
                controller.Move(motion);
            }
            else
            {
                transform.position += motion;
            }

            FaceDirection(direction, deltaTime);
        }

        public void Stop()
        {
            Velocity = Vector3.zero;
        }

        public void FaceTarget(
            Vector3 worldTarget,
            float deltaTime)
        {
            Vector3 direction =
                worldTarget - transform.position;
            direction.y = 0f;
            FaceDirection(direction, deltaTime);
        }

        private void FaceDirection(
            Vector3 direction,
            float deltaTime)
        {
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Quaternion desired =
                Quaternion.LookRotation(
                    direction.normalized,
                    Vector3.up);

            transform.rotation =
                Quaternion.RotateTowards(
                    transform.rotation,
                    desired,
                    turnDegreesPerSecond *
                    Mathf.Max(0f, deltaTime));
        }
    }
}
