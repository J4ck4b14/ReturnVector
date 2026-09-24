using UnityEngine;

namespace ReturnVector.Encounters
{
    /// <summary>
    /// Displays live encounter state while testing the prototype.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EncounterDebugOverlay : MonoBehaviour
    {
        [SerializeField] private EncounterSequenceDirector sequence;

        public void Configure(
            EncounterSequenceDirector newSequence)
        {
            sequence = newSequence;
        }

        private void OnGUI()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (sequence == null)
            {
                return;
            }

            EncounterController encounter =
                sequence.CurrentEncounter;

            GUILayout.BeginArea(
                new Rect(
                    Screen.width - 390f,
                    12f,
                    378f,
                    160f),
                GUI.skin.box);

            GUILayout.Label(
                $"ENCOUNTERS {sequence.CompletedCount}/{sequence.EncounterCount}");

            if (encounter == null)
            {
                GUILayout.Label(
                    sequence.SequenceComplete
                        ? "Sequence complete"
                        : "Move into the next room");
            }
            else
            {
                EncounterDefinition definition =
                    encounter.Definition;

                GUILayout.Label(
                    definition != null
                        ? definition.DisplayName
                        : encounter.name);

                GUILayout.Label(
                    $"Phase {encounter.CurrentPhaseNumber}/{encounter.PhaseCount}: " +
                    $"{encounter.CurrentPhaseLabel}");

                GUILayout.Label(
                    $"Living enemies: {encounter.LiveEnemyCount}");

                if (definition != null &&
                    !string.IsNullOrWhiteSpace(
                        definition.LearningGoal))
                {
                    GUILayout.Label(
                        $"Intent: {definition.LearningGoal}");
                }
            }

            GUILayout.EndArea();
#endif
        }
    }
}
