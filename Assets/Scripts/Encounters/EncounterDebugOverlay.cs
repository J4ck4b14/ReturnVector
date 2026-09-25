using UnityEngine;

// Script summary: Displays live encounter state while testing the prototype.

namespace ReturnVector.Encounters
{
    /// <summary>
    /// Displays live encounter state while testing the prototype.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EncounterDebugOverlay : MonoBehaviour
    {
        // Encounter variables
        [SerializeField] private EncounterSequenceDirector sequence;

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(
            EncounterSequenceDirector newSequence)
        {
            sequence = newSequence;
        }

        /// <summary>
        /// Draws the current runtime interface.
        /// </summary>
        private void OnGUI()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (sequence == null)
            {
                return;
            }

            EncounterController encounter =
                sequence.CurrentEncounter;

            Rect area = new Rect(
                Screen.width - 390f,
                12f,
                378f,
                160f);

            Color previous = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.72f);
            GUI.DrawTexture(area, Texture2D.whiteTexture);
            GUI.color = previous;

            GUILayout.BeginArea(
                new Rect(
                    area.x + 10f,
                    area.y + 8f,
                    area.width - 20f,
                    area.height - 16f));

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
