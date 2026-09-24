using System;
using System.Collections.Generic;
using ReturnVector.Combat;
using ReturnVector.Debugging;
using ReturnVector.Surfaces;
using UnityEngine;

namespace ReturnVector.Weapon
{
    /// <summary>
    /// Controlled outbound projectile simulation.
    /// Travel uses fixed-step swept casts so collision response and surface routing stay predictable.
    /// </summary>
    public sealed class OutboundWeaponMotor : MonoBehaviour
    {
        private const int HitBufferSize = 32;
        private const int CurvatureBufferSize = 16;
        private const int OverlapBufferSize = 24;

        [SerializeField] private WeaponController weapon;
        [SerializeField] private WeaponThrowTuning tuning;
        [SerializeField] private RVDebugSettings debugSettings;

        private readonly RaycastHit[] hitBuffer = new RaycastHit[HitBufferSize];
        private readonly Collider[] curvatureBuffer = new Collider[CurvatureBufferSize];
        private readonly Collider[] overlapBuffer = new Collider[OverlapBufferSize];
        private readonly HashSet<int> damagedColliderIds = new HashSet<int>();
        private readonly HashSet<int> passedSurfaceColliderIds = new HashSet<int>();

        private Vector3 direction;
        private float currentSpeed;
        private float remainingDistance;
        private float travelledDistance;
        private float accumulator;
        private float localImpactPause;
        private bool active;
        private Vector3 lastSafePosition;

        public bool IsActive => active;
        public Vector3 Direction => direction;
        public float Speed => currentSpeed;
        public float TravelledDistance => travelledDistance;
        public float RemainingDistance => remainingDistance;
        public WeaponSurfaceKind? LastSurfaceResponse { get; private set; }

        public event Action<WeaponImpactInfo> Impacted;
        public event Action<WeaponSurfaceInteractionInfo> SurfaceInteracted;
        public event Action Parked;

        public void Configure(
            WeaponController newWeapon,
            WeaponThrowTuning newTuning,
            RVDebugSettings newDebugSettings)
        {
            weapon = newWeapon;
            tuning = newTuning;
            debugSettings = newDebugSettings;
        }

        public bool Begin(Vector3 outboundDirection)
        {
            if (weapon == null || tuning == null ||
                weapon.State != WeaponState.Outbound)
            {
                return false;
            }

            Vector3 flatDirection = outboundDirection;
            flatDirection.y = 0f;

            if (flatDirection.sqrMagnitude < 0.0001f)
            {
                return false;
            }

            direction = flatDirection.normalized;
            currentSpeed = tuning.OutboundSpeed;
            remainingDistance = tuning.MaxDistance;
            travelledDistance = 0f;
            accumulator = 0f;
            localImpactPause = 0f;
            damagedColliderIds.Clear();
            passedSurfaceColliderIds.Clear();
            LastSurfaceResponse = null;
            active = true;
            lastSafePosition = transform.position;

            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            return true;
        }


        public bool DeflectToward(
            Vector3 desiredWorldDirection,
            float maxDegrees)
        {
            if (!active ||
                maxDegrees <= 0f)
            {
                return false;
            }

            Vector3 desired = desiredWorldDirection;
            desired.y = 0f;

            if (desired.sqrMagnitude < 0.0001f ||
                direction.sqrMagnitude < 0.0001f)
            {
                return false;
            }

            direction = Vector3.RotateTowards(
                direction.normalized,
                desired.normalized,
                Mathf.Deg2Rad * maxDegrees,
                0f);
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f)
            {
                return false;
            }

            direction.Normalize();
            transform.rotation =
                Quaternion.LookRotation(
                    direction,
                    Vector3.up);
            return true;
        }

        public void Abort()
        {
            active = false;
            accumulator = 0f;
            localImpactPause = 0f;
            damagedColliderIds.Clear();
            passedSurfaceColliderIds.Clear();
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        public void Tick(float deltaTime)
        {
            if (!active ||
                weapon == null ||
                tuning == null ||
                weapon.State != WeaponState.Outbound)
            {
                return;
            }

            if (deltaTime <= 0f ||
                float.IsNaN(deltaTime) ||
                float.IsInfinity(deltaTime))
            {
                return;
            }

            if (localImpactPause > 0f)
            {
                localImpactPause = Mathf.Max(
                    0f,
                    localImpactPause - Time.unscaledDeltaTime);
                return;
            }

            // Keep projectile motion on its authored simulation step even when rendering frames vary.
            accumulator = Mathf.Min(
                accumulator + deltaTime,
                tuning.MaxAccumulatedTime);

            float step = tuning.SimulationStep;
            int steps = 0;

            while (accumulator >= step &&
                   steps < tuning.MaxSimulationStepsPerFrame &&
                   active)
            {
                SimulateStep(step);
                accumulator -= step;
                steps++;

                if (localImpactPause > 0f)
                {
                    break;
                }
            }
        }

        private void SimulateStep(float deltaTime)
        {
            if (WeaponCollisionUtility.HasBlockingOverlap(
                    transform.position,
                    tuning.CollisionRadius,
                    tuning.CollisionMask,
                    AttackPhase.Outbound,
                    overlapBuffer,
                    transform,
                    weapon.Owner))
            {
                transform.position = lastSafePosition;
                ParkAtCurrentPosition(embedded: true);
                return;
            }

            // Curvature fields steer the travel direction before this step is swept for collisions.
            direction = WeaponCurvatureUtility.ApplyAtPosition(
                transform.position,
                direction,
                AttackPhase.Outbound,
                deltaTime,
                curvatureBuffer,
                out int fieldsApplied);

            if (fieldsApplied > 0 &&
                debugSettings != null &&
                debugSettings.DrawCurvatureInfluence)
            {
                RVDebugDraw.Line(
                    transform.position,
                    transform.position + direction * 1.2f,
                    debugSettings.CurvatureColor,
                    0.08f);
            }

            float requestedTravel = OutboundTravelMath.StepDistance(
                currentSpeed,
                deltaTime,
                remainingDistance);

            if (requestedTravel <= 0.000001f)
            {
                ParkAtCurrentPosition(embedded: false);
                return;
            }

            Vector3 origin = transform.position;

            // Every contact in the step is sorted front-to-back so response order stays readable.
            int hitCount = Physics.SphereCastNonAlloc(
                origin,
                tuning.CollisionRadius,
                direction,
                hitBuffer,
                requestedTravel,
                tuning.CollisionMask,
                QueryTriggerInteraction.Ignore);

            WeaponCollisionUtility.SortHitsByDistance(hitBuffer, hitCount);

            float actualTravel = requestedTravel;
            RaycastHit blockingHit = default;
            bool foundBlockingHit = false;
            bool blockingImpactAlreadyReported = false;
            bool pausedByTargetHit = false;

            bool reflected = false;
            RaycastHit reflectionHit = default;
            WeaponSurfaceResponse reflectionResponse = default;
            Vector3 incomingBeforeReflection = direction;

            bool targetDeflected = false;
            RaycastHit targetDeflectionHit = default;
            Vector3 targetDeflectionDirection = Vector3.zero;

            float extraDistanceCost = 0f;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = hitBuffer[i];
                Collider collider = hit.collider;

                if (WeaponCollisionUtility.IsOwnedCollider(
                        collider,
                        transform,
                        weapon.Owner))
                {
                    continue;
                }

                if (passedSurfaceColliderIds.Contains(collider.GetInstanceID()))
                {
                    continue;
                }

                int targetColliderId = collider.GetInstanceID();

                // Rich receivers get first say: shields and bosses can block or deflect the weapon.
                if (WeaponCollisionUtility.TryGetWeaponHitReceiver(
                        collider,
                        out IWeaponHitReceiver hitReceiver))
                {
                    if (!damagedColliderIds.Add(targetColliderId))
                    {
                        continue;
                    }

                    DamageInfo damage = new DamageInfo(
                        tuning.OutboundDamage,
                        hit.point,
                        direction,
                        weapon.Owner != null
                            ? weapon.Owner.gameObject
                            : null,
                        gameObject,
                        AttackPhase.Outbound);

                    WeaponHitResult result =
                        hitReceiver.ResolveWeaponHit(in damage);

                    DrawNormal(hit, 0.6f, 0.15f);

                    Impacted?.Invoke(
                        new WeaponImpactInfo(
                            hit.point,
                            hit.normal,
                            collider,
                            blocking: result.BlocksWeapon,
                            damagedTarget: result.DamagedTarget));

                    if (result.BlocksWeapon)
                    {
                        actualTravel = WeaponCollisionUtility.StopDistance(
                            hit.distance,
                            tuning.SurfaceBackoff);
                        blockingHit = hit;
                        foundBlockingHit = true;
                        blockingImpactAlreadyReported = true;
                        break;
                    }

                    if (result.DeflectsWeapon)
                    {
                        actualTravel = WeaponCollisionUtility.StopDistance(
                            hit.distance,
                            tuning.SurfaceBackoff);

                        Vector3 reflectedDirection =
                            Vector3.Reflect(
                                direction,
                                hit.normal.sqrMagnitude > 0.0001f
                                    ? hit.normal.normalized
                                    : -direction);

                        reflectedDirection.y = 0f;

                        Vector3 desired =
                            reflectedDirection.sqrMagnitude > 0.0001f
                                ? reflectedDirection.normalized
                                : -direction;

                        targetDeflectionDirection =
                            Vector3.RotateTowards(
                                direction,
                                desired,
                                Mathf.Deg2Rad *
                                Mathf.Max(0f, result.DeflectionDegrees),
                                0f);

                        targetDeflectionDirection.y = 0f;

                        if (targetDeflectionDirection.sqrMagnitude <
                            0.0001f)
                        {
                            targetDeflectionDirection = -direction;
                        }

                        targetDeflectionDirection.Normalize();
                        targetDeflected = true;
                        targetDeflectionHit = hit;
                        pausedByTargetHit = false;
                        break;
                    }

                    if (result.DamagedTarget &&
                        tuning.EnemyImpactPauseSeconds > 0f)
                    {
                        actualTravel = WeaponCollisionUtility.StopDistance(
                            hit.distance,
                            tuning.SurfaceBackoff);

                        localImpactPause =
                            tuning.EnemyImpactPauseSeconds;
                        pausedByTargetHit = true;
                        break;
                    }

                    continue;
                }

                if (WeaponCollisionUtility.TryGetDamageable(
                        collider,
                        out IDamageable damageable))
                {
                    if (damagedColliderIds.Add(targetColliderId))
                    {
                        DamageInfo damage = new DamageInfo(
                            tuning.OutboundDamage,
                            hit.point,
                            direction,
                            weapon.Owner != null
                                ? weapon.Owner.gameObject
                                : null,
                            gameObject,
                            AttackPhase.Outbound);

                        damageable.ReceiveDamage(in damage);
                        DrawNormal(hit, 0.6f, 0.15f);

                        Impacted?.Invoke(
                            new WeaponImpactInfo(
                                hit.point,
                                hit.normal,
                                collider,
                                blocking: false,
                                damagedTarget: true));

                        if (tuning.EnemyImpactPauseSeconds > 0f)
                        {
                            actualTravel = WeaponCollisionUtility.StopDistance(
                            hit.distance,
                            tuning.SurfaceBackoff);

                            localImpactPause =
                                tuning.EnemyImpactPauseSeconds;
                            pausedByTargetHit = true;
                            break;
                        }
                    }

                    continue;
                }

                // Static geometry reaches the authored material response after combat targets are resolved.
                if (WeaponSurfaceResolver.TryResolve(
                        collider,
                        direction,
                        hit.normal,
                        AttackPhase.Outbound,
                        out WeaponSurfaceResponse response))
                {
                    LastSurfaceResponse = response.Kind;

                    if (response.Blocks)
                    {
                        actualTravel = WeaponCollisionUtility.StopDistance(
                            hit.distance,
                            tuning.SurfaceBackoff);
                        blockingHit = hit;
                        foundBlockingHit = true;

                        EmitSurfaceInteraction(
                            response.Kind,
                            hit,
                            direction,
                            Vector3.zero);
                        break;
                    }

                    if (response.Kind == WeaponSurfaceKind.Reflective)
                    {
                        actualTravel = WeaponCollisionUtility.StopDistance(
                            hit.distance,
                            tuning.SurfaceBackoff);

                        reflected = true;
                        reflectionHit = hit;
                        reflectionResponse = response;
                        incomingBeforeReflection = direction;
                        break;
                    }

                    // Penetrable and curving solids continue through this cast.
                    // Curvature itself is handled continuously by trigger volumes.
                    passedSurfaceColliderIds.Add(collider.GetInstanceID());
                    currentSpeed *= response.SpeedRetention;
                    extraDistanceCost += response.DistanceCost;

                    EmitSurfaceInteraction(
                        response.Kind,
                        hit,
                        direction,
                        direction);

                    Impacted?.Invoke(
                        new WeaponImpactInfo(
                            hit.point,
                            hit.normal,
                            collider,
                            blocking: false,
                            damagedTarget: false));

                    continue;
                }

                actualTravel = WeaponCollisionUtility.StopDistance(
                            hit.distance,
                            tuning.SurfaceBackoff);
                blockingHit = hit;
                foundBlockingHit = true;
                break;
            }

            // Apply only the distance that survived hit resolution for this simulation step.
            Vector3 destination = origin + direction * actualTravel;

            if (debugSettings != null &&
                debugSettings.DrawCastSweeps)
            {
                RVDebugDraw.Line(
                    origin,
                    destination,
                    debugSettings.OutboundPathColor,
                    0.1f);
            }

            transform.position = destination;

            if (WeaponCollisionUtility.HasBlockingOverlap(
                    transform.position,
                    tuning.CollisionRadius,
                    tuning.CollisionMask,
                    AttackPhase.Outbound,
                    overlapBuffer,
                    transform,
                    weapon.Owner))
            {
                transform.position = lastSafePosition;
                ParkAtCurrentPosition(embedded: true);
                return;
            }

            lastSafePosition = transform.position;
            travelledDistance += actualTravel;
            remainingDistance = Mathf.Max(
                0f,
                remainingDistance - actualTravel - extraDistanceCost);

            // Target deflection and material reflection are resolved after moving to the contact point.
            if (targetDeflected)
            {
                direction = targetDeflectionDirection;

                DrawNormal(
                    targetDeflectionHit,
                    0.8f,
                    0.2f);

                float separation =
                    Mathf.Max(
                        0.001f,
                        tuning.SurfaceBackoff * 2f);

                Vector3 separatedPosition =
                    transform.position +
                    direction * separation;

                if (!WeaponCollisionUtility.HasBlockingOverlap(
                        separatedPosition,
                        tuning.CollisionRadius,
                        tuning.CollisionMask,
                        AttackPhase.Outbound,
                        overlapBuffer,
                        transform,
                        weapon.Owner))
                {
                    transform.position = separatedPosition;
                    lastSafePosition = transform.position;
                    travelledDistance += separation;
                }

                remainingDistance = Mathf.Max(
                    0f,
                    remainingDistance - separation);

                transform.rotation =
                    Quaternion.LookRotation(
                        direction,
                        Vector3.up);

                return;
            }

            if (foundBlockingHit)
            {
                DrawNormal(blockingHit, 0.8f, 0.2f);

                if (!blockingImpactAlreadyReported)
                {
                    Impacted?.Invoke(
                        new WeaponImpactInfo(
                            blockingHit.point,
                            blockingHit.normal,
                            blockingHit.collider,
                            blocking: true,
                            damagedTarget: false));
                }

                ParkAtCurrentPosition(embedded: true);
                return;
            }

            if (reflected)
            {
                direction = reflectionResponse.OutgoingDirection;
                currentSpeed *= reflectionResponse.SpeedRetention;
                remainingDistance = Mathf.Max(
                    0f,
                    remainingDistance - reflectionResponse.DistanceCost);

                DrawNormal(reflectionHit, 0.8f, 0.2f);

                if (debugSettings != null &&
                    debugSettings.DrawSurfaceResponses)
                {
                    RVDebugDraw.Line(
                        reflectionHit.point,
                        reflectionHit.point + direction * 1.5f,
                        debugSettings.ReflectionColor,
                        0.2f);
                }

                EmitSurfaceInteraction(
                    WeaponSurfaceKind.Reflective,
                    reflectionHit,
                    incomingBeforeReflection,
                    direction);

                Impacted?.Invoke(
                    new WeaponImpactInfo(
                        reflectionHit.point,
                        reflectionHit.normal,
                        reflectionHit.collider,
                        blocking: false,
                        damagedTarget: false));

                float separation = Mathf.Max(
                    0.001f,
                    tuning.SurfaceBackoff * 2f);
                Vector3 separatedPosition =
                    transform.position + direction * separation;

                if (!WeaponCollisionUtility.HasBlockingOverlap(
                        separatedPosition,
                        tuning.CollisionRadius,
                        tuning.CollisionMask,
                        AttackPhase.Outbound,
                        overlapBuffer,
                        transform,
                        weapon.Owner))
                {
                    transform.position = separatedPosition;
                    lastSafePosition = transform.position;
                    travelledDistance += separation;
                }
                remainingDistance = Mathf.Max(
                    0f,
                    remainingDistance - separation);

                if (direction.sqrMagnitude > 0.0001f)
                {
                    transform.rotation =
                        Quaternion.LookRotation(direction, Vector3.up);
                }

                if (currentSpeed <= 0.01f ||
                    remainingDistance <= 0.0001f)
                {
                    ParkAtCurrentPosition(embedded: false);
                }

                return;
            }

            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.rotation =
                    Quaternion.LookRotation(direction, Vector3.up);
            }

            if (remainingDistance <= 0.0001f ||
                currentSpeed <= 0.01f)
            {
                ParkAtCurrentPosition(embedded: false);
                return;
            }

            if (pausedByTargetHit)
            {
                accumulator = 0f;
            }
        }

        private void EmitSurfaceInteraction(
            WeaponSurfaceKind kind,
            RaycastHit hit,
            Vector3 incoming,
            Vector3 outgoing)
        {
            SurfaceInteracted?.Invoke(
                new WeaponSurfaceInteractionInfo(
                    kind,
                    AttackPhase.Outbound,
                    hit.point,
                    hit.normal,
                    incoming,
                    outgoing,
                    hit.collider));
        }

        private void DrawNormal(
            RaycastHit hit,
            float length,
            float duration)
        {
            if (debugSettings == null ||
                !debugSettings.DrawCollisionNormals)
            {
                return;
            }

            RVDebugDraw.Normal(
                hit.point,
                hit.normal,
                length,
                debugSettings.CollisionNormalColor,
                duration);
        }

        // Parking keeps the persistent weapon in world space until recall begins.
        private void ParkAtCurrentPosition(bool embedded)
        {
            active = false;
            accumulator = 0f;
            localImpactPause = 0f;

            if (weapon != null &&
                weapon.State == WeaponState.Outbound)
            {
                if (embedded)
                {
                    weapon.MarkEmbedded();
                }
                else
                {
                    weapon.MarkParked();
                }
            }

            Parked?.Invoke();
        }
    }
}
