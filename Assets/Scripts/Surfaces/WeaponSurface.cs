using UnityEngine;

// Script summary: Binds a collider to an authored weapon-surface profile.

namespace ReturnVector.Surfaces
{
    /// <summary>
    /// Binds a collider to an authored weapon-surface profile.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WeaponSurface : MonoBehaviour
    {
        // Surface variables
        [SerializeField] private WeaponSurfaceProfile profile;

        public WeaponSurfaceProfile Profile => profile;

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(WeaponSurfaceProfile newProfile)
        {
            profile = newProfile;
        }
    }
}
