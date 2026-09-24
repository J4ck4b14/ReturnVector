using System.Collections.Generic;
using ReturnVector.Combat;
using ReturnVector.Surfaces;
using ReturnVector.Weapon;
using UnityEngine;

namespace ReturnVector.Debugging
{
    /// <summary>
    /// Development-only approximation of the current recall route.
    /// Unlike the player-facing preview, this diagnostic path understands authored surface rules.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public sealed class RecallPathDebugRenderer : MonoBehaviour
    {
        private const int HitBufferSize = 32;
        private const int CurvatureBufferSize = 16;

        [SerializeField] private WeaponController weapon;
        [SerializeField] private OutboundWeaponMotor outboundMotor;
        [SerializeField] private RecallWeaponMotor recallMotor;
        [SerializeField] private WeaponRecallTuning tuning;
        [SerializeField] private RVDebugSettings settings;
        [SerializeField] private LineRenderer line;

        private readonly RaycastHit[] hitBuffer =
            new RaycastHit[HitBufferSize];

        private readonly Collider[] curvatureBuffer =
            new Collider[CurvatureBufferSize];

        private readonly List<Vector3> points =
            new List<Vector3>(96);

        private readonly HashSet<int> passedSurfaceIds =
            new HashSet<int>();

        public void Configure(
            WeaponController newWeapon,
            OutboundWeaponMotor newOutboundMotor,
            RecallWeaponMotor newRecallMotor,
            WeaponRecallTuning newTuning,
            RVDebugSettings newSettings,
            LineRenderer newLine)
        {
            weapon = newWeapon;
            outboundMotor = newOutboundMotor;
            recallMotor = newRecallMotor;
            tuning = newTuning;
            settings = newSettings;
            line = newLine != null
                ? newLine
                : GetComponent<LineRenderer>();

            ApplyLineSettings();
        }

        private void Awake()
        {
            if (line == null)
            {
                line = GetComponent<LineRenderer>();
            }

            ApplyLineSettings();
        }

        private void LateUpdate()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!CanDraw())
            {
                SetVisible(false);
                return;
            }

            BuildPath();

            line.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
            {
                line.SetPosition(i, points[i]);
            }

            SetVisible(points.Count >= 2);
#else
            SetVisible(false);
#endif
        }

        private bool CanDraw()
        {
            if (weapon == null ||
                tuning == null ||
                settings == null ||
                line == null ||
                !settings.DrawPlannedReturnRoute)
            {
                return false;
            }

            return weapon.State == WeaponState.Outbound ||
                   weapon.State == WeaponState.Parked ||
                   weapon.State == WeaponState.Embedded ||
                   weapon.State == WeaponState.Returning;
        }

        private void BuildPath()
        {
            points.Clear();
            passedSurfaceIds.Clear();

            Transform target =
                weapon.HeldAnchor != null
                    ? weapon.HeldAnchor
                    : weapon.Owner;

            if (target == null)
            {
                return;
            }

            Vector3 position = weapon.transform.position;
            Vector3 direction =
                InitialDirection(position, target.position);

            float speed =
                recallMotor != null && recallMotor.IsActive
                    ? Mathf.Max(0.01f, recallMotor.Speed)
                    : tuning.InitialSpeed;

            points.Add(position);

            for (int segment = 0;
                 segment < tuning.DebugPathSegments;
                 segment++)
            {
                Vector3 toTarget =
                    target.position - position;
                toTarget.y = 0f;

                float distance = toTarget.magnitude;

                if (distance <= tuning.CatchRadius)
                {
                    points.Add(target.position);
                    break;
                }

                float turnRate =
                    RecallTravelMath.TurnRateForDistance(
                        tuning.TurnDegreesPerSecond,
                        distance,
                        tuning.NearCatchDistance,
                        tuning.NearCatchTurnMultiplier);

                direction = RecallTravelMath.Steer(
                    direction,
                    toTarget,
                    turnRate,
                    tuning.DebugPathStepSeconds);

                direction =
                    WeaponCurvatureUtility.ApplyAtPosition(
                        position,
                        direction,
                        AttackPhase.Recall,
                        tuning.DebugPathStepSeconds,
                        curvatureBuffer,
                        out _);

                speed = RecallTravelMath.Accelerate(
                    speed,
                    tuning.MaxSpeed,
                    tuning.Acceleration,
                    tuning.DebugPathStepSeconds);

                float travel =
                    RecallTravelMath.StepDistance(
                        speed,
                        tuning.DebugPathStepSeconds,
                        distance);

                if (travel <= 0.000001f)
                {
                    break;
                }

                if (TryFindWorldInteraction(
                        position,
                        direction,
                        travel,
                        out RaycastHit hit,
                        out bool hasSurfaceResponse,
                        out WeaponSurfaceResponse response))
                {
                    float impactTravel =
                        Mathf.Max(
                            0f,
                            hit.distance -
                            tuning.SurfaceBackoff);

                    position += direction * impactTravel;
                    points.Add(position);

                    if (!hasSurfaceResponse ||
                        response.Blocks)
                    {
                        break;
                    }

                    if (response.Kind ==
                        WeaponSurfaceKind.Reflective)
                    {
                        direction =
                            response.OutgoingDirection;

                        speed *= response.SpeedRetention;

                        float separation =
                            Mathf.Max(
                                0.001f,
                                tuning.SurfaceBackoff * 2f);

                        position += direction * separation;
                        points.Add(position);
                        continue;
                    }

                    passedSurfaceIds.Add(
                        hit.collider.GetInstanceID());

                    speed *= response.SpeedRetention;

                    // Continue the remainder of the diagnostic step after
                    // passing through penetrable/non-blocking geometry.
                    float remainingTravel =
                        Mathf.Max(0f, travel - impactTravel);

                    position += direction * remainingTravel;
                    points.Add(position);
                    continue;
                }

                position += direction * travel;
                points.Add(position);
            }
        }

        private Vector3 InitialDirection(
            Vector3 position,
            Vector3 target)
        {
            if (recallMotor != null &&
                recallMotor.IsActive &&
                recallMotor.Direction.sqrMagnitude > 0.0001f)
            {
                return recallMotor.Direction;
            }

            if (weapon.State == WeaponState.Outbound &&
                outboundMotor != null &&
                outboundMotor.Direction.sqrMagnitude > 0.0001f)
            {
                return outboundMotor.Direction;
            }

            Vector3 towardTarget = target - position;
            towardTarget.y = 0f;

            if (towardTarget.sqrMagnitude > 0.0001f)
            {
                return towardTarget.normalized;
            }

            Vector3 forward = weapon.transform.forward;
            forward.y = 0f;

            return forward.sqrMagnitude > 0.0001f
                ? forward.normalized
                : Vector3.forward;
        }

        private bool TryFindWorldInteraction(
            Vector3 origin,
            Vector3 direction,
            float distance,
            out RaycastHit worldHit,
            out bool hasSurfaceResponse,
            out WeaponSurfaceResponse surfaceResponse)
        {
            worldHit = default;
            hasSurfaceResponse = false;
            surfaceResponse = default;

            int hitCount = Physics.SphereCastNonAlloc(
                origin,
                tuning.CollisionRadius,
                direction,
                hitBuffer,
                distance,
                tuning.CollisionMask,
                QueryTriggerInteraction.Ignore);

            WeaponCollisionUtility.SortHitsByDistance(
                hitBuffer,
                hitCount);

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = hitBuffer[i];

                if (WeaponCollisionUtility.IsOwnedCollider(
                        hit.collider,
                        weapon.transform,
                        weapon.Owner))
                {
                    continue;
                }

                if (WeaponCollisionUtility.TryGetDamageable(
                        hit.collider,
                        out _))
                {
                    continue;
                }

                int colliderId =
                    hit.collider.GetInstanceID();

                if (passedSurfaceIds.Contains(colliderId))
                {
                    continue;
                }

                worldHit = hit;

                if (WeaponSurfaceResolver.TryResolve(
                        hit.collider,
                        direction,
                        hit.normal,
                        AttackPhase.Recall,
                        out WeaponSurfaceResponse response))
                {
                    hasSurfaceResponse = true;
                    surfaceResponse = response;
                }

                return true;
            }

            return false;
        }

        private void ApplyLineSettings()
        {
            if (line == null || tuning == null)
            {
                return;
            }

            line.useWorldSpace = true;
            line.startWidth = tuning.DebugPathWidth;
            line.endWidth = tuning.DebugPathWidth;
            line.positionCount = 0;
        }

        private void SetVisible(bool visible)
        {
            if (line != null)
            {
                line.enabled = visible;
            }
        }
    }
}
