using ReturnVector.Player;
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

            fired = true;
            encounter.StartEncounter();
        }
    }
}
