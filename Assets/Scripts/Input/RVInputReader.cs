using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Script summary: Thin input boundary for gameplay code. It keeps concrete Input System assets out of combat and weapon classes.

namespace ReturnVector.Input
{
    /// <summary>
    /// Thin input boundary for gameplay code. It keeps concrete Input System assets
    /// out of combat and weapon classes.
    /// </summary>
    public sealed class RVInputReader : MonoBehaviour
    {
        // Input variables
        [SerializeField] private InputActionAsset actions;
        [SerializeField] private string gameplayMapName = "Gameplay";

        private InputActionMap gameplayMap;
        private InputAction move;
        private InputAction aim;
        private InputAction throwAction;
        private InputAction recall;
        private InputAction dodge;
        private InputAction restart;
        private bool callbacksBound;

        public Vector2 Move => move != null ? move.ReadValue<Vector2>() : Vector2.zero;
        public Vector2 Aim => aim != null ? aim.ReadValue<Vector2>() : Vector2.zero;
        public bool IsReady => gameplayMap != null;

        // Player variables
        public AimInputKind AimKind
        {
            get
            {
                if (aim == null || aim.activeControl == null)
                {
                    return Mouse.current != null
                        ? AimInputKind.ScreenPosition
                        : AimInputKind.Unknown;
                }

                return aim.activeControl.device is Pointer
                    ? AimInputKind.ScreenPosition
                    : AimInputKind.Directional;
            }
        }

        public event Action ThrowPressed;
        public event Action RecallPressed;
        public event Action DodgePressed;
        public event Action RestartPressed;

        /// <summary>
        /// Caches required references and prepares runtime state before the object starts running.
        /// </summary>
        private void Awake()
        {
            ResolveActions();
        }

        /// <summary>
        /// Subscribes to runtime events when the component becomes active.
        /// </summary>
        private void OnEnable()
        {
            ResolveActions();
            BindAndEnable();
        }

        /// <summary>
        /// Unsubscribes from runtime events when the component is disabled.
        /// </summary>
        private void OnDisable()
        {
            UnbindAndDisable();
        }

        /// <summary>
        /// Assigns the runtime references and tuning used by the component.
        /// </summary>
        public void Configure(InputActionAsset actionAsset)
        {
            UnbindAndDisable();
            actions = actionAsset;
            ResolveActions();

            if (isActiveAndEnabled)
            {
                BindAndEnable();
            }
        }

        /// <summary>
        /// Resolves the actions.
        /// </summary>
        private void ResolveActions()
        {
            gameplayMap = null;
            move = null;
            aim = null;
            throwAction = null;
            recall = null;
            dodge = null;
            restart = null;

            if (actions == null)
            {
                return;
            }

            gameplayMap = actions.FindActionMap(gameplayMapName, false);
            if (gameplayMap == null)
            {
                Debug.LogError(
                    $"[RETURN VECTOR] Input map '{gameplayMapName}' was not found.",
                    this);
                return;
            }

            move = gameplayMap.FindAction("Move", true);
            aim = gameplayMap.FindAction("Aim", true);
            throwAction = gameplayMap.FindAction("Throw", true);
            recall = gameplayMap.FindAction("Recall", true);
            dodge = gameplayMap.FindAction("Dodge", true);
            restart = gameplayMap.FindAction("Restart", true);
        }

        /// <summary>
        /// Binds gameplay callbacks and enables the configured Input System actions.
        /// </summary>
        private void BindAndEnable()
        {
            if (gameplayMap == null || callbacksBound)
            {
                return;
            }

            throwAction.performed += OnThrowPerformed;
            recall.performed += OnRecallPerformed;
            dodge.performed += OnDodgePerformed;
            restart.performed += OnRestartPerformed;
            callbacksBound = true;
            gameplayMap.Enable();
        }

        /// <summary>
        /// Unbinds gameplay callbacks and disables the configured Input System actions.
        /// </summary>
        private void UnbindAndDisable()
        {
            if (gameplayMap == null)
            {
                callbacksBound = false;
                return;
            }

            if (callbacksBound)
            {
                throwAction.performed -= OnThrowPerformed;
                recall.performed -= OnRecallPerformed;
                dodge.performed -= OnDodgePerformed;
                restart.performed -= OnRestartPerformed;
                callbacksBound = false;
            }

            gameplayMap.Disable();
        }

        /// <summary>
        /// Publishes the throw input event.
        /// </summary>
        private void OnThrowPerformed(InputAction.CallbackContext context) => ThrowPressed?.Invoke();
        /// <summary>
        /// Publishes the recall input event.
        /// </summary>
        private void OnRecallPerformed(InputAction.CallbackContext context) => RecallPressed?.Invoke();
        /// <summary>
        /// Publishes the dodge input event.
        /// </summary>
        private void OnDodgePerformed(InputAction.CallbackContext context) => DodgePressed?.Invoke();
        /// <summary>
        /// Publishes the terminal-state restart input event.
        /// </summary>
        private void OnRestartPerformed(InputAction.CallbackContext context) => RestartPressed?.Invoke();
    }
}
