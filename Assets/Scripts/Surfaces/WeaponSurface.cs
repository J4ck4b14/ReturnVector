using UnityEngine;

namespace ReturnVector.Surfaces
{
    /// <summary>
    /// Binds a collider to an authored weapon-surface profile.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WeaponSurface : MonoBehaviour
    {
        [SerializeField] private WeaponSurfaceProfile profile;

        public WeaponSurfaceProfile Profile => profile;

        public void Configure(WeaponSurfaceProfile newProfile)
        {
            profile = newProfile;
        }
    }
}
