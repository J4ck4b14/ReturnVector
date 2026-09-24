using ReturnVector.Input;
using ReturnVector.Player;
using ReturnVector.Weapon;
using UnityEngine;

namespace ReturnVector.Debugging
{
    /// <summary>
    /// Runtime diagnostics overlay for development builds and the Editor.
    /// </summary>
    public sealed class RVDebugOverlay : MonoBehaviour
    {
        [SerializeField] private RVDebugSettings settings;
        [SerializeField] private RVInputReader input;
        [SerializeField] private PlayerCombatController playerCombat;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerMov playerMovement;
        [SerializeField] private PlayerTacticalStateSource tacticalState;
        [SerializeField] private WeaponController weapon;
        [SerializeField] private OutboundWeaponMotor outboundMotor;
        [SerializeField] private RecallWeaponMotor recallMotor;
        [SerializeField] private PlayerThrowController throwController;
        [SerializeField] private RVCombatDebugRecorder recorder;
        [SerializeField] private RVWeaponTraceRecorder traceRecorder;

        public void Configure(
            RVDebugSettings newSettings,
            RVInputReader newInput,
            PlayerCombatController newPlayerCombat,
            WeaponController newWeapon)
        {
            Configure(
                newSettings, newInput, newPlayerCombat, newWeapon,
                null, null, null, null, null, null, null);
        }

        public void Configure(
            RVDebugSettings newSettings,
            RVInputReader newInput,
            PlayerCombatController newPlayerCombat,
            WeaponController newWeapon,
            OutboundWeaponMotor newOutboundMotor,
            PlayerThrowController newThrowController)
        {
            Configure(
                newSettings, newInput, newPlayerCombat, newWeapon,
                newOutboundMotor, null, newThrowController,
                null, null, null, null);
        }

        public void Configure(
            RVDebugSettings newSettings,
            RVInputReader newInput,
            PlayerCombatController newPlayerCombat,
            WeaponController newWeapon,
            OutboundWeaponMotor newOutboundMotor,
            RecallWeaponMotor newRecallMotor,
            PlayerThrowController newThrowController)
        {
            Configure(
                newSettings, newInput, newPlayerCombat, newWeapon,
                newOutboundMotor, newRecallMotor, newThrowController,
                null, null, null, null);
        }

        public void Configure(
            RVDebugSettings newSettings,
            RVInputReader newInput,
            PlayerCombatController newPlayerCombat,
            WeaponController newWeapon,
            OutboundWeaponMotor newOutboundMotor,
            RecallWeaponMotor newRecallMotor,
            PlayerThrowController newThrowController,
            PlayerMov newPlayerMovement,
            PlayerTacticalStateSource newTacticalState,
            RVCombatDebugRecorder newRecorder = null,
            RVWeaponTraceRecorder newTraceRecorder = null)
        {
            settings = newSettings;
            input = newInput;
            playerCombat = newPlayerCombat;
            weapon = newWeapon;
            outboundMotor = newOutboundMotor;
            recallMotor = newRecallMotor;
            throwController = newThrowController;
            playerMovement = newPlayerMovement;
            tacticalState = newTacticalState;
            recorder = newRecorder;
            traceRecorder = newTraceRecorder;
        }

        public void ConfigurePlayerHealth(
            PlayerHealth newPlayerHealth)
        {
            playerHealth = newPlayerHealth;
        }

        private void OnGUI()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (settings == null || !settings.ShowRuntimeOverlay)
            {
                return;
            }

            float height =
                recorder != null &&
                settings.ShowRecentEventsInOverlay
                    ? 520f
                    : 370f;

            GUILayout.BeginArea(
                new Rect(12, 12, 520, height),
                GUI.skin.box);

            GUILayout.Label(
                recorder != null
                    ? "RETURN VECTOR — DEBUG"
                    : playerMovement != null
                        ? "RETURN VECTOR"
                        : "RETURN VECTOR");

            GUILayout.Label(
                $"Input: {(input != null && input.IsReady ? "Ready" : "Missing")}");
            GUILayout.Label(
                $"Player: {(playerCombat != null ? playerCombat.Mode.ToString() : "Missing")}");
            GUILayout.Label(
                $"Weapon: {(weapon != null ? weapon.State.ToString() : "Missing")}");

            if (playerHealth != null)
            {
                GUILayout.Label(
                    $"Health: {playerHealth.CurrentHealth:0.0} / " +
                    $"{playerHealth.MaxHealth:0.0}");
            }

            if (playerMovement != null)
            {
                GUILayout.Label(
                    $"Movement: {playerMovement.State} / " +
                    $"{playerMovement.Speed:0.00} m/s");

                GUILayout.Label(
                    $"Dodge cooldown: " +
                    $"{playerMovement.DodgeCooldownRemaining:0.00}s");
            }

            if (tacticalState != null)
            {
                string opportunity = tacticalState.IsExposed
                    ? "EXPOSED — enemy opportunity"
                    : tacticalState.IsEvading
                        ? "EVADING"
                        : "COVERED";

                GUILayout.Label($"Tactical: {opportunity}");
            }

            if (throwController != null &&
                throwController.IsAnticipating)
            {
                GUILayout.Label(
                    $"Release in: " +
                    $"{throwController.AnticipationRemaining:0.000}s");
            }

            if (outboundMotor != null)
            {
                string activeText =
                    outboundMotor.IsActive ? "ACTIVE" : "idle";

                GUILayout.Label(
                    $"Outbound: {activeText}, " +
                    $"{outboundMotor.Speed:0.0} m/s, " +
                    $"{outboundMotor.TravelledDistance:0.00}m travelled, " +
                    $"{outboundMotor.RemainingDistance:0.00}m remaining");
            }

            if (recallMotor != null)
            {
                string activeText =
                    recallMotor.IsActive ? "ACTIVE" : "idle";

                GUILayout.Label(
                    $"Recall: {activeText}, " +
                    $"{recallMotor.Speed:0.0} m/s, " +
                    $"{recallMotor.DistanceToCatch:0.00}m to catch");
            }

            if (outboundMotor != null &&
                outboundMotor.LastSurfaceResponse.HasValue)
            {
                GUILayout.Label(
                    $"Outbound surface: " +
                    $"{outboundMotor.LastSurfaceResponse.Value}");
            }

            if (recallMotor != null &&
                recallMotor.LastSurfaceResponse.HasValue)
            {
                GUILayout.Label(
                    $"Recall surface: " +
                    $"{recallMotor.LastSurfaceResponse.Value}");
            }

            if (traceRecorder != null)
            {
                GUILayout.Label(
                    $"Actual trace samples: " +
                    $"{traceRecorder.SampleCount}");
            }

            if (recorder != null &&
                settings.ShowSessionCounters)
            {
                GUILayout.Space(4f);
                GUILayout.Label(
                    $"Session — Out hits {recorder.OutboundImpacts} | " +
                    $"Return hits {recorder.RecallImpacts} | " +
                    $"Surfaces {recorder.SurfaceResponses} | " +
                    $"Blocked recalls {recorder.BlockedRecalls} | " +
                    $"Catches {recorder.SuccessfulCatches}");
            }

            if (recorder != null &&
                settings.ShowRecentEventsInOverlay)
            {
                GUILayout.Space(6f);
                GUILayout.Label("Recent events");

                int count = recorder.Events.Count;
                int show = Mathf.Min(
                    settings.OverlayRecentEventCount,
                    count);
                int start = Mathf.Max(0, count - show);

                for (int i = start; i < count; i++)
                {
                    GUILayout.Label(
                        recorder.Events[i].ToString());
                }
            }

            GUILayout.EndArea();
#endif
        }
    }
}
