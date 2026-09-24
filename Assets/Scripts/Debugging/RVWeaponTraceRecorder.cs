using System.Collections.Generic;
using ReturnVector.Weapon;
using UnityEngine;

namespace ReturnVector.Debugging
{
    /// <summary>
    /// Records the path the weapon actually travelled for comparison with route prediction.
    /// </summary>
    public sealed class RVWeaponTraceRecorder : MonoBehaviour
    {
        [SerializeField] private WeaponController weapon;
        [SerializeField] private RVDebugSettings settings;

        private readonly List<RVWeaponTraceSample> samples =
            new List<RVWeaponTraceSample>(256);

        private Vector3 lastSamplePosition;
        private bool hasLastSample;

        public IReadOnlyList<RVWeaponTraceSample> Samples => samples;
        public int SampleCount => samples.Count;

        private void OnEnable()
        {
            if (weapon != null)
            {
                weapon.StateChanged += HandleStateChanged;
            }
        }

        private void OnDisable()
        {
            if (weapon != null)
            {
                weapon.StateChanged -= HandleStateChanged;
            }
        }

        public void Configure(
            WeaponController newWeapon,
            RVDebugSettings newSettings)
        {
            if (isActiveAndEnabled && weapon != null)
            {
                weapon.StateChanged -= HandleStateChanged;
            }

            weapon = newWeapon;
            settings = newSettings;

            if (isActiveAndEnabled && weapon != null)
            {
                weapon.StateChanged += HandleStateChanged;
            }
        }

        public void ClearTrace()
        {
            samples.Clear();
            hasLastSample = false;
        }

        private void HandleStateChanged(
            WeaponState previous,
            WeaponState next)
        {
            if (next == WeaponState.ThrowAnticipation &&
                settings != null &&
                settings.ClearTraceOnNewThrow)
            {
                ClearTrace();
            }

            if (next != WeaponState.Held)
            {
                AddSample(force: true);
            }
            else if (previous == WeaponState.Catching)
            {
                AddSample(force: true);
            }
        }

        private void LateUpdate()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (weapon == null ||
                settings == null ||
                !settings.RecordActualWeaponTrace)
            {
                return;
            }

            WeaponState state = weapon.State;
            if (state == WeaponState.Held ||
                state == WeaponState.Disabled)
            {
                return;
            }

            AddSample(force: false);
#endif
        }

        private void AddSample(bool force)
        {
            if (weapon == null)
            {
                return;
            }

            float spacing =
                settings != null
                    ? settings.TraceSampleSpacing
                    : 0.12f;

            Vector3 position = weapon.transform.position;

            if (!force &&
                hasLastSample &&
                Vector3.Distance(
                    lastSamplePosition,
                    position) < spacing)
            {
                return;
            }

            int capacity =
                settings != null
                    ? settings.MaxTraceSamples
                    : 256;

            if (samples.Count >= capacity)
            {
                samples.RemoveAt(0);
            }

            samples.Add(
                new RVWeaponTraceSample(
                    position,
                    weapon.State,
                    Time.unscaledTime));

            lastSamplePosition = position;
            hasLastSample = true;
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (settings == null ||
                !settings.DrawActualWeaponTrace ||
                samples.Count < 2)
            {
                return;
            }

            for (int i = 1; i < samples.Count; i++)
            {
                RVWeaponTraceSample from = samples[i - 1];
                RVWeaponTraceSample to = samples[i];

                Gizmos.color = IsRecallState(to.State)
                    ? settings.ReturnPathColor
                    : settings.OutboundPathColor;

                Gizmos.DrawLine(from.Position, to.Position);
            }
#endif
        }

        private static bool IsRecallState(WeaponState state)
        {
            return state == WeaponState.Returning ||
                   state == WeaponState.Catching;
        }
    }
}
