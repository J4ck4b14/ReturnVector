using System;
using System.Collections.Generic;
using System.Text;
using ReturnVector.Combat;
using ReturnVector.Surfaces;
using ReturnVector.Weapon;
using UnityEngine;

namespace ReturnVector.Debugging
{
    /// <summary>
    /// Subscribes to gameplay events and keeps a bounded, human-readable combat history.
    /// </summary>
    public sealed class RVCombatDebugRecorder : MonoBehaviour
    {
        [SerializeField] private RVDebugSettings settings;
        [SerializeField] private WeaponController weapon;
        [SerializeField] private OutboundWeaponMotor outboundMotor;
        [SerializeField] private RecallWeaponMotor recallMotor;

        private RVDebugEventBuffer buffer;
        private bool bound;
        private int sequence;

        public IReadOnlyList<RVDebugEvent> Events =>
            Buffer.Events;

        public int EventCount => Buffer.Count;
        public int OutboundImpacts { get; private set; }
        public int RecallImpacts { get; private set; }
        public int SurfaceResponses { get; private set; }
        public int SuccessfulCatches { get; private set; }
        public int BlockedRecalls { get; private set; }

        public RVDebugEvent? LastEvent =>
            Buffer.Count > 0
                ? Buffer.Events[Buffer.Count - 1]
                : (RVDebugEvent?)null;

        private RVDebugEventBuffer Buffer
        {
            get
            {
                if (buffer == null)
                {
                    int capacity =
                        settings != null
                            ? settings.EventHistoryCapacity
                            : 64;
                    buffer = new RVDebugEventBuffer(capacity);
                }

                return buffer;
            }
        }

        private void OnEnable()
        {
            Bind();
        }

        private void OnDisable()
        {
            Unbind();
        }

        public void Configure(
            RVDebugSettings newSettings,
            WeaponController newWeapon,
            OutboundWeaponMotor newOutboundMotor,
            RecallWeaponMotor newRecallMotor)
        {
            Unbind();

            settings = newSettings;
            weapon = newWeapon;
            outboundMotor = newOutboundMotor;
            recallMotor = newRecallMotor;

            Buffer.SetCapacity(
                settings != null
                    ? settings.EventHistoryCapacity
                    : 64);

            if (isActiveAndEnabled)
            {
                Bind();
            }
        }

        public void Clear()
        {
            Buffer.Clear();
            sequence = 0;
            OutboundImpacts = 0;
            RecallImpacts = 0;
            SurfaceResponses = 0;
            SuccessfulCatches = 0;
            BlockedRecalls = 0;
        }

        public string BuildTextReport()
        {
            StringBuilder builder = new StringBuilder(2048);
            builder.AppendLine("RETURN VECTOR — Combat Debug Report");
            builder.AppendLine(
                $"Outbound impacts: {OutboundImpacts}");
            builder.AppendLine(
                $"Recall impacts: {RecallImpacts}");
            builder.AppendLine(
                $"Surface responses: {SurfaceResponses}");
            builder.AppendLine(
                $"Blocked recalls: {BlockedRecalls}");
            builder.AppendLine(
                $"Successful catches: {SuccessfulCatches}");
            builder.AppendLine();

            IReadOnlyList<RVDebugEvent> events = Buffer.Events;
            for (int i = 0; i < events.Count; i++)
            {
                builder.AppendLine(events[i].ToString());
            }

            return builder.ToString();
        }

        private void Bind()
        {
            if (bound)
            {
                return;
            }

            if (weapon != null)
            {
                weapon.StateChanged += HandleStateChanged;
            }

            if (outboundMotor != null)
            {
                outboundMotor.Impacted += HandleOutboundImpact;
                outboundMotor.SurfaceInteracted += HandleSurfaceInteraction;
                outboundMotor.Parked += HandleOutboundParked;
            }

            if (recallMotor != null)
            {
                recallMotor.Impacted += HandleRecallImpact;
                recallMotor.SurfaceInteracted += HandleSurfaceInteraction;
                recallMotor.RecallStarted += HandleRecallStarted;
                recallMotor.RecallBlocked += HandleRecallBlocked;
                recallMotor.CatchStarted += HandleCatchStarted;
                recallMotor.CatchCompleted += HandleCatchCompleted;
            }

            bound = true;
        }

        private void Unbind()
        {
            if (!bound)
            {
                return;
            }

            if (weapon != null)
            {
                weapon.StateChanged -= HandleStateChanged;
            }

            if (outboundMotor != null)
            {
                outboundMotor.Impacted -= HandleOutboundImpact;
                outboundMotor.SurfaceInteracted -= HandleSurfaceInteraction;
                outboundMotor.Parked -= HandleOutboundParked;
            }

            if (recallMotor != null)
            {
                recallMotor.Impacted -= HandleRecallImpact;
                recallMotor.SurfaceInteracted -= HandleSurfaceInteraction;
                recallMotor.RecallStarted -= HandleRecallStarted;
                recallMotor.RecallBlocked -= HandleRecallBlocked;
                recallMotor.CatchStarted -= HandleCatchStarted;
                recallMotor.CatchCompleted -= HandleCatchCompleted;
            }

            bound = false;
        }

        private void HandleStateChanged(
            WeaponState previous,
            WeaponState next)
        {
            Record(
                RVDebugEventKind.StateTransition,
                AttackPhase.Unknown,
                weapon != null
                    ? weapon.transform.position
                    : Vector3.zero,
                $"{previous} -> {next}");
        }

        private void HandleOutboundImpact(
            WeaponImpactInfo impact)
        {
            OutboundImpacts++;

            Record(
                RVDebugEventKind.OutboundImpact,
                AttackPhase.Outbound,
                impact.Point,
                DescribeImpact(impact));
        }

        private void HandleRecallImpact(
            WeaponImpactInfo impact)
        {
            RecallImpacts++;

            Record(
                RVDebugEventKind.RecallImpact,
                AttackPhase.Recall,
                impact.Point,
                DescribeImpact(impact));
        }

        private void HandleSurfaceInteraction(
            WeaponSurfaceInteractionInfo info)
        {
            SurfaceResponses++;

            string colliderName =
                info.Collider != null
                    ? info.Collider.name
                    : "<none>";

            float turn =
                info.OutgoingDirection.sqrMagnitude > 0.0001f &&
                info.IncomingDirection.sqrMagnitude > 0.0001f
                    ? Vector3.Angle(
                        info.IncomingDirection,
                        info.OutgoingDirection)
                    : 0f;

            Record(
                RVDebugEventKind.SurfaceResponse,
                info.Phase,
                info.Point,
                $"{info.Kind} on {colliderName}, turn {turn:0.0}°");
        }

        private void HandleOutboundParked()
        {
            Record(
                RVDebugEventKind.OutboundParked,
                AttackPhase.Outbound,
                weapon != null
                    ? weapon.transform.position
                    : Vector3.zero,
                weapon != null
                    ? $"weapon state = {weapon.State}"
                    : "weapon parked");
        }

        private void HandleRecallStarted()
        {
            Record(
                RVDebugEventKind.RecallStarted,
                AttackPhase.Recall,
                weapon != null
                    ? weapon.transform.position
                    : Vector3.zero,
                "return simulation started");
        }

        private void HandleRecallBlocked()
        {
            BlockedRecalls++;

            Record(
                RVDebugEventKind.RecallBlocked,
                AttackPhase.Recall,
                weapon != null
                    ? weapon.transform.position
                    : Vector3.zero,
                "return stopped by blocking geometry");
        }

        private void HandleCatchStarted()
        {
            Record(
                RVDebugEventKind.CatchStarted,
                AttackPhase.Recall,
                weapon != null
                    ? weapon.transform.position
                    : Vector3.zero,
                "catch snap started");
        }

        private void HandleCatchCompleted()
        {
            SuccessfulCatches++;

            Record(
                RVDebugEventKind.CatchCompleted,
                AttackPhase.Recall,
                weapon != null
                    ? weapon.transform.position
                    : Vector3.zero,
                "weapon returned to Held");
        }

        private void Record(
            RVDebugEventKind kind,
            AttackPhase phase,
            Vector3 point,
            string detail)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            int capacity =
                settings != null
                    ? settings.EventHistoryCapacity
                    : 64;

            Buffer.SetCapacity(capacity);

            Buffer.Add(
                new RVDebugEvent(
                    ++sequence,
                    Time.unscaledTime,
                    kind,
                    phase,
                    point,
                    detail));
#endif
        }

        private static string DescribeImpact(
            WeaponImpactInfo impact)
        {
            string target =
                impact.Collider != null
                    ? impact.Collider.name
                    : "<none>";

            if (impact.DamagedTarget)
            {
                return $"damage hit: {target}";
            }

            return impact.Blocking
                ? $"blocking hit: {target}"
                : $"non-blocking hit: {target}";
        }
    }
}
