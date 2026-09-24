using System;
using System.Collections.Generic;
using ReturnVector.Enemies;
using UnityEngine;

namespace ReturnVector.Encounters
{
    /// <summary>
    /// Runs one authored encounter from activation through its final phase.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EncounterController : MonoBehaviour
    {
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

        public void StartEncounter()
        {
            if (State != EncounterState.Waiting ||
                definition == null ||
                definition.Phases == null ||
                definition.Phases.Length == 0)
            {
                return;
            }

            // The room closes as soon as the trigger commits the player to the encounter.
            State = EncounterState.Active;
            entranceGate?.SetOpen(false);
            exitGate?.SetOpen(false);

            Started?.Invoke(this);
            BeginPhase(0);
        }

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

            // Negative elapsed time represents the phase's authored opening delay.
            phaseElapsed += Time.deltaTime;

            if (phaseElapsed >= 0f)
            {
                SpawnDueEntries(phase);
                PruneDeadEnemies();
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

        private void BeginPhase(int phaseIndex)
        {
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

            // Spawn bookkeeping is rebuilt per phase; each entry may carry its own delay.
            spawnedEntries =
                new bool[spawns.Length];

            phaseElapsed =
                -Mathf.Max(0f, phase.StartDelay);

            PhaseStarted?.Invoke(
                this,
                currentPhaseIndex);
        }

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
                Spawn(entry);
            }
        }

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

        private void HandleEnemyDeath(
            ReturnVector.Combat.DamageInfo damage)
        {
            EnemyDefeated?.Invoke(
                this,
                damage);

            PruneDeadEnemies();
        }

        // Recount from the tracked set so destroyed GameObjects and normal deaths converge cleanly.
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

        private void CompleteEncounter()
        {
            if (State == EncounterState.Complete)
            {
                return;
            }

            State = EncounterState.Complete;
            liveEnemyCount = 0;

            entranceGate?.SetOpen(true);
            exitGate?.SetOpen(true);

            Completed?.Invoke(this);
        }

        private void PrepareWaitingState()
        {
            if (State == EncounterState.Active)
            {
                return;
            }

            State = EncounterState.Waiting;
            currentPhaseIndex = -1;
            phaseElapsed = 0f;
            liveEnemyCount = 0;
            spawnedEntries = Array.Empty<bool>();

            entranceGate?.SetOpen(true);
            exitGate?.SetOpen(false);
        }
    }
}
