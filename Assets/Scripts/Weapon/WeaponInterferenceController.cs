using UnityEngine;

namespace ReturnVector.Weapon
{
    /// <summary>
    /// Narrow deterministic hook for authored enemy interference.
    /// Routes an authored deflection into whichever travel motor is currently active.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WeaponInterferenceController : MonoBehaviour
    {
        [SerializeField] private WeaponController weapon;
        [SerializeField] private OutboundWeaponMotor outboundMotor;
        [SerializeField] private RecallWeaponMotor recallMotor;

        public void Configure(
            WeaponController newWeapon,
            OutboundWeaponMotor newOutboundMotor,
            RecallWeaponMotor newRecallMotor)
        {
            weapon = newWeapon;
            outboundMotor = newOutboundMotor;
            recallMotor = newRecallMotor;
        }

        public bool DeflectAwayFrom(
            Vector3 sourcePosition,
            float degrees)
        {
            if (weapon == null ||
                degrees <= 0f)
            {
                return false;
            }

            Vector3 away =
                transform.position - sourcePosition;
            away.y = 0f;

            if (away.sqrMagnitude < 0.0001f)
            {
                away = transform.right;
            }

            switch (weapon.State)
            {
                case WeaponState.Outbound:
                    return outboundMotor != null &&
                           outboundMotor.DeflectToward(
                               away,
                               degrees);

                case WeaponState.Returning:
                    return recallMotor != null &&
                           recallMotor.DeflectToward(
                               away,
                               degrees);

                default:
                    return false;
            }
        }
    }
}
