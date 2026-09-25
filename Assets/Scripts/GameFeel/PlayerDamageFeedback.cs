using ReturnVector.Combat;
using ReturnVector.Player;
using UnityEngine;

// Script summary: Routes player damage into short renderer feedback.

namespace ReturnVector.GameFeel
{
    /// <summary>
    /// Routes player damage into short renderer feedback.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerDamageFeedback : MonoBehaviour
    {
        // Feedback variables
        [SerializeField] private PlayerHealth health;
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
            PlayerHealth newHealth,
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

            flash.Flash(
                profile.PlayerDamageFlashSeconds,
                new Color(
                    1f,
                    0.25f,
                    0.25f,
                    1f));
        }
    }
}
