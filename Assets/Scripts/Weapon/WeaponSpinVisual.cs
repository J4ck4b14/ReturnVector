using UnityEngine;

// Script summary: Cosmetic spin applied to the weapon visual child during travel.

namespace ReturnVector.Weapon
{
    /// <summary>
    /// Cosmetic spin applied to the weapon visual child during travel.
    /// </summary>
    public sealed class WeaponSpinVisual : MonoBehaviour
    {
        // Weapon variables
        [SerializeField] private WeaponController weapon;
        [SerializeField] private WeaponThrowTuning throwTuning;
        [SerializeField] private WeaponRecallTuning recallTuning;
        [SerializeField] private Vector3 localAxis = Vector3.right;

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(
            WeaponController newWeapon,
            WeaponThrowTuning newThrowTuning)
        {
            Configure(newWeapon, newThrowTuning, null);
        }

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(
            WeaponController newWeapon,
            WeaponThrowTuning newThrowTuning,
            WeaponRecallTuning newRecallTuning)
        {
            weapon = newWeapon;
            throwTuning = newThrowTuning;
            recallTuning = newRecallTuning;
        }

        /// <summary>
        /// Advances the component for the current frame.
        /// </summary>
        private void Update()
        {
            if (weapon == null)
            {
                return;
            }

            float degreesPerSecond = 0f;

            if (weapon.State == WeaponState.Outbound && throwTuning != null)
            {
                degreesPerSecond = throwTuning.SpinDegreesPerSecond;
            }
            else if (weapon.State == WeaponState.Returning && recallTuning != null)
            {
                degreesPerSecond = recallTuning.RecallSpinDegreesPerSecond;
            }

            if (degreesPerSecond <= 0f)
            {
                return;
            }

            transform.Rotate(
                localAxis,
                degreesPerSecond * Time.deltaTime,
                Space.Self);
        }
    }
}
