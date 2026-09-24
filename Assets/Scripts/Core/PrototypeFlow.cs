using System.Collections;
using ReturnVector.Encounters;
using ReturnVector.Enemies;
using ReturnVector.GameFeel;
using ReturnVector.Input;
using ReturnVector.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ReturnVector.Core
{
    /// <summary>
    /// Owns the menu, run HUD, fail state, victory summary and scene-level flow.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PrototypeFlow : MonoBehaviour
    {
        private enum FlowState
        {
            MainMenu = 0,
            DifficultyMenu = 1,
            Playing = 2,
            Lost = 3,
            Complete = 4
        }

        [SerializeField] private RVInputReader input;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerMov movement;
        [SerializeField] private PlayerThrowController throwController;
        [SerializeField] private PlayerRecallController recallController;
        [SerializeField] private EncounterSequenceDirector sequence;
        [SerializeField] private RVHitStopController hitStop;
        [SerializeField, Min(0f)] private float victoryPauseDelay = 1.05f;

        private FlowState state;
        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle healthStyle;
        private GUIStyle bossStyle;
        private GUIStyle statLabelStyle;
        private GUIStyle statValueStyle;
        private GUIStyle menuTitleStyle;
        private GUIStyle menuHintStyle;

        private float runSeconds;
        private float finalRunSeconds;
        private float finalHealth;
        private int finalHealthScore;
        private int finalTimeScore;
        private int finalScore;
        private int finalEncounterCount;
        private int finalCompletedCount;
        private float finalScoreMultiplier;
        private string finalDifficultyLabel;

        private void Awake()
        {
            if (GameDifficulty.ShowMenuOnLoad)
            {
                state = FlowState.MainMenu;
                Time.timeScale = 0f;
                SetGameplayEnabled(false);
            }
            else
            {
                BeginRun();
            }
        }

        private void OnEnable()
        {
            if (input != null)
            {
                input.RestartPressed += HandleRestartPressed;
            }

            if (playerHealth != null)
            {
                playerHealth.Died += HandlePlayerDeath;
            }

            if (sequence != null)
            {
                sequence.EncounterCompleted += HandleEncounterCompleted;
                sequence.SequenceCompleted += HandleSequenceComplete;
            }
        }

        private void OnDisable()
        {
            if (input != null)
            {
                input.RestartPressed -= HandleRestartPressed;
            }

            if (playerHealth != null)
            {
                playerHealth.Died -= HandlePlayerDeath;
            }

            if (sequence != null)
            {
                sequence.EncounterCompleted -= HandleEncounterCompleted;
                sequence.SequenceCompleted -= HandleSequenceComplete;
            }
        }

        private void Update()
        {
            if (state == FlowState.Playing)
            {
                runSeconds += Mathf.Max(0f, Time.unscaledDeltaTime);
                return;
            }

            if ((state == FlowState.Lost || state == FlowState.Complete) &&
                Keyboard.current != null &&
                Keyboard.current.qKey.wasPressedThisFrame)
            {
                ReturnToMenu();
            }
        }

        private void BeginRun()
        {
            state = FlowState.Playing;
            runSeconds = 0f;
            Time.timeScale = 1f;

            if (playerHealth != null)
            {
                playerHealth.Configure(
                    GameDifficulty.Current.PlayerHealth,
                    false);
            }

            SetGameplayEnabled(true);
        }

        private void SelectDifficulty(RunDifficulty difficulty)
        {
            GameDifficulty.BeginRun(difficulty);
            BeginRun();
        }

        private void HandlePlayerDeath()
        {
            if (state != FlowState.Playing)
            {
                return;
            }

            state = FlowState.Lost;
            SetGameplayEnabled(false);

            if (hitStop != null)
            {
                hitStop.enabled = false;
            }

            Time.timeScale = 0f;
        }

        private void HandleEncounterCompleted(
            int index,
            EncounterController encounter)
        {
            if (state != FlowState.Playing ||
                playerHealth == null ||
                sequence == null ||
                sequence.SequenceComplete)
            {
                return;
            }

            float restoreFraction =
                GameDifficulty.Current.EncounterHealFraction;

            if (restoreFraction <= 0f)
            {
                return;
            }

            playerHealth.Restore(
                playerHealth.MaxHealth * restoreFraction);
        }

        private void HandleSequenceComplete()
        {
            if (state != FlowState.Playing)
            {
                return;
            }

            CaptureVictoryStats();
            state = FlowState.Complete;
            SetGameplayEnabled(false);
            StartCoroutine(PauseAfterVictoryFeedback());
        }

        private void CaptureVictoryStats()
        {
            finalRunSeconds = runSeconds;
            finalHealth =
                playerHealth != null
                    ? Mathf.Max(0f, playerHealth.CurrentHealth)
                    : 0f;

            float healthRatio =
                playerHealth != null && playerHealth.MaxHealth > 0f
                    ? Mathf.Clamp01(finalHealth / playerHealth.MaxHealth)
                    : 0f;

            finalHealthScore =
                Mathf.RoundToInt(healthRatio * 10000f);

            finalTimeScore =
                Mathf.RoundToInt(
                    Mathf.Max(0f, 720f - finalRunSeconds) * 10f);

            finalScoreMultiplier =
                GameDifficulty.Current.ScoreMultiplier;

            finalScore =
                Mathf.RoundToInt(
                    (finalHealthScore + finalTimeScore) *
                    finalScoreMultiplier);

            finalDifficultyLabel =
                GameDifficulty.SelectedLabel;

            finalEncounterCount =
                sequence != null
                    ? sequence.EncounterCount
                    : 0;

            finalCompletedCount =
                sequence != null
                    ? sequence.CompletedCount
                    : 0;
        }

        private IEnumerator PauseAfterVictoryFeedback()
        {
            yield return new WaitForSecondsRealtime(victoryPauseDelay);

            if (state != FlowState.Complete)
            {
                yield break;
            }

            if (hitStop != null)
            {
                hitStop.enabled = false;
            }

            Time.timeScale = 0f;
        }

        private void SetGameplayEnabled(bool enabled)
        {
            if (movement != null)
            {
                movement.SetDisabled(!enabled);
            }

            if (throwController != null)
            {
                throwController.enabled = enabled;
            }

            if (recallController != null)
            {
                recallController.enabled = enabled;
            }
        }

        private void HandleRestartPressed()
        {
            if (state != FlowState.Lost &&
                state != FlowState.Complete)
            {
                return;
            }

            GameDifficulty.RestartCurrentRun();
            ReloadScene();
        }

        private void ReturnToMenu()
        {
            if (state != FlowState.Lost &&
                state != FlowState.Complete)
            {
                return;
            }

            GameDifficulty.ReturnToMenu();
            ReloadScene();
        }

        private static void ReloadScene()
        {
            Time.timeScale = 1f;
            Scene scene = SceneManager.GetActiveScene();

            if (scene.buildIndex >= 0)
            {
                SceneManager.LoadScene(scene.buildIndex);
            }
            else
            {
                SceneManager.LoadScene(scene.name);
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            if (state == FlowState.MainMenu)
            {
                DrawMainMenu();
                return;
            }

            if (state == FlowState.DifficultyMenu)
            {
                DrawDifficultyMenu();
                return;
            }

            DrawPlayerHealth();
            DrawBossHealth();

            if (state == FlowState.Playing)
            {
                return;
            }

            if (state == FlowState.Complete)
            {
                DrawVictoryScreen();
            }
            else
            {
                DrawLossScreen();
            }
        }

        private void DrawMainMenu()
        {
            float width = Mathf.Min(360f, Screen.width - 48f);
            float x = (Screen.width - width) * 0.5f;
            float y = Screen.height * 0.28f;

            GUI.Label(
                new Rect(0f, y - 92f, Screen.width, 70f),
                "RETURN VECTOR",
                menuTitleStyle);

            if (GUI.Button(
                    new Rect(x, y, width, 68f),
                    "PLAY"))
            {
                state = FlowState.DifficultyMenu;
            }

            if (GUI.Button(
                    new Rect(x, y + 78f, width, 68f),
                    "QUIT"))
            {
                Application.Quit();
            }
        }

        private void DrawDifficultyMenu()
        {
            float panelWidth = Mathf.Min(820f, Screen.width - 48f);
            float buttonWidth = Mathf.Min(300f, panelWidth * 0.42f);
            float x = (Screen.width - panelWidth) * 0.5f;
            float buttonX = x + 24f;
            float hintX = buttonX + buttonWidth + 28f;
            float hintWidth = panelWidth - buttonWidth - 76f;
            float y = Mathf.Max(80f, Screen.height * 0.18f);

            GUI.Label(
                new Rect(0f, y - 76f, Screen.width, 58f),
                "SELECT DIFFICULTY",
                menuTitleStyle);

            DrawDifficultyChoice(
                RunDifficulty.VeryEasy,
                buttonX,
                hintX,
                y,
                buttonWidth,
                hintWidth);

            DrawDifficultyChoice(
                RunDifficulty.Easy,
                buttonX,
                hintX,
                y + 76f,
                buttonWidth,
                hintWidth);

            DrawDifficultyChoice(
                RunDifficulty.Normal,
                buttonX,
                hintX,
                y + 152f,
                buttonWidth,
                hintWidth);

            DrawDifficultyChoice(
                RunDifficulty.Hard,
                buttonX,
                hintX,
                y + 228f,
                buttonWidth,
                hintWidth);

            if (GUI.Button(
                    new Rect(
                        buttonX,
                        y + 316f,
                        buttonWidth,
                        46f),
                    "BACK"))
            {
                state = FlowState.MainMenu;
            }
        }

        private void DrawDifficultyChoice(
            RunDifficulty difficulty,
            float buttonX,
            float hintX,
            float y,
            float buttonWidth,
            float hintWidth)
        {
            if (GUI.Button(
                    new Rect(buttonX, y, buttonWidth, 62f),
                    GameDifficulty.Label(difficulty)))
            {
                SelectDifficulty(difficulty);
            }

            GUI.Label(
                new Rect(hintX, y, hintWidth, 62f),
                GameDifficulty.Recommendation(difficulty),
                menuHintStyle);
        }

        private void DrawPlayerHealth()
        {
            if (playerHealth == null)
            {
                return;
            }

            float ratio =
                playerHealth.MaxHealth > 0f
                    ? Mathf.Clamp01(
                        playerHealth.CurrentHealth /
                        playerHealth.MaxHealth)
                    : 0f;

            Rect labelRect = new Rect(24f, 20f, 220f, 24f);
            Rect backgroundRect = new Rect(24f, 48f, 220f, 14f);
            Rect fillRect =
                new Rect(
                    backgroundRect.x + 2f,
                    backgroundRect.y + 2f,
                    Mathf.Max(0f, (backgroundRect.width - 4f) * ratio),
                    backgroundRect.height - 4f);

            GUI.Label(
                labelRect,
                $"HP  {Mathf.CeilToInt(playerHealth.CurrentHealth)} / {Mathf.CeilToInt(playerHealth.MaxHealth)}",
                healthStyle);

            Color previous = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.72f);
            GUI.DrawTexture(backgroundRect, Texture2D.whiteTexture);
            GUI.color = new Color(0.9f, 0.18f, 0.14f, 0.95f);
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private void DrawBossHealth()
        {
            ReturnWardenHealth boss = ReturnWardenHealth.Active;
            if (boss == null || !boss.CanReceiveDamage)
            {
                return;
            }

            float ratio =
                boss.MaxHealth > 0f
                    ? Mathf.Clamp01(boss.CurrentHealth / boss.MaxHealth)
                    : 0f;

            float width = Mathf.Min(520f, Screen.width * 0.46f);
            float x = (Screen.width - width) * 0.5f;
            Rect labelRect = new Rect(x, 20f, width, 26f);
            Rect backgroundRect = new Rect(x, 50f, width, 18f);
            Rect fillRect =
                new Rect(
                    backgroundRect.x + 2f,
                    backgroundRect.y + 2f,
                    Mathf.Max(0f, (backgroundRect.width - 4f) * ratio),
                    backgroundRect.height - 4f);

            string phase =
                boss.IsTransitioning
                    ? boss.TransitionTargetPhase == 3
                        ? "PHASE III — ASCENDING"
                        : "TRANSFORMING"
                    : boss.IsPhaseThree
                        ? "PHASE III"
                        : boss.IsPhaseTwo
                            ? "PHASE II"
                            : "PHASE I";

            GUI.Label(
                labelRect,
                $"RETURN WARDEN  —  {phase}",
                bossStyle);

            Color previous = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.82f);
            GUI.DrawTexture(backgroundRect, Texture2D.whiteTexture);
            GUI.color =
                boss.IsPhaseThree
                    ? new Color(1f, 0.28f, 0.06f, 0.99f)
                    : boss.IsPhaseTwo
                        ? new Color(1f, 0.13f, 0.42f, 0.98f)
                        : new Color(0.72f, 0.2f, 0.92f, 0.96f);
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private void DrawVictoryScreen()
        {
            float panelWidth = Mathf.Min(560f, Screen.width - 48f);
            float panelHeight = 472f;
            float x = (Screen.width - panelWidth) * 0.5f;
            float y = Mathf.Max(28f, (Screen.height - panelHeight) * 0.5f);

            Rect panel = new Rect(x, y, panelWidth, panelHeight);
            Color previous = GUI.color;
            GUI.color = new Color(0.02f, 0.02f, 0.025f, 0.92f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = previous;

            GUI.Label(
                new Rect(x, y + 20f, panelWidth, 58f),
                "YOU WIN!",
                titleStyle);

            GUI.Label(
                new Rect(x, y + 78f, panelWidth, 42f),
                $"SCORE  {finalScore:N0}",
                bossStyle);

            float rowY = y + 136f;
            DrawStatRow(
                x + 72f,
                rowY,
                panelWidth - 144f,
                "DIFFICULTY",
                $"{finalDifficultyLabel}  x{finalScoreMultiplier:0.##}");
            rowY += 36f;
            DrawStatRow(x + 72f, rowY, panelWidth - 144f, "TIME", FormatTime(finalRunSeconds));
            rowY += 36f;
            DrawStatRow(
                x + 72f,
                rowY,
                panelWidth - 144f,
                "HEALTH",
                playerHealth != null
                    ? $"{Mathf.CeilToInt(finalHealth)} / {Mathf.CeilToInt(playerHealth.MaxHealth)}"
                    : "—");
            rowY += 36f;
            DrawStatRow(x + 72f, rowY, panelWidth - 144f, "HEALTH BONUS", finalHealthScore.ToString("N0"));
            rowY += 36f;
            DrawStatRow(x + 72f, rowY, panelWidth - 144f, "TIME BONUS", finalTimeScore.ToString("N0"));
            rowY += 36f;
            DrawStatRow(
                x + 72f,
                rowY,
                panelWidth - 144f,
                "ENCOUNTERS",
                $"{finalCompletedCount} / {finalEncounterCount}");

            GUI.Label(
                new Rect(x, y + panelHeight - 50f, panelWidth, 30f),
                "R — Restart     Q — Menu",
                subtitleStyle);
        }

        private void DrawLossScreen()
        {
            Rect titleRect =
                new Rect(
                    0f,
                    Screen.height * 0.36f,
                    Screen.width,
                    70f);

            Rect timeRect =
                new Rect(
                    0f,
                    Screen.height * 0.36f + 62f,
                    Screen.width,
                    32f);

            Rect subtitleRect =
                new Rect(
                    0f,
                    Screen.height * 0.36f + 96f,
                    Screen.width,
                    38f);

            GUI.Label(titleRect, "VECTOR LOST", titleStyle);
            GUI.Label(timeRect, $"TIME  {FormatTime(runSeconds)}", subtitleStyle);
            GUI.Label(subtitleRect, "R — Restart     Q — Menu", subtitleStyle);
        }

        private void DrawStatRow(
            float x,
            float y,
            float width,
            string label,
            string value)
        {
            GUI.Label(
                new Rect(x, y, width * 0.58f, 30f),
                label,
                statLabelStyle);

            GUI.Label(
                new Rect(x + width * 0.55f, y, width * 0.45f, 30f),
                value,
                statValueStyle);
        }

        private static string FormatTime(float seconds)
        {
            seconds = Mathf.Max(0f, seconds);
            int minutes = Mathf.FloorToInt(seconds / 60f);
            float remaining = seconds - minutes * 60f;
            return $"{minutes:00}:{remaining:00.00}";
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 38,
                fontStyle = FontStyle.Bold
            };
            titleStyle.normal.textColor = Color.white;

            menuTitleStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 42,
                fontStyle = FontStyle.Bold
            };
            menuTitleStyle.normal.textColor = Color.white;

            subtitleStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16
            };
            subtitleStyle.normal.textColor = Color.white;

            menuHintStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 16,
                wordWrap = true
            };
            menuHintStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f, 1f);

            healthStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 15,
                fontStyle = FontStyle.Bold
            };
            healthStyle.normal.textColor = Color.white;

            bossStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
            bossStyle.normal.textColor = Color.white;

            statLabelStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 16
            };
            statLabelStyle.normal.textColor = Color.white;

            statValueStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleRight,
                fontSize = 17,
                fontStyle = FontStyle.Bold
            };
            statValueStyle.normal.textColor = Color.white;
        }
    }
}
