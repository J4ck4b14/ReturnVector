using UnityEngine;

// Script summary: Short MaterialPropertyBlock flash used for impact feedback.

namespace ReturnVector.GameFeel
{
    /// <summary>
    /// Short MaterialPropertyBlock flash used for impact feedback.
    /// </summary>
    public sealed class RVRendererFlash : MonoBehaviour
    {
        // Visual variables
        [SerializeField] private Renderer[] renderers;

        // Feedback variables
        private MaterialPropertyBlock block;
        private float remaining;
        private float duration;
        private Color flashColor = Color.white;

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(
            Renderer[] newRenderers)
        {
            renderers =
                newRenderers ?? new Renderer[0];
        }

        /// <summary>
        /// Starts a short renderer flash using the requested feedback colour and duration.
        /// </summary>
        public void Flash(
            float seconds,
            Color color)
        {
            if (seconds <= 0f)
            {
                return;
            }

            duration =
                Mathf.Max(duration, seconds);

            remaining =
                Mathf.Max(remaining, seconds);

            flashColor = color;
        }

        /// <summary>
        /// Caches required references and prepares runtime state before the object starts running.
        /// </summary>
        private void Awake()
        {
            block =
                new MaterialPropertyBlock();

            if (renderers == null ||
                renderers.Length == 0)
            {
                renderers =
                    GetComponentsInChildren<
                        Renderer>(true);
            }
        }

        /// <summary>
        /// Advances the component for the current frame.
        /// </summary>
        private void Update()
        {
            if (remaining <= 0f)
            {
                return;
            }

            remaining =
                Mathf.Max(
                    0f,
                    remaining -
                    Time.unscaledDeltaTime);

            float t =
                duration <= 0f
                    ? 0f
                    : remaining / duration;

            float strength =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t);

            Apply(strength);

            if (remaining <= 0f)
            {
                Apply(0f);
                duration = 0f;
            }
        }

        /// <summary>
        /// Unsubscribes from runtime events when the component is disabled.
        /// </summary>
        private void OnDisable()
        {
            Apply(0f);
        }

        /// <summary>
        /// Applies the configured surface or curvature response.
        /// </summary>
        private void Apply(float strength)
        {
            if (renderers == null)
            {
                return;
            }

            if (block == null)
            {
                block =
                    new MaterialPropertyBlock();
            }

            for (int i = 0;
                 i < renderers.Length;
                 i++)
            {
                Renderer renderer =
                    renderers[i];

                if (renderer == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(
                    block);

                if (renderer.sharedMaterial != null &&
                    renderer.sharedMaterial.HasProperty(
                        "_BaseColor"))
                {
                    Color baseColor =
                        renderer.sharedMaterial.GetColor(
                            "_BaseColor");

                    block.SetColor(
                        "_BaseColor",
                        Color.Lerp(
                            baseColor,
                            flashColor,
                            strength));
                }
                else if (renderer.sharedMaterial != null &&
                         renderer.sharedMaterial.HasProperty(
                             "_Color"))
                {
                    Color baseColor =
                        renderer.sharedMaterial.GetColor(
                            "_Color");

                    block.SetColor(
                        "_Color",
                        Color.Lerp(
                            baseColor,
                            flashColor,
                            strength));
                }

                renderer.SetPropertyBlock(
                    block);
            }
        }
    }
}
