using UnityEngine;

namespace ReturnVector.Weapon
{
    /// <summary>
    /// Shows the Warden pin around the resting weapon and its release progress.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WeaponPinVisual : MonoBehaviour
    {
        [SerializeField] private WeaponRecallConstraint constraint;
        [SerializeField] private Transform marker;
        [SerializeField, Min(0f)] private float pulseSpeed = 8f;
        [SerializeField, Min(0f)] private float pulseAmount = 0.12f;

        private Vector3 baseScale;

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

        private void Awake()
        {
            if (marker != null)
            {
                baseScale =
                    marker.localScale;
            }
        }

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
