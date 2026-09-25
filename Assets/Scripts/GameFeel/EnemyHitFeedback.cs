using ReturnVector.Combat;
using ReturnVector.Enemies;
using UnityEngine;

// Script summary: Applies phase-sensitive hit feedback to an enemy renderer.

namespace ReturnVector.GameFeel
{
    /// <summary>
    /// Applies phase-sensitive hit feedback to an enemy renderer.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyHitFeedback : MonoBehaviour
    {
        // Feedback variables
        [SerializeField] private EnemyHealth health;
        [SerializeField] private RVRendererFlash flash;
        [SerializeField] private RVGameFeelProfile profile;

        /// <summary>
        /// Subscribes to runtime events when the component becomes active.
        /// </summary>
        private void OnEnable()
        {
            if (health != null)
            {
                health.Damaged +=
                    HandleDamaged;
            }
        }

        /// <summary>
        /// Unsubscribes from runtime events when the component is disabled.
        /// </summary>
        private void OnDisable()
        {
            if (health != null)
            {
                health.Damaged -=
                    HandleDamaged;
            }
        }

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(
            EnemyHealth newHealth,
            RVRendererFlash newFlash,
            RVGameFeelProfile newProfile)
        {
            if (isActiveAndEnabled &&
                health != null)
            {
                health.Damaged -=
                    HandleDamaged;
            }

            health = newHealth;
            flash = newFlash;
            profile = newProfile;

            if (isActiveAndEnabled &&
                health != null)
            {
                health.Damaged +=
                    HandleDamaged;
            }
        }

        /// <summary>
        /// Responds when the tracked target takes damage.
        /// </summary>
        private void HandleDamaged(
            float currentHealth,
            DamageInfo damage)
        {
            if (flash == null ||
                profile == null)
            {
                return;
            }

            Color color =
                damage.Phase == AttackPhase.Recall
                    ? new Color(
                        0.55f,
                        0.95f,
                        1f,
                        1f)
                    : new Color(
                        1f,
                        0.75f,
                        0.55f,
                        1f);

            flash.Flash(
                profile.EnemyFlashSeconds,
                color);
        }
    }
}
