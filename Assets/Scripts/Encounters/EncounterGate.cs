using UnityEngine;

// Script summary: Controls the physical gate used to contain or release an encounter.

namespace ReturnVector.Encounters
{
    /// <summary>
    /// Controls the physical gate used to contain or release an encounter.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EncounterGate : MonoBehaviour
    {
        // Encounter variables
        [SerializeField] private Collider blockingCollider;
        [SerializeField] private Renderer gateRenderer;
        [SerializeField] private bool startsOpen;

        public bool IsOpen { get; private set; }

        /// <summary>
        /// Caches required references and prepares runtime state before the object starts running.
        /// </summary>
        private void Awake()
        {
            SetOpen(startsOpen);
        }

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
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

        /// <summary>
        /// Sets the open.
        /// </summary>
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
