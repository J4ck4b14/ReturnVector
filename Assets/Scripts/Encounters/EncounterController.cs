using System;
using System.Collections.Generic;
using ReturnVector.Core;
using ReturnVector.Enemies;
using UnityEngine;

// Script summary: Runs one authored encounter from activation through its final phase.

namespace ReturnVector.Encounters
{
    /// <summary>
    /// Runs one authored encounter from activation through its final phase.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EncounterController : MonoBehaviour
    {
        // Encounter variables
        private const float ExtremePressureDelay = 18f;
        private const float ExtremeWarningSeconds = 1.25f;

        [SerializeField] private EncounterDefinition definition;
        [SerializeField] private EncounterEnemyFactory enemyFactory;
        [SerializeField] private EncounterGate entranceGate;
        [SerializeField] private EncounterGate exitGate;
        [SerializeField] private Transform enemyContainer;

        private readonly List<EnemyHealth> spawnedEnemies =
            new List<EnemyHealth>(16);

        private bool[] spawnedEntries = Array.Empty<bool>();
        private int currentPhaseIndex = -1;
        private float phaseElapsed;
        private int liveEnemyCount;

        private bool extremePressureCommitted;
        private float extremeWarningRemaining;
        private EncounterSpawnEntry extremeReinforcement;
        private GameObject extremeWarning;

        public EncounterDefinition Definition => definition;
        public EncounterState State { get; private set; } =
            EncounterState.Waiting;
        public int CurrentPhaseIndex => currentPhaseIndex;
        public int CurrentPhaseNumber =>
            currentPhaseIndex >= 0
                ? currentPhaseIndex + 1
                : 0;
        public int PhaseCount =>
            definition != null && definition.Phases != null
                ? definition.Phases.Length
                : 0;
        public int LiveEnemyCount => liveEnemyCount;
        public float PhaseElapsed => Mathf.Max(0f, phaseElapsed);
        public EncounterGate EntranceGate => entranceGate;
        public EncounterGate ExitGate => exitGate;

        public string CurrentPhaseLabel
        {
            get
            {
                EncounterPhaseDefinition phase = CurrentPhase;
                return phase != null
                    ? phase.Label
                    : string.Empty;
            }
        }

        public event Action<EncounterController> Started;
        public event Action<EncounterController, int> PhaseStarted;
        public event Action<EncounterController, EnemyHealth> EnemySpawned;
        public event Action<EncounterController, ReturnVector.Combat.DamageInfo> EnemyDefeated;
        public event Action<EncounterController> Completed;

        private EncounterPhaseDefinition CurrentPhase
        {
            get
            {
                if (definition == null ||
                    definition.Phases == null ||
                    currentPhaseIndex < 0 ||
                    currentPhaseIndex >= definition.Phases.Length)
                {
                    return null;
                }

                return definition.Phases[currentPhaseIndex];
            }
        }

        /// <summary>
        /// Caches required references and prepares runtime state before the object starts running.
        /// </summary>
        private void Awake()
        {
            if (enemyContainer == null)
            {
                GameObject container =
                    new GameObject("ActiveEnemies");
                container.transform.SetParent(transform);
                enemyContainer = container.transform;
            }

            PrepareWaitingState();
        }

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(
            EncounterDefinition newDefinition,
            EncounterEnemyFactory newEnemyFactory,
            EncounterGate newEntranceGate,
            EncounterGate newExitGate,
            Transform newEnemyContainer = null)
        {
            definition = newDefinition;
            enemyFactory = newEnemyFactory;
            entranceGate = newEntranceGate;
            exitGate = newExitGate;

            if (newEnemyContainer != null)
            {
                enemyContainer = newEnemyContainer;
            }

            PrepareWaitingState();
        }

        /// <summary>
        /// Starts the encounter.
        /// </summary>
        public void StartEncounter()
        {
            if (State != EncounterState.Waiting ||
                definition == null ||
                definition.Phases == null ||
                definition.Phases.Length == 0)
            {
                return;
            }

            State = EncounterState.Active;
            entranceGate?.SetOpen(false);
            exitGate?.SetOpen(false);

            Started?.Invoke(this);
            BeginPhase(0);
        }

        /// <summary>
        /// Advances the component for the current frame.
        /// </summary>
        private void Update()
        {
            if (State != EncounterState.Active)
            {
                return;
            }

            EncounterPhaseDefinition phase = CurrentPhase;
            if (phase == null)
            {
                CompleteEncounter();
                return;
            }

            phaseElapsed += Time.deltaTime;

            if (phaseElapsed >= 0f)
            {
                SpawnDueEntries(phase);
                PruneDeadEnemies();
                TickExtremePressure(phase);
            }

            if (!EncounterTimelineMath.CanAdvancePhase(
                    AllEntriesSpawned(),
                    liveEnemyCount,
                    phaseElapsed,
                    phase.MinimumDuration))
            {
                return;
            }

            int next = currentPhaseIndex + 1;
            if (next >= definition.Phases.Length)
            {
                CompleteEncounter();
                return;
            }

            BeginPhase(next);
        }

        /// <summary>
        /// Starts the phase.
        /// </summary>
        private void BeginPhase(int phaseIndex)
        {
            ClearExtremeWarning();
            extremePressureCommitted = false;
            extremeWarningRemaining = 0f;
            extremeReinforcement = null;

            currentPhaseIndex = phaseIndex;

            EncounterPhaseDefinition phase =
                CurrentPhase;

            if (phase == null)
            {
                CompleteEncounter();
                return;
            }

            EncounterSpawnEntry[] spawns =
                phase.Spawns ??
                Array.Empty<EncounterSpawnEntry>();

            spawnedEntries =
                new bool[spawns.Length];

            phaseElapsed =
                -Mathf.Max(0f, phase.StartDelay);

            PhaseStarted?.Invoke(
                this,
                currentPhaseIndex);
        }

        /// <summary>
        /// Spawns the due entries.
        /// </summary>
        private void SpawnDueEntries(
            EncounterPhaseDefinition phase)
        {
            EncounterSpawnEntry[] spawns =
                phase.Spawns ??
                Array.Empty<EncounterSpawnEntry>();

            for (int i = 0; i < spawns.Length; i++)
            {
                if (spawnedEntries[i])
                {
                    continue;
                }

                EncounterSpawnEntry entry =
                    spawns[i];

                if (entry == null ||
                    !EncounterTimelineMath.IsSpawnDue(
                        phaseElapsed,
                        entry.Delay))
                {
                    continue;
                }

                spawnedEntries[i] = true;

                if (!GameDifficulty.ShouldSpawnEntry(
                        i,
                        spawns.Length,
                        entry.Archetype))
                {
                    continue;
                }

                Spawn(entry);
            }
        }

        /// <summary>
        /// Advances the the extreme pressure state for the current frame.
        /// </summary>
        private void TickExtremePressure(
            EncounterPhaseDefinition phase)
        {
            if (!GameDifficulty.IsExtreme ||
                enemyFactory == null ||
                phase == null ||
                ContainsWarden(phase) ||
                !AllEntriesSpawned())
            {
                return;
            }

            if (extremeReinforcement != null)
            {
                extremeWarningRemaining -= Time.deltaTime;

                if (extremeWarningRemaining > 0f)
                {
                    return;
                }

                ClearExtremeWarning();
                Spawn(extremeReinforcement);
                extremeReinforcement = null;
                return;
            }

            if (extremePressureCommitted ||
                liveEnemyCount < 2 ||
                phaseElapsed < ExtremePressureDelay)
            {
                return;
            }

            EncounterSpawnEntry reinforcement =
                BuildExtremeReinforcement(phase);

            if (reinforcement == null)
            {
                extremePressureCommitted = true;
                return;
            }

            extremePressureCommitted = true;
            extremeReinforcement = reinforcement;
            extremeWarningRemaining = ExtremeWarningSeconds;

            Vector3 worldPosition =
                transform.TransformPoint(
                    reinforcement.LocalPosition);

            extremeWarning =
                enemyFactory.CreatePressureWarning(
                    worldPosition);
        }

        /// <summary>
        /// Builds the extreme reinforcement.
        /// </summary>
        private EncounterSpawnEntry BuildExtremeReinforcement(
            EncounterPhaseDefinition phase)
        {
            EncounterSpawnEntry[] spawns =
                phase.Spawns ??
                Array.Empty<EncounterSpawnEntry>();

            if (spawns.Length == 0)
            {
                return null;
            }

            bool hasRusher = false;
            bool hasShielded = false;
            bool hasController = false;

            EncounterSpawnEntry farthest = null;
            float farthestDistance = float.NegativeInfinity;
            Transform player = enemyFactory.Player;

            for (int i = 0; i < spawns.Length; i++)
            {
                EncounterSpawnEntry entry = spawns[i];
                if (entry == null ||
                    entry.Archetype == EnemyArchetype.ReturnWarden)
                {
                    continue;
                }

                hasRusher |= entry.Archetype == EnemyArchetype.Rusher;
                hasShielded |= entry.Archetype == EnemyArchetype.Shielded;
                hasController |= entry.Archetype == EnemyArchetype.Controller;

                Vector3 world =
                    transform.TransformPoint(
                        entry.LocalPosition);

                float distance =
                    player != null
                        ? FlatDistance(world, player.position)
                        : i;

                if (distance > farthestDistance)
                {
                    farthestDistance = distance;
                    farthest = entry;
                }
            }

            if (farthest == null)
            {
                return null;
            }

            EnemyArchetype archetype;

            if (hasRusher && hasController)
            {
                archetype = EnemyArchetype.Shielded;
            }
            else if (hasRusher)
            {
                archetype = EnemyArchetype.Controller;
            }
            else if (hasController)
            {
                archetype = EnemyArchetype.Rusher;
            }
            else if (hasShielded)
            {
                archetype = EnemyArchetype.Controller;
            }
            else
            {
                archetype = EnemyArchetype.Rusher;
            }

            return new EncounterSpawnEntry(
                archetype,
                farthest.LocalPosition,
                0f,
                1f,
                "Extreme pressure");
        }

        /// <summary>
        /// Checks whether the current encounter definition contains the Return Warden.
        /// </summary>
        private static bool ContainsWarden(
            EncounterPhaseDefinition phase)
        {
            EncounterSpawnEntry[] spawns =
                phase.Spawns ??
                Array.Empty<EncounterSpawnEntry>();

            for (int i = 0; i < spawns.Length; i++)
            {
                if (spawns[i] != null &&
                    spawns[i].Archetype ==
                    EnemyArchetype.ReturnWarden)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Registers the runtime enemy with the owning system.
        /// </summary>
        public void RegisterRuntimeEnemy(
            EnemyHealth enemy)
        {
            if (enemy == null ||
                spawnedEnemies.Contains(enemy))
            {
                return;
            }

            spawnedEnemies.Add(enemy);
            liveEnemyCount++;

            enemy.Died += HandleEnemyDeath;
            EnemySpawned?.Invoke(this, enemy);
        }

        /// <summary>
        /// Spawns the requested enemy archetype at the supplied world position.
        /// </summary>
        private void Spawn(
            EncounterSpawnEntry entry)
        {
            if (enemyFactory == null)
            {
                Debug.LogError(
                    $"[RETURN VECTOR] Encounter '{name}' has no enemy factory.",
                    this);
                return;
            }

            Vector3 worldPosition =
                transform.TransformPoint(
                    entry.LocalPosition);

            EnemyHealth enemy =
                enemyFactory.Spawn(
                    entry,
                    worldPosition,
                    enemyContainer);

            if (enemy == null)
            {
                return;
            }

            spawnedEnemies.Add(enemy);
            liveEnemyCount++;

            enemy.Died += HandleEnemyDeath;
            EnemySpawned?.Invoke(this, enemy);
        }

        /// <summary>
        /// Responds when an enemy dies.
        /// </summary>
        private void HandleEnemyDeath(
            ReturnVector.Combat.DamageInfo damage)
        {
            EnemyDefeated?.Invoke(
                this,
                damage);

            PruneDeadEnemies();
        }

        /// <summary>
        /// Removes destroyed enemy references from the active encounter list.
        /// </summary>
        private void PruneDeadEnemies()
        {
            int living = 0;

            for (int i = spawnedEnemies.Count - 1;
                 i >= 0;
                 i--)
            {
                EnemyHealth enemy =
                    spawnedEnemies[i];

                if (enemy == null)
                {
                    spawnedEnemies.RemoveAt(i);
                    continue;
                }

                if (enemy.CanReceiveDamage)
                {
                    living++;
                    continue;
                }

                enemy.Died -= HandleEnemyDeath;
                spawnedEnemies.RemoveAt(i);
            }

            liveEnemyCount = living;
        }

        /// <summary>
        /// Checks whether every spawn entry in the current phase has been issued.
        /// </summary>
        private bool AllEntriesSpawned()
        {
            for (int i = 0;
                 i < spawnedEntries.Length;
                 i++)
            {
                if (!spawnedEntries[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Completes the encounter and updates the owning state.
        /// </summary>
        private void CompleteEncounter()
        {
            if (State == EncounterState.Complete)
            {
                return;
            }

            ClearExtremeWarning();
            State = EncounterState.Complete;
            liveEnemyCount = 0;

            entranceGate?.SetOpen(true);
            exitGate?.SetOpen(true);

            Completed?.Invoke(this);
        }

        /// <summary>
        /// Resets phase bookkeeping while the encounter waits to begin.
        /// </summary>
        private void PrepareWaitingState()
        {
            if (State == EncounterState.Active)
            {
                return;
            }

            ClearExtremeWarning();
            State = EncounterState.Waiting;
            currentPhaseIndex = -1;
            phaseElapsed = 0f;
            liveEnemyCount = 0;
            spawnedEntries = Array.Empty<bool>();
            extremePressureCommitted = false;
            extremeWarningRemaining = 0f;
            extremeReinforcement = null;

            entranceGate?.SetOpen(true);
            exitGate?.SetOpen(false);
        }

        /// <summary>
        /// Removes the active Extreme-mode reinforcement warning.
        /// </summary>
        private void ClearExtremeWarning()
        {
            if (extremeWarning != null)
            {
                Destroy(extremeWarning);
                extremeWarning = null;
            }
        }

        /// <summary>
        /// Returns the flat distance.
        /// </summary>
        private static float FlatDistance(
            Vector3 a,
            Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}
