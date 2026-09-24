using System;
using UnityEngine;

namespace ReturnVector.Encounters
{
    /// <summary>
    /// Defines the spawn list and timing constraints for one encounter phase.
    /// </summary>
    [Serializable]
    public sealed class EncounterPhaseDefinition
    {
        public string Label = "Phase";
        [Min(0f)] public float StartDelay;
        [Min(0f)] public float MinimumDuration;
        public EncounterSpawnEntry[] Spawns = Array.Empty<EncounterSpawnEntry>();

        public EncounterPhaseDefinition()
        {
        }

        public EncounterPhaseDefinition(
            string label,
            EncounterSpawnEntry[] spawns,
            float startDelay = 0f,
            float minimumDuration = 0f)
        {
            Label = label;
            Spawns = spawns ?? Array.Empty<EncounterSpawnEntry>();
            StartDelay = Mathf.Max(0f, startDelay);
            MinimumDuration = Mathf.Max(0f, minimumDuration);
        }
    }
}
