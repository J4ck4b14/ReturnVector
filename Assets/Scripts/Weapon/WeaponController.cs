using System;
using ReturnVector.Debugging;
using UnityEngine;

namespace ReturnVector.Weapon
{
    /// <summary>
    /// Runtime owner of the weapon lifecycle.
    /// Motion systems are separate and may only act while the corresponding explicit state is active.
    /// </summary>
    public sealed class WeaponController : MonoBehaviour
    {
        private const int HeldHitBufferSize = 24;
        private const int HeldOverlapBufferSize = 24;

        [SerializeField] private Transform owner;
        [SerializeField] private Transform heldAnchor;
        [SerializeField] private RVDebugSettings debugSettings;

        private readonly RaycastHit[] heldHitBuffer =
            new RaycastHit[HeldHitBufferSize];
        private readonly Collider[] heldOverlapBuffer =
            new Collider[HeldOverlapBufferSize];

        private WeaponStateMachine stateMachine;
        private float heldCollisionRadius = 0.12f;
        private float heldSurfaceBackoff = 0.015f;
        private LayerMask heldCollisionMask = ~0;
        private bool heldCollisionConfigured;

        public Transform Owner => owner;
        public Transform HeldAnchor => heldAnchor;
        public WeaponState State =>
            stateMachine != null ? stateMachine.Current : WeaponState.Held;

        public event Action<WeaponState, WeaponState> StateChanged;

        private void Awake()
        {
            EnsureStateMachine();
        }

        private void OnEnable()
        {
            EnsureStateMachine();
            stateMachine.Transitioned += HandleTransition;
            SnapToHeldAnchorIfNeeded();
        }

        private void OnDisable()
        {
            if (stateMachine != null)
            {
                stateMachine.Transitioned -= HandleTransition;
            }
        }

        private void LateUpdate()
        {
            SnapToHeldAnchorIfNeeded();
        }

        public void Configure(
            Transform newOwner,
            RVDebugSettings settings)
        {
            Configure(newOwner, null, settings);
        }

        public void Configure(
            Transform newOwner,
            Transform newHeldAnchor,
            RVDebugSettings settings)
        {
            owner = newOwner;
            heldAnchor = newHeldAnchor;
            debugSettings = settings;
            SnapToHeldAnchorIfNeeded();
        }

        public void ConfigureHeldCollision(
            WeaponThrowTuning tuning)
        {
            if (tuning == null)
            {
                heldCollisionConfigured = false;
                return;
            }

            heldCollisionRadius =
                Mathf.Max(0.001f, tuning.CollisionRadius);
            heldSurfaceBackoff =
                Mathf.Max(0f, tuning.SurfaceBackoff);
            heldCollisionMask = tuning.CollisionMask;
            heldCollisionConfigured = true;

            SnapToHeldAnchorIfNeeded();
        }

        public bool PrepareOutboundLaunch()
        {
            WeaponState state = State;
            if (state != WeaponState.Held &&
                state != WeaponState.ThrowAnticipation)
            {
                return false;
            }

            return SnapToHeldAnchorIfNeeded();
        }

        public bool BeginThrowAnticipation() =>
            TrySetState(WeaponState.ThrowAnticipation);

        public bool LaunchOutbound() =>
            TrySetState(WeaponState.Outbound);

        public bool MarkParked() =>
            TrySetState(WeaponState.Parked);

        public bool MarkEmbedded() =>
            TrySetState(WeaponState.Embedded);

        public bool BeginRecall() =>
            TrySetState(WeaponState.Returning);

        public bool BeginCatch() =>
            TrySetState(WeaponState.Catching);

        public bool CompleteCatch() =>
            TrySetState(WeaponState.Held);

        public void ResetToHeld()
        {
            EnsureStateMachine();
            stateMachine.ResetToHeld();
            SnapToHeldAnchorIfNeeded();
        }

        private bool TrySetState(WeaponState next)
        {
            EnsureStateMachine();
            bool changed = stateMachine.TryTransition(next);

            if (!changed &&
                debugSettings != null &&
                debugSettings.LogRejectedStateTransitions)
            {
                UnityEngine.Debug.LogWarning(
                    $"[RETURN VECTOR] Rejected weapon transition {stateMachine.Current} -> {next}",
                    this);
            }

            return changed;
        }

        private void HandleTransition(
            WeaponState previous,
            WeaponState next)
        {
            if (debugSettings != null &&
                debugSettings.LogStateTransitions)
            {
                UnityEngine.Debug.Log(
                    $"[RETURN VECTOR] Weapon: {previous} -> {next}",
                    this);
            }

            if (next == WeaponState.Held ||
                next == WeaponState.ThrowAnticipation)
            {
                SnapToHeldAnchorIfNeeded();
            }

            StateChanged?.Invoke(previous, next);
        }

        private void EnsureStateMachine()
        {
            if (stateMachine == null)
            {
                stateMachine = new WeaponStateMachine();
            }
        }

        private bool SnapToHeldAnchorIfNeeded()
        {
            if (heldAnchor == null)
            {
                return false;
            }

            WeaponState state = State;
            if (state != WeaponState.Held &&
                state != WeaponState.ThrowAnticipation)
            {
                return true;
            }

            Vector3 desiredPosition =
                heldAnchor.position;
            Vector3 resolvedPosition =
                desiredPosition;

            if (heldCollisionConfigured &&
                owner != null)
            {
                Vector3 referencePosition =
                    owner.position;
                referencePosition.y =
                    desiredPosition.y;

                if (!WeaponCollisionUtility.TryResolveReachableWorldPosition(
                        referencePosition,
                        desiredPosition,
                        heldCollisionRadius,
                        heldSurfaceBackoff,
                        heldCollisionMask,
                        ReturnVector.Combat.AttackPhase.Outbound,
                        heldHitBuffer,
                        heldOverlapBuffer,
                        transform,
                        owner,
                        out resolvedPosition,
                        out _))
                {
                    return false;
                }
            }

            transform.SetPositionAndRotation(
                resolvedPosition,
                heldAnchor.rotation);
            return true;
        }

        private void OnDrawGizmosSelected()
        {
            if (owner == null ||
                debugSettings == null ||
                !debugSettings.DrawWeaponOwnerLine)
            {
                return;
            }

            Gizmos.color = debugSettings.OwnerLineColor;
            Gizmos.DrawLine(transform.position, owner.position);
        }
    }
}
