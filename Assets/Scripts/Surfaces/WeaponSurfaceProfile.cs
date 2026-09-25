using ReturnVector.Combat;
using UnityEngine;

// Script summary: Authored response data for outbound and recall interaction with a surface.

namespace ReturnVector.Surfaces
{
    [CreateAssetMenu(
        fileName = "SO_WeaponSurface",
        menuName = "RETURN VECTOR/Weapon Surface Profile")]
    /// <summary>
    /// Authored response data for outbound and recall interaction with a surface.
    /// </summary>
    public sealed class WeaponSurfaceProfile : ScriptableObject
    {
        // Surface variables
        [SerializeField] private WeaponSurfaceKind kind = WeaponSurfaceKind.Neutral;
        [SerializeField] private string designerNote;

        // Availability variables
        [Header("Availability")]
        [SerializeField] private bool affectsOutbound = true;
        [SerializeField] private bool affectsRecall = true;

        // Response variables
        [Header("Response")]
        [SerializeField, Range(0f, 1f)] private float speedRetention = 1f;
        [SerializeField, Min(0f)] private float distanceCost = 0f;

        public WeaponSurfaceKind Kind => kind;
        public string DesignerNote => designerNote;
        public float SpeedRetention => speedRetention;
        public float DistanceCost => distanceCost;

        /// <summary>
        /// Checks whether the authored surface response applies to the requested weapon phase.
        /// </summary>
        public bool AppliesTo(AttackPhase phase)
        {
            return phase == AttackPhase.Outbound
                ? affectsOutbound
                : phase == AttackPhase.Recall && affectsRecall;
        }

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(
            WeaponSurfaceKind newKind,
            string note,
            float newSpeedRetention = 1f,
            float newDistanceCost = 0f,
            bool outbound = true,
            bool recall = true)
        {
            kind = newKind;
            designerNote = note;
            speedRetention = Mathf.Clamp01(newSpeedRetention);
            distanceCost = Mathf.Max(0f, newDistanceCost);
            affectsOutbound = outbound;
            affectsRecall = recall;
        }

        /// <summary>
        /// Restores the authored default tuning values.
        /// </summary>
        public void ResetDefaults(WeaponSurfaceKind newKind)
        {
            switch (newKind)
            {
                case WeaponSurfaceKind.Reflective:
                    Configure(
                        newKind,
                        "Redirects the weapon using the collision normal. Slight speed loss keeps repeated banks readable.",
                        0.94f,
                        0.05f);
                    break;

                case WeaponSurfaceKind.Penetrable:
                    Configure(
                        newKind,
                        "Allows the weapon through but taxes momentum and effective range.",
                        0.82f,
                        0.45f);
                    break;

                case WeaponSurfaceKind.Curving:
                    Configure(
                        newKind,
                        "The solid itself does not stop the weapon. Directional bending comes from an authored curvature field.",
                        1f,
                        0f);
                    break;

                case WeaponSurfaceKind.Absorbing:
                    Configure(
                        newKind,
                        "Stops the weapon and leaves it embedded for a later recall.",
                        0f,
                        0f);
                    break;

                default:
                    Configure(
                        WeaponSurfaceKind.Neutral,
                        "Default world response: stop and embed.",
                        0f,
                        0f);
                    break;
            }
        }
    }
}
