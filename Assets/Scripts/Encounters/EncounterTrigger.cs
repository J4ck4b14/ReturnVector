using ReturnVector.Player;
using ReturnVector.Weapon;
using UnityEngine;

namespace ReturnVector.Encounters
{
    /// <summary>
    /// Starts an encounter when the player crosses its trigger.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EncounterTrigger : MonoBehaviour
    {
        [SerializeField] private EncounterController encounter;
        private bool fired;

        public void Configure(EncounterController newEncounter)
        {
            encounter = newEncounter;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (fired || encounter == null)
            {
                return;
            }

            PlayerCombatController player =
                other.GetComponentInParent<PlayerCombatController>();

            if (player == null)
            {
                return;
            }

            WeaponController weapon =
                player.Weapon;

            if (weapon != null &&
                weapon.State != WeaponState.Held)
            {
                PlayerRecallController recall =
                    player.GetComponent<PlayerRecallController>();

                if (recall != null)
                {
                    recall.ResetWeaponToHand();
                }
                else
                {
                    weapon.ResetToHeld();
                }
            }

            fired = true;
            encounter.StartEncounter();
        }
    }
}
