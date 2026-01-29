using Fusion;
using UnityEngine;

namespace Network
{
    /// <summary>
    /// Network input payload sent from the input-authority client to the host each tick.
    /// Fusion IL-weaves this struct automatically — no manual registration needed.
    ///
    /// Add new fields here as gameplay requires (e.g., NetworkBool Attack).
    /// Every field increases per-tick bandwidth, so keep it minimal.
    /// </summary>
    public struct PlayerInputData : INetworkInput
    {
        /// <summary>
        /// Movement direction from the local player's input device.
        /// Read from InputSystem_Actions.Player.Move (Vector2).
        /// Magnitude 0..1 for analog sticks, exactly 0 or 1 for keyboard.
        /// </summary>
        public Vector2 MoveDirection;
    }
}
