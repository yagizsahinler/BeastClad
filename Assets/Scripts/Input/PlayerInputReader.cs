using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BeastClad.Input
{
    /// <summary>
    /// Component-based Input Reader wrapping Unity's New Input System.
    /// Reads action maps via InputActionReference and emits clean C# events/values.
    /// Includes direct fallback polling via New Input System devices (Mouse/Keyboard).
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerInputReader : MonoBehaviour, IPlayerInput
    {
        [Header("Action References (New Input System)")]
        [Tooltip("Reference to the 2D Move action (e.g. Player/Move).")]
        [SerializeField] private InputActionReference moveAction;

        [Tooltip("Reference to the Dash action (e.g. Player/Sprint or Player/Dash).")]
        [SerializeField] private InputActionReference dashAction;

        [Tooltip("Reference to the Left Arm action (e.g. Player/Attack or Player/LeftArm).")]
        [SerializeField] private InputActionReference leftArmAction;

        [Tooltip("Reference to the Right Arm action (e.g. Player/RightArm).")]
        [SerializeField] private InputActionReference rightArmAction;

        [Tooltip("Reference to the Chest action (e.g. Player/Chest).")]
        [SerializeField] private InputActionReference chestAction;

        [Tooltip("Reference to the Head action (e.g. Player/Head).")]
        [SerializeField] private InputActionReference headAction;

        public Vector2 MoveInput { get; private set; }

        public event Action OnDashTriggered;
        public event Action OnLeftArmTriggered;
        public event Action OnRightArmTriggered;
        public event Action OnChestTriggered;
        public event Action OnHeadTriggered;

        private void OnEnable()
        {
            EnableAction(moveAction);
            EnableAction(dashAction, HandleDashPerformed);
            EnableAction(leftArmAction, HandleLeftArmPerformed);
            EnableAction(rightArmAction, HandleRightArmPerformed);
            EnableAction(chestAction, HandleChestPerformed);
            EnableAction(headAction, HandleHeadPerformed);
        }

        private void OnDisable()
        {
            DisableAction(moveAction);
            DisableAction(dashAction, HandleDashPerformed);
            DisableAction(leftArmAction, HandleLeftArmPerformed);
            DisableAction(rightArmAction, HandleRightArmPerformed);
            DisableAction(chestAction, HandleChestPerformed);
            DisableAction(headAction, HandleHeadPerformed);

            MoveInput = Vector2.zero;
        }

        private void Update()
        {
            // Read 2D Movement from InputAction
            Vector2 input = Vector2.zero;
            if (moveAction != null && moveAction.action != null && moveAction.action.enabled)
            {
                input = moveAction.action.ReadValue<Vector2>();
            }

            // Fallback direct polling via New Input System devices if actions are unassigned or produced zero
            if (input.sqrMagnitude < 0.001f)
            {
                var keyboard = Keyboard.current;
                if (keyboard != null)
                {
                    Vector2 kbMove = Vector2.zero;
                    if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) kbMove.y += 1f;
                    if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) kbMove.y -= 1f;
                    if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) kbMove.x -= 1f;
                    if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) kbMove.x += 1f;
                    input = Vector2.ClampMagnitude(kbMove, 1f);
                }

                var gamepad = Gamepad.current;
                if (gamepad != null && input.sqrMagnitude < 0.001f)
                {
                    Vector2 stick = gamepad.leftStick.ReadValue();
                    if (stick.sqrMagnitude > 0.04f)
                    {
                        input = Vector2.ClampMagnitude(stick, 1f);
                    }
                }
            }

            MoveInput = input;

            PollFallbacks();
        }

        private void PollFallbacks()
        {
            var mouse = Mouse.current;
            var keyboard = Keyboard.current;
            var gamepad = Gamepad.current;

            if (mouse != null)
            {
                if ((leftArmAction == null || leftArmAction.action == null) && mouse.leftButton.wasPressedThisFrame)
                {
                    OnLeftArmTriggered?.Invoke();
                }

                if ((rightArmAction == null || rightArmAction.action == null) && mouse.rightButton.wasPressedThisFrame)
                {
                    OnRightArmTriggered?.Invoke();
                }
            }

            if (keyboard != null)
            {
                if ((chestAction == null || chestAction.action == null) && keyboard.spaceKey.wasPressedThisFrame)
                {
                    OnChestTriggered?.Invoke();
                }

                if ((headAction == null || headAction.action == null) && keyboard.qKey.wasPressedThisFrame)
                {
                    OnHeadTriggered?.Invoke();
                }

                if ((dashAction == null || dashAction.action == null) && (keyboard.leftShiftKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame))
                {
                    OnDashTriggered?.Invoke();
                }
            }

            if (gamepad != null)
            {
                if (gamepad.buttonWest.wasPressedThisFrame) OnLeftArmTriggered?.Invoke();
                if (gamepad.buttonNorth.wasPressedThisFrame) OnRightArmTriggered?.Invoke();
                if (gamepad.buttonSouth.wasPressedThisFrame || gamepad.leftShoulder.wasPressedThisFrame) OnChestTriggered?.Invoke();
                if (gamepad.buttonEast.wasPressedThisFrame) OnHeadTriggered?.Invoke();
                if (gamepad.rightTrigger.wasPressedThisFrame) OnDashTriggered?.Invoke();
            }
        }

        private void EnableAction(InputActionReference actionRef, Action<InputAction.CallbackContext> callback = null)
        {
            if (actionRef != null && actionRef.action != null)
            {
                actionRef.action.Enable();
                if (callback != null) actionRef.action.performed += callback;
            }
        }

        private void DisableAction(InputActionReference actionRef, Action<InputAction.CallbackContext> callback = null)
        {
            if (actionRef != null && actionRef.action != null)
            {
                if (callback != null) actionRef.action.performed -= callback;
                actionRef.action.Disable();
            }
        }

        private void HandleDashPerformed(InputAction.CallbackContext context) => OnDashTriggered?.Invoke();
        private void HandleLeftArmPerformed(InputAction.CallbackContext context) => OnLeftArmTriggered?.Invoke();
        private void HandleRightArmPerformed(InputAction.CallbackContext context) => OnRightArmTriggered?.Invoke();
        private void HandleChestPerformed(InputAction.CallbackContext context) => OnChestTriggered?.Invoke();
        private void HandleHeadPerformed(InputAction.CallbackContext context) => OnHeadTriggered?.Invoke();
    }
}
