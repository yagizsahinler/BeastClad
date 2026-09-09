using System;
using UnityEngine;

namespace BeastClad.Input
{
    /// <summary>
    /// Decoupled interface for reading player input across movement and the 5 limb actions.
    /// Supports Unity New Input System, AI controllers, or debug replays.
    /// </summary>
    public interface IPlayerInput
    {
        /// <summary>
        /// Normalized 2D movement vector from input device.
        /// </summary>
        Vector2 MoveInput { get; }

        /// <summary>
        /// Fired when the Dash (Legs) action is triggered.
        /// </summary>
        event Action OnDashTriggered;

        /// <summary>
        /// Fired when the Left Arm primary attack is triggered.
        /// </summary>
        event Action OnLeftArmTriggered;

        /// <summary>
        /// Fired when the Right Arm secondary attack is triggered.
        /// </summary>
        event Action OnRightArmTriggered;

        /// <summary>
        /// Fired when the Chest defensive skill is triggered.
        /// </summary>
        event Action OnChestTriggered;

        /// <summary>
        /// Fired when the Head utility skill is triggered.
        /// </summary>
        event Action OnHeadTriggered;
    }
}
