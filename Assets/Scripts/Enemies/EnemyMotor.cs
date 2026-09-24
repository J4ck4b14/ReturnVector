using System.Collections.Generic;
using UnityEngine;

namespace ReturnVector.Enemies
{
    /// <summary>
    /// Planar enemy locomotion with A* routing around blocking geometry.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyMotor : MonoBehaviour
    {
        [SerializeField] private CharacterController controller;
        [SerializeField, Min(0f)] private float turnDegreesPerSecond = 720f;
        [SerializeField, Min(0.4f)] private float pathCellSize = 0.72f;
        [SerializeField, Min(0.1f)] private float pathAgentRadius = 0.42f;
        [SerializeField, Min(0.05f)] private float repathSeconds = 0.28f;
        [SerializeField, Min(2f)] private float pathPadding = 5.5f;

        private readonly List<Vector3> path = new List<Vector3>(32);
        private int pathIndex;
        private float repathTimer;
        private Vector3 lastPathTarget;

        public Vector3 Velocity { get; private set; }
        public float Speed => Velocity.magnitude;

        public void Configure(
            CharacterController newController,
            float turnRate = 720f)
        {
            controller = newController;
            turnDegreesPerSecond = Mathf.Max(0f, turnRate);

            if (newController != null)
            {
                pathAgentRadius = Mathf.Max(
                    0.18f,
                    newController.radius * 0.92f);
            }

            path.Clear();
            pathIndex = 0;
            repathTimer = 0f;
        }

        public void MoveToward(
            Vector3 worldTarget,
            float speed,
            float deltaTime,
            float stoppingDistance = 0f)
        {
            Vector3 toTarget = worldTarget - transform.position;
            toTarget.y = 0f;

            float distance = toTarget.magnitude;
            if (distance <= Mathf.Max(0f, stoppingDistance) || speed <= 0f)
            {
                Velocity = Vector3.zero;
                FaceDirection(
                    distance > 0.0001f
                        ? toTarget.normalized
                        : transform.forward,
                    deltaTime);
                return;
            }

            repathTimer -= Mathf.Max(0f, deltaTime);

            if (AStarPathfinder.HasClearSegment(
                    transform.position,
                    worldTarget,
                    pathAgentRadius,
                    transform))
            {
                path.Clear();
                pathIndex = 0;
                MoveStraightToward(
                    worldTarget,
                    speed,
                    deltaTime,
                    stoppingDistance);
                return;
            }

            bool targetChanged =
                (worldTarget - lastPathTarget).sqrMagnitude >
                pathCellSize * pathCellSize;

            if (path.Count == 0 ||
                pathIndex >= path.Count ||
                repathTimer <= 0f ||
                targetChanged)
            {
                RebuildPath(worldTarget);
            }

            if (path.Count == 0 || pathIndex >= path.Count)
            {
                // No route exists right now. CharacterController collision still keeps
                // the fallback movement honest while a later repath can recover.
                MoveStraightToward(
                    worldTarget,
                    speed,
                    deltaTime,
                    stoppingDistance);
                return;
            }

            SkipVisibleWaypoints();

            Vector3 waypoint = path[pathIndex];
            Vector3 toWaypoint = waypoint - transform.position;
            toWaypoint.y = 0f;

            if (toWaypoint.sqrMagnitude <= pathCellSize * pathCellSize * 0.20f)
            {
                pathIndex++;

                if (pathIndex >= path.Count)
                {
                    MoveStraightToward(
                        worldTarget,
                        speed,
                        deltaTime,
                        stoppingDistance);
                    return;
                }

                waypoint = path[pathIndex];
            }

            MoveStraightToward(
                waypoint,
                speed,
                deltaTime,
                0f);
        }

        public void MoveAwayFrom(
            Vector3 worldThreat,
            float speed,
            float deltaTime)
        {
            Vector3 away = transform.position - worldThreat;
            away.y = 0f;

            if (away.sqrMagnitude < 0.0001f || speed <= 0f)
            {
                Velocity = Vector3.zero;
                return;
            }

            away.Normalize();
            Vector3 bestDirection = Vector3.zero;
            float bestScore = float.NegativeInfinity;

            for (int i = -3; i <= 3; i++)
            {
                Vector3 candidate =
                    Quaternion.Euler(0f, i * 30f, 0f) * away;

                Vector3 end =
                    transform.position +
                    candidate * Mathf.Max(1.2f, speed * 0.35f);

                if (!AStarPathfinder.HasClearSegment(
                        transform.position,
                        end,
                        pathAgentRadius,
                        transform))
                {
                    continue;
                }

                float score = Vector3.Dot(candidate, away);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestDirection = candidate;
                }
            }

            path.Clear();
            pathIndex = 0;
            repathTimer = 0f;

            if (bestDirection.sqrMagnitude < 0.0001f)
            {
                Stop();
                return;
            }

            MoveDirection(bestDirection, speed, deltaTime);
        }

        public void MoveDirection(
            Vector3 worldDirection,
            float speed,
            float deltaTime)
        {
            Vector3 direction = worldDirection;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f || speed <= 0f)
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

            if (controller != null && controller.enabled)
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
            Vector3 direction = worldTarget - transform.position;
            direction.y = 0f;
            FaceDirection(direction, deltaTime);
        }

        private void RebuildPath(Vector3 worldTarget)
        {
            path.Clear();
            pathIndex = 0;
            lastPathTarget = worldTarget;
            repathTimer = repathSeconds;

            AStarPathfinder.TryFindPath(
                transform.position,
                worldTarget,
                pathCellSize,
                pathAgentRadius,
                transform,
                path,
                pathPadding);
        }

        private void SkipVisibleWaypoints()
        {
            for (int i = path.Count - 1; i > pathIndex; i--)
            {
                if (!AStarPathfinder.HasClearSegment(
                        transform.position,
                        path[i],
                        pathAgentRadius,
                        transform))
                {
                    continue;
                }

                pathIndex = i;
                break;
            }
        }

        private void MoveStraightToward(
            Vector3 target,
            float speed,
            float deltaTime,
            float stoppingDistance)
        {
            Vector3 toTarget = target - transform.position;
            toTarget.y = 0f;

            float distance = toTarget.magnitude;
            if (distance <= Mathf.Max(0f, stoppingDistance) || speed <= 0f)
            {
                Velocity = Vector3.zero;
                return;
            }

            Vector3 direction = toTarget / distance;
            float travel =
                Mathf.Min(
                    speed * Mathf.Max(0f, deltaTime),
                    Mathf.Max(0f, distance - stoppingDistance));

            Velocity =
                deltaTime > 0f
                    ? direction * (travel / deltaTime)
                    : Vector3.zero;

            if (controller != null && controller.enabled)
            {
                controller.Move(direction * travel);
            }
            else
            {
                transform.position += direction * travel;
            }

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
