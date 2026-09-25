using System;
using ReturnVector.Enemies;
using UnityEngine;

// Script summary: Serialized spawn instruction used by an encounter phase.

namespace ReturnVector.Encounters
{
    /// <summary>
    /// Serialized spawn instruction used by an encounter phase.
    /// </summary>
    [Serializable]
    public sealed class EncounterSpawnEntry
    {
        // Encounter variables
        public EnemyArchetype Archetype = EnemyArchetype.Rusher;
        public Vector3 LocalPosition;
        [Min(0f)] public float Delay;
        [Min(0.1f)] public float HealthMultiplier = 1f;
        public string DebugLabel;

        /// <summary>
        /// Creates a new EncounterSpawnEntry with the supplied values.
        /// </summary>
        public EncounterSpawnEntry()
        {
        }

        /// <summary>
        /// Creates a new EncounterSpawnEntry with the supplied values.
        /// </summary>
        public EncounterSpawnEntry(
            EnemyArchetype archetype,
            Vector3 localPosition,
            float delay = 0f,
            float healthMultiplier = 1f,
            string debugLabel = null)
        {
            Archetype = archetype;
            LocalPosition = localPosition;
            Delay = Mathf.Max(0f, delay);
            HealthMultiplier = Mathf.Max(0.1f, healthMultiplier);
            DebugLabel = debugLabel ?? string.Empty;
        }
    }
}
