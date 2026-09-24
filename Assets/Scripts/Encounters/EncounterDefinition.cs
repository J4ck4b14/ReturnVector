using System;
using UnityEngine;

namespace ReturnVector.Encounters
{
    [CreateAssetMenu(
        fileName = "SO_Encounter",
        menuName = "RETURN VECTOR/Encounter Definition")]
    /// <summary>
    /// Authored phase data for a combat encounter.
    /// </summary>
    public sealed class EncounterDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Encounter";
        [SerializeField, TextArea] private string learningGoal;
        [SerializeField] private EncounterPhaseDefinition[] phases =
            Array.Empty<EncounterPhaseDefinition>();

        public string DisplayName => displayName;
        public string LearningGoal => learningGoal;
        public EncounterPhaseDefinition[] Phases => phases;

        public void Configure(
            string newDisplayName,
            string newLearningGoal,
            EncounterPhaseDefinition[] newPhases)
        {
            displayName =
                string.IsNullOrWhiteSpace(newDisplayName)
                    ? "Encounter"
                    : newDisplayName;

            learningGoal = newLearningGoal ?? string.Empty;
            phases = newPhases ?? Array.Empty<EncounterPhaseDefinition>();
        }
    }
}
