using UnityEngine;

namespace ReturnVector.Weapon
{
    /// <summary>
    /// Cosmetic spin applied to the weapon visual child during travel.
    /// </summary>
    public sealed class WeaponSpinVisual : MonoBehaviour
    {
        [SerializeField] private WeaponController weapon;
        [SerializeField] private WeaponThrowTuning throwTuning;
        [SerializeField] private WeaponRecallTuning recallTuning;
        [SerializeField] private Vector3 localAxis = Vector3.right;

        public void Configure(
            WeaponController newWeapon,
            WeaponThrowTuning newThrowTuning)
        {
            Configure(newWeapon, newThrowTuning, null);
        }

        public void Configure(
            WeaponController newWeapon,
            WeaponThrowTuning newThrowTuning,
            WeaponRecallTuning newRecallTuning)
        {
            weapon = newWeapon;
            throwTuning = newThrowTuning;
            recallTuning = newRecallTuning;
        }

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
