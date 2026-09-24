using UnityEngine;

namespace ReturnVector.Enemies
{
    /// <summary>
    /// Simple runtime telegraph used by Rusher and Controller attacks.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyTelegraphVisual : MonoBehaviour
    {
        [SerializeField] private RusherEnemyAI rusher;
        [SerializeField] private ControllerEnemyAI controller;
        [SerializeField] private Transform marker;
        [SerializeField, Min(0f)] private float pulseSpeed = 8f;
        [SerializeField, Min(0f)] private float pulseAmount = 0.22f;

        private Vector3 baseScale;

        public void Configure(
            Transform newMarker,
            RusherEnemyAI newRusher = null,
            ControllerEnemyAI newController = null)
        {
            marker = newMarker;
            rusher = newRusher;
            controller = newController;

            if (marker != null)
            {
                baseScale = marker.localScale;
            }
        }

        private void Awake()
        {
            if (marker != null)
            {
                baseScale = marker.localScale;
            }
        }

        private void Update()
        {
            if (marker == null)
            {
                return;
            }

            bool active =
                (rusher != null &&
                 rusher.State == RusherEnemyState.Windup) ||
                (controller != null &&
                 controller.IsTelegraphing);

            marker.gameObject.SetActive(active);

            if (!active)
            {
                return;
            }

            float pulse =
                1f +
                Mathf.Sin(Time.time * pulseSpeed) *
                pulseAmount;

            marker.localScale =
                baseScale * pulse;
        }
    }
}
