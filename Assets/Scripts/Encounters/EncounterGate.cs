using UnityEngine;

namespace ReturnVector.Encounters
{
    /// <summary>
    /// Controls the physical gate used to contain or release an encounter.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EncounterGate : MonoBehaviour
    {
        [SerializeField] private Collider blockingCollider;
        [SerializeField] private Renderer gateRenderer;
        [SerializeField] private bool startsOpen;

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            SetOpen(startsOpen);
        }

        public void Configure(
            Collider newBlockingCollider,
            Renderer newRenderer,
            bool openInitially)
        {
            blockingCollider = newBlockingCollider;
            gateRenderer = newRenderer;
            startsOpen = openInitially;
            SetOpen(openInitially);
        }

        public void SetOpen(bool open)
        {
            IsOpen = open;

            if (blockingCollider != null)
            {
                blockingCollider.enabled = !open;
            }

            if (gateRenderer != null)
            {
                gateRenderer.enabled = !open;
            }
        }
    }
}
