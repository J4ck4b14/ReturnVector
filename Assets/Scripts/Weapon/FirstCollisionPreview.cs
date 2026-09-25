using ReturnVector.Player;
using UnityEngine;

// Script summary: Shows the straight outbound segment up to the first relevant interaction.

namespace ReturnVector.Weapon
{
    /// <summary>
    /// Shows the straight outbound segment up to the first relevant interaction.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public sealed class FirstCollisionPreview : MonoBehaviour
    {
        // Collision variables
        private const int HitBufferSize = 32;

        // Weapon variables
        [SerializeField] private WeaponController weapon;
        [SerializeField] private WorldAimProvider aim;
        [SerializeField] private WeaponThrowTuning tuning;
        [SerializeField] private LineRenderer line;

        // Collision variables
        private readonly RaycastHit[] hitBuffer = new RaycastHit[HitBufferSize];

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(
            WeaponController newWeapon,
            WorldAimProvider newAim,
            WeaponThrowTuning newTuning,
            LineRenderer newLine)
        {
            weapon = newWeapon;
            aim = newAim;
            tuning = newTuning;
            line = newLine != null ? newLine : GetComponent<LineRenderer>();
            ApplyLineSettings();
        }

        /// <summary>
        /// Caches required references and prepares runtime state before the object starts running.
        /// </summary>
        private void Awake()
        {
            if (line == null)
            {
                line = GetComponent<LineRenderer>();
            }

            ApplyLineSettings();
        }

        /// <summary>
        /// Updates presentation after regular frame logic has completed.
        /// </summary>
        private void LateUpdate()
        {
            if (weapon == null ||
                aim == null ||
                tuning == null ||
                line == null)
            {
                SetVisible(false);
                return;
            }

            bool shouldShow =
                weapon.State == WeaponState.Held ||
                weapon.State == WeaponState.ThrowAnticipation;

            if (!shouldShow)
            {
                SetVisible(false);
                return;
            }

            Vector3 origin = weapon.transform.position;
            if (!aim.TryGetAim(origin, out _, out Vector3 direction))
            {
                SetVisible(false);
                return;
            }

            Vector3 end = FindPreviewEnd(origin, direction);

            line.positionCount = 2;
            line.SetPosition(0, origin);
            line.SetPosition(1, end);
            SetVisible(true);
        }

        /// <summary>
        /// Finds the first interaction point used by the outbound preview line.
        /// </summary>
        private Vector3 FindPreviewEnd(
            Vector3 origin,
            Vector3 direction)
        {
            int hitCount = Physics.SphereCastNonAlloc(
                origin,
                tuning.CollisionRadius,
                direction,
                hitBuffer,
                tuning.MaxDistance,
                tuning.CollisionMask,
                QueryTriggerInteraction.Ignore);

            WeaponCollisionUtility.SortHitsByDistance(
                hitBuffer,
                hitCount);

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = hitBuffer[i];

                if (WeaponCollisionUtility.IsOwnedCollider(
                        hit.collider,
                        weapon.transform,
                        weapon.Owner))
                {
                    continue;
                }

                return origin + direction * hit.distance;
            }

            return origin + direction * tuning.MaxDistance;
        }

        /// <summary>
        /// Applies material and width settings to the preview LineRenderer.
        /// </summary>
        private void ApplyLineSettings()
        {
            if (line == null || tuning == null)
            {
                return;
            }

            line.useWorldSpace = true;
            line.startWidth = tuning.PreviewWidth;
            line.endWidth = tuning.PreviewWidth;
            line.positionCount = 0;
        }

        /// <summary>
        /// Sets the visible.
        /// </summary>
        private void SetVisible(bool visible)
        {
            if (line != null)
            {
                line.enabled = visible;
            }
        }
    }
}
