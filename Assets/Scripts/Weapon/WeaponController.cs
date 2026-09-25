using System;
using ReturnVector.Debugging;
using UnityEngine;

// Script summary: Runtime owner of the weapon lifecycle. Motion systems are separate and may only act while the corresponding explicit state is active.

namespace ReturnVector.Weapon
{
    /// <summary>
    /// Runtime owner of the weapon lifecycle.
    /// Motion systems are separate and may only act while the corresponding explicit state is active.
    /// </summary>
    public sealed class WeaponController : MonoBehaviour
    {
        // Collision variables
        private const int HeldHitBufferSize = 24;
        private const int HeldOverlapBufferSize = 24;

        // Weapon variables
        [SerializeField] private Transform owner;
        [SerializeField] private Transform heldAnchor;
        [SerializeField] private RVDebugSettings debugSettings;

        // Collision variables
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

        /// <summary>
        /// Caches required references and prepares runtime state before the object starts running.
        /// </summary>
        private void Awake()
        {
            EnsureStateMachine();
        }

        /// <summary>
        /// Subscribes to runtime events when the component becomes active.
        /// </summary>
        private void OnEnable()
        {
            EnsureStateMachine();
            stateMachine.Transitioned += HandleTransition;
            SnapToHeldAnchorIfNeeded();
        }

        /// <summary>
        /// Unsubscribes from runtime events when the component is disabled.
        /// </summary>
        private void OnDisable()
        {
            if (stateMachine != null)
            {
                stateMachine.Transitioned -= HandleTransition;
            }
        }

        /// <summary>
        /// Updates presentation after regular frame logic has completed.
        /// </summary>
        private void LateUpdate()
        {
            SnapToHeldAnchorIfNeeded();
        }

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(
            Transform newOwner,
            RVDebugSettings settings)
        {
            Configure(newOwner, null, settings);
        }

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
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

        /// <summary>
        /// Copies throw collision settings used to keep the held baton outside solid geometry.
        /// </summary>
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

        /// <summary>
        /// Validates and resolves the held baton position before starting a throw.
        /// </summary>
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

        /// <summary>
        /// Starts the throw anticipation.
        /// </summary>
        public bool BeginThrowAnticipation() =>
            TrySetState(WeaponState.ThrowAnticipation);

        /// <summary>
        /// Returns the launch outbound.
        /// </summary>
        public bool LaunchOutbound() =>
            TrySetState(WeaponState.Outbound);

        /// <summary>
        /// Returns the mark parked.
        /// </summary>
        public bool MarkParked() =>
            TrySetState(WeaponState.Parked);

        /// <summary>
        /// Moves the weapon into the embedded state after an authored blocking interaction.
        /// </summary>
        public bool MarkEmbedded() =>
            TrySetState(WeaponState.Embedded);

        /// <summary>
        /// Starts the recall.
        /// </summary>
        public bool BeginRecall() =>
            TrySetState(WeaponState.Returning);

        /// <summary>
        /// Starts the catch.
        /// </summary>
        public bool BeginCatch() =>
            TrySetState(WeaponState.Catching);

        /// <summary>
        /// Completes the catch and updates the owning state.
        /// </summary>
        public bool CompleteCatch() =>
            TrySetState(WeaponState.Held);

        /// <summary>
        /// Resets the to held.
        /// </summary>
        public void ResetToHeld()
        {
            EnsureStateMachine();
            stateMachine.ResetToHeld();
            SnapToHeldAnchorIfNeeded();
        }

        /// <summary>
        /// Attempts to move the weapon into the requested lifecycle state.
        /// </summary>
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

        /// <summary>
        /// Responds to a weapon state transition.
        /// </summary>
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

        /// <summary>
        /// Creates the weapon state machine when it has not been initialized yet.
        /// </summary>
        private void EnsureStateMachine()
        {
            if (stateMachine == null)
            {
                stateMachine = new WeaponStateMachine();
            }
        }

        /// <summary>
        /// Keeps the held baton at the nearest reachable anchor position outside solid geometry.
        /// </summary>
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

        /// <summary>
        /// Draws Scene view gizmos while the object is selected.
        /// </summary>
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
