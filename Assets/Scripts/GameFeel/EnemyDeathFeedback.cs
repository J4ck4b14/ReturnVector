using System.Collections;
using ReturnVector.Combat;
using ReturnVector.Enemies;
using UnityEngine;

// Script summary: Plays the enemy fall and leaves a flat material-matched splatter on the floor.

namespace ReturnVector.GameFeel
{
    /// <summary>
    /// Plays the enemy fall and leaves a flat material-matched splatter on the floor.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyDeathFeedback : MonoBehaviour
    {
        // Feedback variables
        private const float FallSeconds = 0.34f;
        private const float FallAngle = 82f;
        private const float GroundProbeHeight = 2f;
        private const float GroundProbeDistance = 5f;
        private const int GroundHitBufferSize = 12;

        // Visual variables
        [SerializeField] private EnemyHealth health;
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Material splatterMaterial;

        // Feedback variables
        private readonly RaycastHit[] groundHits =
            new RaycastHit[GroundHitBufferSize];

        private bool dying;

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(
            EnemyHealth newHealth,
            Renderer[] newRenderers,
            Material newSplatterMaterial)
        {
            Unsubscribe();

            health = newHealth;
            renderers = newRenderers ?? new Renderer[0];
            splatterMaterial = newSplatterMaterial;

            Subscribe();
        }

        /// <summary>
        /// Sets the splatter material.
        /// </summary>
        public void SetSplatterMaterial(Material material)
        {
            splatterMaterial = material;
        }

        /// <summary>
        /// Subscribes to runtime events when the component becomes active.
        /// </summary>
        private void OnEnable()
        {
            Subscribe();
        }

        /// <summary>
        /// Unsubscribes from runtime events when the component is disabled.
        /// </summary>
        private void OnDisable()
        {
            Unsubscribe();
        }

        /// <summary>
        /// Subscribes to the runtime events used by this component.
        /// </summary>
        private void Subscribe()
        {
            if (health != null)
            {
                health.Died -= HandleDeath;
                health.Died += HandleDeath;
            }
        }

        /// <summary>
        /// Unsubscribes from the runtime events used by this component.
        /// </summary>
        private void Unsubscribe()
        {
            if (health != null)
            {
                health.Died -= HandleDeath;
            }
        }

        /// <summary>
        /// Responds when the tracked enemy dies.
        /// </summary>
        private void HandleDeath(DamageInfo damage)
        {
            if (dying)
            {
                return;
            }

            dying = true;
            CreateSplatter();
            StartCoroutine(PlayFall(damage.Direction));
        }

        /// <summary>
        /// Plays the fall.
        /// </summary>
        private IEnumerator PlayFall(Vector3 hitDirection)
        {
            Vector3 fallDirection = hitDirection;
            fallDirection.y = 0f;

            if (fallDirection.sqrMagnitude < 0.0001f)
            {
                fallDirection = transform.forward;
            }

            fallDirection.Normalize();

            Vector3 fallAxis =
                Vector3.Cross(
                    Vector3.up,
                    fallDirection);

            if (fallAxis.sqrMagnitude < 0.0001f)
            {
                fallAxis = Vector3.right;
            }

            fallAxis.Normalize();

            Vector3 startPosition = transform.position;
            Quaternion startRotation = transform.rotation;

            float scale = Mathf.Max(
                0.6f,
                Mathf.Max(
                    transform.lossyScale.x,
                    transform.lossyScale.z));

            Vector3 endPosition =
                startPosition +
                fallDirection * (0.28f * scale) +
                Vector3.down * (0.34f * scale);

            Quaternion endRotation =
                Quaternion.AngleAxis(
                    FallAngle,
                    fallAxis) *
                startRotation;

            float elapsed = 0f;

            while (elapsed < FallSeconds)
            {
                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(
                    elapsed / FallSeconds);

                float eased =
                    1f -
                    Mathf.Pow(1f - t, 3f);

                transform.SetPositionAndRotation(
                    Vector3.Lerp(
                        startPosition,
                        endPosition,
                        eased),
                    Quaternion.Slerp(
                        startRotation,
                        endRotation,
                        eased));

                yield return null;
            }

            SetRenderersVisible(false);
            Destroy(gameObject);
        }

        /// <summary>
        /// Creates the splatter.
        /// </summary>
        private void CreateSplatter()
        {
            Vector3 groundPoint = transform.position;
            Vector3 groundNormal = Vector3.up;

            TryFindGround(
                ref groundPoint,
                ref groundNormal);

            Transform parent = transform.parent;

            GameObject splatter =
                new GameObject($"DeathSplatter_{name}");

            splatter.transform.SetParent(parent);
            splatter.transform.position =
                groundPoint + groundNormal * 0.012f;
            splatter.transform.rotation =
                Quaternion.FromToRotation(
                    Vector3.up,
                    groundNormal);

            float scale = Mathf.Max(
                0.75f,
                Mathf.Max(
                    transform.lossyScale.x,
                    transform.lossyScale.z));

            CreateSplatterPiece(
                splatter.transform,
                "Splatter_01",
                Vector3.zero,
                new Vector3(0.42f, 0.008f, 0.34f) * scale);

            CreateSplatterPiece(
                splatter.transform,
                "Splatter_02",
                new Vector3(0.31f, 0.004f, 0.15f) * scale,
                new Vector3(0.19f, 0.006f, 0.15f) * scale);

            CreateSplatterPiece(
                splatter.transform,
                "Splatter_03",
                new Vector3(-0.27f, 0.003f, -0.18f) * scale,
                new Vector3(0.16f, 0.005f, 0.21f) * scale);
        }

        /// <summary>
        /// Attempts to find the floor point used for the enemy death splatter.
        /// </summary>
        private void TryFindGround(
            ref Vector3 point,
            ref Vector3 normal)
        {
            Vector3 origin =
                transform.position +
                Vector3.up * GroundProbeHeight;

            int hitCount = Physics.RaycastNonAlloc(
                origin,
                Vector3.down,
                groundHits,
                GroundProbeDistance,
                ~0,
                QueryTriggerInteraction.Ignore);

            float nearestDistance = float.PositiveInfinity;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = groundHits[i];

                if (hit.collider == null ||
                    hit.collider.transform == transform ||
                    hit.collider.transform.IsChildOf(transform))
                {
                    continue;
                }

                if (hit.distance >= nearestDistance)
                {
                    continue;
                }

                nearestDistance = hit.distance;
                point = hit.point;
                normal = hit.normal.sqrMagnitude > 0.0001f
                    ? hit.normal.normalized
                    : Vector3.up;
            }

            if (float.IsPositiveInfinity(nearestDistance))
            {
                point.y -= 0.92f * transform.lossyScale.y;
            }
        }

        /// <summary>
        /// Creates the splatter piece.
        /// </summary>
        private void CreateSplatterPiece(
            Transform parent,
            string pieceName,
            Vector3 localPosition,
            Vector3 localScale)
        {
            GameObject piece =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cylinder);

            piece.name = pieceName;
            piece.transform.SetParent(parent, false);
            piece.transform.localPosition = localPosition;
            piece.transform.localRotation =
                Quaternion.Euler(
                    0f,
                    localPosition.x * 83f +
                    localPosition.z * 47f,
                    0f);
            piece.transform.localScale = localScale;

            Renderer renderer =
                piece.GetComponent<Renderer>();

            if (renderer != null &&
                splatterMaterial != null)
            {
                renderer.sharedMaterial = splatterMaterial;
            }

            Collider collider =
                piece.GetComponent<Collider>();

            if (collider != null)
            {
                collider.enabled = false;
                Destroy(collider);
            }
        }

        /// <summary>
        /// Sets the renderers visible.
        /// </summary>
        private void SetRenderersVisible(bool visible)
        {
            if (renderers == null)
            {
                return;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].enabled = visible;
                }
            }
        }
    }
}
