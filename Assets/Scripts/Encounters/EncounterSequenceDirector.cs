using System;
using System.Collections.Generic;
using UnityEngine;

// Script summary: Tracks progress across the ordered encounter chain.

namespace ReturnVector.Encounters
{
    /// <summary>
    /// Tracks progress across the ordered encounter chain.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EncounterSequenceDirector : MonoBehaviour
    {
        // Encounter variables
        [SerializeField] private EncounterController[] encounters =
            Array.Empty<EncounterController>();

        private int completedCount;

        public int EncounterCount =>
            encounters != null
                ? encounters.Length
                : 0;

        public int CompletedCount => completedCount;
        public IReadOnlyList<EncounterController> Encounters => encounters;

        public bool SequenceComplete =>
            EncounterCount > 0 &&
            completedCount >= EncounterCount;

        public EncounterController CurrentEncounter
        {
            get
            {
                if (encounters == null)
                {
                    return null;
                }

                for (int i = 0;
                     i < encounters.Length;
                     i++)
                {
                    EncounterController encounter =
                        encounters[i];

                    if (encounter != null &&
                        encounter.State == EncounterState.Active)
                    {
                        return encounter;
                    }
                }

                return null;
            }
        }

        public event Action<int, EncounterController> EncounterCompleted;
        public event Action SequenceCompleted;

        /// <summary>
        /// Subscribes to runtime events when the component becomes active.
        /// </summary>
        private void OnEnable()
        {
            Subscribe();
            RefreshCompletedCount();
        }

        /// <summary>
        /// Unsubscribes from runtime events when the component is disabled.
        /// </summary>
        private void OnDisable()
        {
            Unsubscribe();
        }

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(
            EncounterController[] newEncounters)
        {
            if (isActiveAndEnabled)
            {
                Unsubscribe();
            }

            encounters =
                newEncounters ??
                Array.Empty<EncounterController>();

            RefreshCompletedCount();

            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        /// <summary>
        /// Subscribes to the runtime events used by this component.
        /// </summary>
        private void Subscribe()
        {
            if (encounters == null)
            {
                return;
            }

            for (int i = 0;
                 i < encounters.Length;
                 i++)
            {
                if (encounters[i] != null)
                {
                    encounters[i].Completed +=
                        HandleEncounterCompleted;
                }
            }
        }

        /// <summary>
        /// Unsubscribes from the runtime events used by this component.
        /// </summary>
        private void Unsubscribe()
        {
            if (encounters == null)
            {
                return;
            }

            for (int i = 0;
                 i < encounters.Length;
                 i++)
            {
                if (encounters[i] != null)
                {
                    encounters[i].Completed -=
                        HandleEncounterCompleted;
                }
            }
        }

        /// <summary>
        /// Responds when an encounter completes.
        /// </summary>
        private void HandleEncounterCompleted(
            EncounterController encounter)
        {
            RefreshCompletedCount();

            int index =
                Array.IndexOf(
                    encounters,
                    encounter);

            EncounterCompleted?.Invoke(
                index,
                encounter);

            if (SequenceComplete)
            {
                SequenceCompleted?.Invoke();
            }
        }

        /// <summary>
        /// Refreshes the completed count.
        /// </summary>
        private void RefreshCompletedCount()
        {
            completedCount = 0;

            if (encounters == null)
            {
                return;
            }

            for (int i = 0;
                 i < encounters.Length;
                 i++)
            {
                if (encounters[i] != null &&
                    encounters[i].State ==
                    EncounterState.Complete)
                {
                    completedCount++;
                }
            }
        }
    }
}
