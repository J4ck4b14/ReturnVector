using UnityEngine;

namespace ReturnVector.Enemies
{
    /// <summary>
    /// Visual warning for the Return Warden slam.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ReturnWardenTelegraphVisual : MonoBehaviour
    {
        [SerializeField] private ReturnWardenAI boss;
        [SerializeField] private Transform marker;
        [SerializeField, Min(0f)] private float pulseSpeed = 7f;
        [SerializeField, Min(0f)] private float pulseAmount = 0.18f;

        private Vector3 baseScale;

        public void Configure(
            ReturnWardenAI newBoss,
            Transform newMarker)
        {
            boss = newBoss;
            marker = newMarker;

            if (marker != null)
            {
                baseScale =
                    marker.localScale;
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
                boss == null)
            {
                return;
            }

            bool active =
                boss.IsTelegraphing;

            marker.gameObject.SetActive(
                active);

            if (!active)
            {
                return;
            }

            float pulse =
                1f +
                Mathf.Sin(
                    Time.time *
                    pulseSpeed) *
                pulseAmount;

            marker.localScale =
                baseScale * pulse;
        }
    }
}
