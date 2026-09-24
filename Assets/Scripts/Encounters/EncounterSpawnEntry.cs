using System;
using ReturnVector.Enemies;
using UnityEngine;

namespace ReturnVector.Encounters
{
    /// <summary>
    /// Serialized spawn instruction used by an encounter phase.
    /// </summary>
    [Serializable]
    public sealed class EncounterSpawnEntry
    {
        public EnemyArchetype Archetype = EnemyArchetype.Rusher;
        public Vector3 LocalPosition;
        [Min(0f)] public float Delay;
        [Min(0.1f)] public float HealthMultiplier = 1f;
        public string DebugLabel;

        public EncounterSpawnEntry()
        {
        }

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
