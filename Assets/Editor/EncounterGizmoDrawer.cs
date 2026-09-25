using ReturnVector.Encounters;
using ReturnVector.Enemies;
using UnityEditor;
using UnityEngine;

// Script summary: Draws encounter spawn positions and labels in the Scene view.

namespace ReturnVector.Editor
{
    /// <summary>
    /// Draws encounter spawn positions and labels in the Scene view.
    /// </summary>
    public static class EncounterGizmoDrawer
    {
        /// <summary>
        /// Draws the encounter.
        /// </summary>
        [DrawGizmo(GizmoType.Selected)]
        private static void DrawEncounter(
            EncounterController encounter,
            GizmoType gizmoType)
        {
            if (encounter == null ||
                encounter.Definition == null ||
                encounter.Definition.Phases == null)
            {
                return;
            }

            EncounterPhaseDefinition[] phases =
                encounter.Definition.Phases;

            for (int p = 0; p < phases.Length; p++)
            {
                EncounterPhaseDefinition phase = phases[p];
                if (phase == null || phase.Spawns == null)
                {
                    continue;
                }

                for (int i = 0; i < phase.Spawns.Length; i++)
                {
                    EncounterSpawnEntry spawn = phase.Spawns[i];
                    if (spawn == null)
                    {
                        continue;
                    }

                    Vector3 position =
                        encounter.transform.TransformPoint(
                            spawn.LocalPosition);

                    Handles.color =
                        ColorFor(spawn.Archetype);

                    Handles.DrawWireDisc(
                        position,
                        Vector3.up,
                        0.6f);

                    Handles.Label(
                        position + Vector3.up * 1.2f,
                        $"P{p + 1} {spawn.Archetype}" +
                        (spawn.Delay > 0f
                            ? $" +{spawn.Delay:0.0}s"
                            : string.Empty));
                }
            }

            Handles.Label(
                encounter.transform.position + Vector3.up * 2f,
                encounter.Definition.DisplayName);
        }

        /// <summary>
        /// Returns the gizmo colour associated with the supplied encounter state.
        /// </summary>
        private static Color ColorFor(
            EnemyArchetype archetype)
        {
            switch (archetype)
            {
                case EnemyArchetype.Rusher:
                    return new Color(1f, 0.25f, 0.2f);

                case EnemyArchetype.Shielded:
                    return new Color(0.25f, 0.5f, 1f);

                case EnemyArchetype.Controller:
                    return new Color(1f, 0.8f, 0.2f);

                case EnemyArchetype.ReturnWarden:
                    return new Color(0.95f, 0.35f, 0.95f);

                default:
                    return Color.white;
            }
        }
    }
}
