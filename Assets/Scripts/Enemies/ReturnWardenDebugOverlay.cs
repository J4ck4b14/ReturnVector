using ReturnVector.Weapon;
using UnityEngine;

namespace ReturnVector.Enemies
{
    /// <summary>
    /// Shows live Return Warden state and pin progress while testing.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ReturnWardenDebugOverlay : MonoBehaviour
    {
        [SerializeField] private ReturnWardenHealth bossHealth;
        [SerializeField] private ReturnWardenAI bossAI;
        [SerializeField] private WeaponRecallConstraint recallConstraint;

        public void Configure(
            ReturnWardenHealth newHealth,
            ReturnWardenAI newAI,
            WeaponRecallConstraint newConstraint)
        {
            bossHealth = newHealth;
            bossAI = newAI;
            recallConstraint = newConstraint;
        }

        private void OnGUI()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (bossHealth == null ||
                !bossHealth.CanReceiveDamage)
            {
                return;
            }

            GUILayout.BeginArea(
                new Rect(
                    Screen.width * 0.5f - 220f,
                    12f,
                    440f,
                    118f),
                GUI.skin.box);

            GUILayout.Label(
                "RETURN WARDEN");

            GUILayout.Label(
                $"HP: {bossHealth.CurrentHealth:0.0} / " +
                $"{bossHealth.MaxHealth:0.0} " +
                $"({bossHealth.HealthRatio * 100f:0}%)");

            if (bossAI != null)
            {
                GUILayout.Label(
                    $"State: {bossAI.State} | " +
                    $"{(bossAI.IsPhaseTwo ? "Phase II" : "Phase I")}");
            }

            if (recallConstraint != null &&
                recallConstraint.IsPinned)
            {
                GUILayout.Label(
                    $"WEAPON PINNED — move " +
                    $"{recallConstraint.RepositionDistance:0.00} / " +
                    $"{recallConstraint.RequiredRepositionDistance:0.00}m " +
                    "to break the anchor");
            }

            GUILayout.EndArea();
#endif
        }
    }
}
