using System;
using System.Collections.Generic;
using ReturnVector.Combat;
using ReturnVector.Debugging;
using ReturnVector.Surfaces;
using UnityEngine;

// Script summary: Controlled return simulation. The weapon continuously steers toward the live catch point, then authored surface rules may redirect, pass or stop that return.

namespace ReturnVector.Weapon
{
    /// <summary>
    /// Controlled return simulation. The weapon continuously steers toward the live catch point,
    /// then authored surface rules may redirect, pass or stop that return.
    /// </summary>
    public sealed class RecallWeaponMotor : MonoBehaviour
    {
        // Collision variables
        private const int HitBufferSize = 32;
        private const int CurvatureBufferSize = 16;
        private const int OverlapBufferSize = 24;

        // Weapon variables
        [SerializeField] private WeaponController weapon;
        [SerializeField] private WeaponRecallTuning tuning;
        [SerializeField] private RVDebugSettings debugSettings;

        // Collision variables
        private readonly RaycastHit[] hitBuffer = new RaycastHit[HitBufferSize];
        private readonly Collider[] curvatureBuffer = new Collider[CurvatureBufferSize];
        private readonly Collider[] overlapBuffer = new Collider[OverlapBufferSize];
        private readonly HashSet<int> damagedColliderIds = new HashSet<int>();
        private readonly HashSet<int> passedSurfaceColliderIds = new HashSet<int>();

        // Weapon variables
        private Vector3 direction;
        private float speed;
        private float accumulator;
        private float localImpactPause;
        private bool active;
        private Vector3 lastSafePosition;

        private float catchElapsed;
        private Vector3 catchStartPosition;
        private Quaternion catchStartRotation;

        public bool IsActive => active;
        public Vector3 Direction => direction;
        public float Speed => speed;
        public WeaponSurfaceKind? LastSurfaceResponse { get; private set; }

        public Transform CatchTarget =>
            weapon != null
                ? (weapon.HeldAnchor != null
                    ? weapon.HeldAnchor
                    : weapon.Owner)
                : null;

        public float DistanceToCatch
        {
            get
            {
                Transform target = CatchTarget;
                return target != null
                    ? Vector3.Distance(
                        transform.position,
                        target.position)
                    : 0f;
            }
        }

        public event Action RecallStarted;
        public event Action<WeaponImpactInfo> Impacted;
        public event Action<WeaponSurfaceInteractionInfo> SurfaceInteracted;
        public event Action RecallBlocked;
        public event Action CatchStarted;
        public event Action CatchCompleted;

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(
            WeaponController newWeapon,
            WeaponRecallTuning newTuning,
            RVDebugSettings newDebugSettings)
        {
            weapon = newWeapon;
            tuning = newTuning;
            debugSettings = newDebugSettings;
        }

        public bool CanBegin =>
            weapon != null &&
            tuning != null &&
            CatchTarget != null;

        /// <summary>
        /// Starts the corresponding weapon travel simulation.
        /// </summary>
        public bool Begin(Vector3 initialDirection)
        {
            if (!CanBegin ||
                weapon.State != WeaponState.Returning)
            {
                return false;
            }

            Vector3 desired =
                CatchTarget.position - transform.position;
            desired.y = 0f;

            Vector3 initial = initialDirection;
            initial.y = 0f;

            direction = initial.sqrMagnitude >= 0.0001f
                ? initial.normalized
                : desired.sqrMagnitude >= 0.0001f
                    ? desired.normalized
                    : transform.forward;

            speed = Mathf.Min(
                Mathf.Max(0f, tuning.InitialSpeed),
                Mathf.Max(0f, tuning.MaxSpeed));

            accumulator = 0f;
            localImpactPause = 0f;
            catchElapsed = 0f;
            damagedColliderIds.Clear();
            passedSurfaceColliderIds.Clear();
            LastSurfaceResponse = null;
            active = true;
            lastSafePosition = transform.position;

            RecallStarted?.Invoke();
            return true;
        }


        /// <summary>
        /// Redirects weapon travel toward the requested world-space direction.
        /// </summary>
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

        /// <summary>
        /// Stops the current weapon travel simulation immediately.
        /// </summary>
        public void Abort()
        {
            active = false;
            accumulator = 0f;
            localImpactPause = 0f;
            catchElapsed = 0f;
            damagedColliderIds.Clear();
            passedSurfaceColliderIds.Clear();
        }

        /// <summary>
        /// Advances the component for the current frame.
        /// </summary>
        private void Update()
        {
            Tick(Time.deltaTime);
        }

        /// <summary>
        /// Advances the current fixed-step simulation.
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (!active ||
                weapon == null ||
                tuning == null)
            {
                return;
            }

            if (deltaTime <= 0f ||
                float.IsNaN(deltaTime) ||
                float.IsInfinity(deltaTime))
            {
                return;
            }

            if (weapon.State == WeaponState.Catching)
            {
                TickCatch(deltaTime);
                return;
            }

            if (weapon.State != WeaponState.Returning)
            {
                Abort();
                return;
            }

            if (localImpactPause > 0f)
            {
                localImpactPause = Mathf.Max(
                    0f,
                    localImpactPause - Time.unscaledDeltaTime);
                return;
            }

            // Recall uses the same fixed simulation cadence as outbound travel.
            accumulator = Mathf.Min(
                accumulator + deltaTime,
                tuning.MaxAccumulatedTime);

            float step = tuning.SimulationStep;
            int steps = 0;

            while (accumulator >= step &&
                   steps < tuning.MaxSimulationStepsPerFrame &&
                   active &&
                   weapon.State == WeaponState.Returning)
            {
                SimulateRecallStep(step);
                accumulator -= step;
                steps++;

                if (localImpactPause > 0f)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Simulates one recall weapon movement step.
        /// </summary>
        private void SimulateRecallStep(float deltaTime)
        {
            if (WeaponCollisionUtility.HasBlockingOverlap(
                    transform.position,
                    tuning.CollisionRadius,
                    tuning.CollisionMask,
                    AttackPhase.Recall,
                    overlapBuffer,
                    transform,
                    weapon.Owner))
            {
                transform.position = lastSafePosition;
                active = false;
                accumulator = 0f;
                localImpactPause = 0f;
                weapon.MarkEmbedded();
                RecallBlocked?.Invoke();
                return;
            }

            Transform target = CatchTarget;
            if (target == null)
            {
                Abort();
                return;
            }

            Vector3 toTarget =
                target.position - transform.position;
            toTarget.y = 0f;
            float distanceToTarget = toTarget.magnitude;

            if (distanceToTarget <= tuning.CatchRadius &&
                CanCatchDirectly(target))
            {
                BeginCatch();
                return;
            }

            // Steering tightens near the hand to shape the final catch approach.
            float turnRate =
                RecallTravelMath.TurnRateForDistance(
                    tuning.TurnDegreesPerSecond,
                    distanceToTarget,
                    tuning.NearCatchDistance,
                    tuning.NearCatchTurnMultiplier);

            direction = RecallTravelMath.Steer(
                direction,
                toTarget,
                turnRate,
                deltaTime);

            direction = WeaponCurvatureUtility.ApplyAtPosition(
                transform.position,
                direction,
                AttackPhase.Recall,
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

            speed = RecallTravelMath.Accelerate(
                speed,
                tuning.MaxSpeed,
                tuning.Acceleration,
                deltaTime);

            float requestedTravel =
                RecallTravelMath.StepDistance(
                    speed,
                    deltaTime,
                    distanceToTarget);

            if (requestedTravel <= 0.000001f)
            {
                if (CanCatchDirectly(target))
                {
                    BeginCatch();
                }

                return;
            }

            Vector3 origin = transform.position;

            // Sweep the curved return segment and resolve contacts nearest-first.
            int hitCount = Physics.SphereCastNonAlloc(
                origin,
                tuning.CollisionRadius,
                direction,
                hitBuffer,
                requestedTravel,
                tuning.CollisionMask,
                QueryTriggerInteraction.Ignore);

            WeaponCollisionUtility.SortHitsByDistance(
                hitBuffer,
                hitCount);

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

                if (passedSurfaceColliderIds.Contains(
                        collider.GetInstanceID()))
                {
                    continue;
                }

                int targetColliderId = collider.GetInstanceID();

                // Armor-facing rules can change both recall damage and the weapon trajectory.
                if (WeaponCollisionUtility.TryGetWeaponHitReceiver(
                        collider,
                        out IWeaponHitReceiver hitReceiver))
                {
                    if (!damagedColliderIds.Add(targetColliderId))
                    {
                        continue;
                    }

                    DamageInfo damage = new DamageInfo(
                        tuning.RecallDamage,
                        hit.point,
                        direction,
                        weapon.Owner != null
                            ? weapon.Owner.gameObject
                            : null,
                        gameObject,
                        AttackPhase.Recall);

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
                            tuning.RecallDamage,
                            hit.point,
                            direction,
                            weapon.Owner != null
                                ? weapon.Owner.gameObject
                                : null,
                            gameObject,
                            AttackPhase.Recall);

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

                // World geometry is resolved through the same surface contract as outbound travel.
                if (WeaponSurfaceResolver.TryResolve(
                        collider,
                        direction,
                        hit.normal,
                        AttackPhase.Recall,
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

                    if (response.Kind ==
                        WeaponSurfaceKind.Reflective)
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

                    passedSurfaceColliderIds.Add(
                        collider.GetInstanceID());
                    speed *= response.SpeedRetention;

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

            // Commit the resolved segment before applying reflection, deflection or embed state.
            Vector3 destination =
                origin + direction * actualTravel;

            if (debugSettings != null &&
                debugSettings.DrawCastSweeps)
            {
                RVDebugDraw.Line(
                    origin,
                    destination,
                    debugSettings.ReturnPathColor,
                    0.1f);
            }

            transform.position = destination;

            if (WeaponCollisionUtility.HasBlockingOverlap(
                    transform.position,
                    tuning.CollisionRadius,
                    tuning.CollisionMask,
                    AttackPhase.Recall,
                    overlapBuffer,
                    transform,
                    weapon.Owner))
            {
                transform.position = lastSafePosition;
                active = false;
                accumulator = 0f;
                localImpactPause = 0f;
                weapon.MarkEmbedded();
                RecallBlocked?.Invoke();
                return;
            }

            lastSafePosition = transform.position;

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
                        AttackPhase.Recall,
                        overlapBuffer,
                        transform,
                        weapon.Owner))
                {
                    transform.position = separatedPosition;
                    lastSafePosition = transform.position;
                }

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

                active = false;
                accumulator = 0f;
                localImpactPause = 0f;
                weapon.MarkEmbedded();
                RecallBlocked?.Invoke();
                return;
            }

            if (reflected)
            {
                direction =
                    reflectionResponse.OutgoingDirection;
                speed *= reflectionResponse.SpeedRetention;

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
                    transform.position +
                    direction * separation;

                if (!WeaponCollisionUtility.HasBlockingOverlap(
                        separatedPosition,
                        tuning.CollisionRadius,
                        tuning.CollisionMask,
                        AttackPhase.Recall,
                        overlapBuffer,
                        transform,
                        weapon.Owner))
                {
                    transform.position = separatedPosition;
                    lastSafePosition = transform.position;
                }

                if (direction.sqrMagnitude > 0.0001f)
                {
                    transform.rotation =
                        Quaternion.LookRotation(
                            direction,
                            Vector3.up);
                }

                return;
            }

            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.rotation =
                    Quaternion.LookRotation(
                        direction,
                        Vector3.up);
            }

            if (pausedByTargetHit)
            {
                accumulator = 0f;
            }

            Transform liveTarget = CatchTarget;
            if (liveTarget != null &&
                Vector3.Distance(
                    transform.position,
                    liveTarget.position) <=
                tuning.CatchRadius &&
                CanCatchDirectly(liveTarget))
            {
                BeginCatch();
            }
        }

        /// <summary>
        /// Checks whether the weapon has a clear final path to the catch point.
        /// </summary>
        private bool CanCatchDirectly(Transform target)
        {
            if (target == null ||
                weapon == null ||
                tuning == null)
            {
                return false;
            }

            return !WeaponCollisionUtility.HasBlockingPath(
                transform.position,
                target.position,
                tuning.CollisionRadius,
                tuning.CollisionMask,
                AttackPhase.Recall,
                hitBuffer,
                transform,
                weapon.Owner);
        }

        /// <summary>
        /// Publishes the resolved surface interaction for feedback and diagnostics.
        /// </summary>
        private void EmitSurfaceInteraction(
            WeaponSurfaceKind kind,
            RaycastHit hit,
            Vector3 incoming,
            Vector3 outgoing)
        {
            SurfaceInteracted?.Invoke(
                new WeaponSurfaceInteractionInfo(
                    kind,
                    AttackPhase.Recall,
                    hit.point,
                    hit.normal,
                    incoming,
                    outgoing,
                    hit.collider));
        }

        /// <summary>
        /// Draws the normal.
        /// </summary>
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

        // Catching is a short authored snap from the live weapon position to the moving hand anchor.
        /// <summary>
        /// Starts the catch.
        /// </summary>
        private void BeginCatch()
        {
            if (weapon == null ||
                weapon.State != WeaponState.Returning)
            {
                return;
            }

            if (!weapon.BeginCatch())
            {
                return;
            }

            catchElapsed = 0f;
            catchStartPosition = transform.position;
            catchStartRotation = transform.rotation;
            accumulator = 0f;
            localImpactPause = 0f;
            CatchStarted?.Invoke();

            if (tuning.CatchSeconds <= 0f)
            {
                CompleteCatchImmediately();
            }
        }

        /// <summary>
        /// Advances the the catch state for the current frame.
        /// </summary>
        private void TickCatch(float deltaTime)
        {
            Transform target = CatchTarget;
            if (target == null)
            {
                CompleteCatchImmediately();
                return;
            }

            catchElapsed += deltaTime;
            float normalized =
                tuning.CatchSeconds <= 0f
                    ? 1f
                    : catchElapsed / tuning.CatchSeconds;

            float eased =
                RecallTravelMath.CatchEase(normalized);

            transform.position = Vector3.Lerp(
                catchStartPosition,
                target.position,
                eased);

            transform.rotation = Quaternion.Slerp(
                catchStartRotation,
                target.rotation,
                eased);

            if (normalized >= 1f)
            {
                CompleteCatchImmediately();
            }
        }

        /// <summary>
        /// Completes the catch immediately and updates the owning state.
        /// </summary>
        private void CompleteCatchImmediately()
        {
            Transform target = CatchTarget;
            if (target != null)
            {
                transform.SetPositionAndRotation(
                    target.position,
                    target.rotation);
            }

            if (weapon != null &&
                weapon.State == WeaponState.Catching)
            {
                weapon.CompleteCatch();
            }

            active = false;
            accumulator = 0f;
            localImpactPause = 0f;
            catchElapsed = 0f;
            damagedColliderIds.Clear();
            passedSurfaceColliderIds.Clear();
            CatchCompleted?.Invoke();
        }
    }
}
