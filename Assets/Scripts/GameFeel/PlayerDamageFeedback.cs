using ReturnVector.Combat;
using ReturnVector.Player;
using UnityEngine;

namespace ReturnVector.GameFeel
{
    /// <summary>
    /// Routes player damage into short renderer feedback.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerDamageFeedback : MonoBehaviour
    {
        [SerializeField] private PlayerHealth health;
        [SerializeField] private RVRendererFlash flash;
        [SerializeField] private RVGameFeelProfile profile;

        private void OnEnable()
        {
            if (health != null)
            {
                health.Damaged +=
                    HandleDamaged;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Damaged -=
                    HandleDamaged;
            }
        }

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
