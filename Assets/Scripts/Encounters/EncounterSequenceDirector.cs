using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReturnVector.Encounters
{
    /// <summary>
    /// Tracks progress across the ordered encounter chain.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EncounterSequenceDirector : MonoBehaviour
    {
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

        private void OnEnable()
        {
            Subscribe();
            RefreshCompletedCount();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

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
