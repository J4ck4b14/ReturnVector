using UnityEngine;

// Script summary: Shows the Warden pin around the resting weapon and its release progress.

namespace ReturnVector.Weapon
{
    /// <summary>
    /// Shows the Warden pin around the resting weapon and its release progress.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WeaponPinVisual : MonoBehaviour
    {
        // Weapon variables
        [SerializeField] private WeaponRecallConstraint constraint;
        [SerializeField] private Transform marker;
        [SerializeField, Min(0f)] private float pulseSpeed = 8f;
        [SerializeField, Min(0f)] private float pulseAmount = 0.12f;

        private Vector3 baseScale;

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(
            WeaponRecallConstraint newConstraint,
            Transform newMarker)
        {
            constraint = newConstraint;
            marker = newMarker;

            if (marker != null)
            {
                baseScale =
                    marker.localScale;

                marker.gameObject.SetActive(
                    constraint != null &&
                    constraint.IsPinned);
            }
        }

        /// <summary>
        /// Caches required references and prepares runtime state before the object starts running.
        /// </summary>
        private void Awake()
        {
            if (marker != null)
            {
                baseScale =
                    marker.localScale;
            }
        }

        /// <summary>
        /// Advances the component for the current frame.
        /// </summary>
        private void Update()
        {
            if (marker == null ||
                constraint == null)
            {
                return;
            }

            bool active =
                constraint.IsPinned;

            if (marker.gameObject.activeSelf != active)
            {
                marker.gameObject.SetActive(active);
            }

            if (!active)
            {
                return;
            }

            float progress =
                constraint.RepositionProgress;

            float pulse =
                1f +
                Mathf.Sin(
                    Time.time *
                    pulseSpeed) *
                pulseAmount;

            float tension =
                Mathf.Lerp(
                    1.15f,
                    0.82f,
                    progress);

            marker.localScale =
                baseScale *
                pulse *
                tension;
        }
    }
}
