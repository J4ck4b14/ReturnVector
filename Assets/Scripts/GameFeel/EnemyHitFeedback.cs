using ReturnVector.Combat;
using ReturnVector.Enemies;
using UnityEngine;

namespace ReturnVector.GameFeel
{
    /// <summary>
    /// Applies phase-sensitive hit feedback to an enemy renderer.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyHitFeedback : MonoBehaviour
    {
        [SerializeField] private EnemyHealth health;
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
