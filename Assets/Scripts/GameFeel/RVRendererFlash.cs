using UnityEngine;

namespace ReturnVector.GameFeel
{
    /// <summary>
    /// Short MaterialPropertyBlock flash used for impact feedback.
    /// </summary>
    public sealed class RVRendererFlash : MonoBehaviour
    {
        [SerializeField] private Renderer[] renderers;

        private MaterialPropertyBlock block;
        private float remaining;
        private float duration;
        private Color flashColor = Color.white;

        public void Configure(
            Renderer[] newRenderers)
        {
            renderers =
                newRenderers ?? new Renderer[0];
        }

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

        private void OnDisable()
        {
            Apply(0f);
        }

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
