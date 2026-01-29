using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Network
{
    /// <summary>
    /// Moves the player character each simulation tick (Shared Mode).
    ///
    /// Position is replicated via a [Networked] property rather than NetworkTransform,
    /// so no extra components are needed beyond NetworkObject.
    ///
    /// - State authority peer: reads input, updates NetworkPosition, applies to transform.
    /// - Proxy peers: NetworkPosition arrives via replication, applied to transform in Render().
    ///
    /// Requires on the same prefab:
    ///   - NetworkObject (network identity)
    /// </summary>
    public class PlayerController : NetworkBehaviour
    {
        [Header("Movement")]
        [Tooltip("Movement speed in units per second.")]
        [SerializeField] private float _moveSpeed = 5f;

        [Header("Input")]
        [Tooltip("Drag the InputSystem_Actions asset here.")]
        [SerializeField] private InputActionAsset _inputActions;

        /// <summary>
        /// Networked position — Fusion replicates this to all peers automatically.
        /// </summary>
        [Networked] public Vector3 NetworkPosition { get; set; }

        private InputAction _moveAction;

        public override void Spawned()
        {
            // Initialize networked position from spawn location.
            if (HasStateAuthority)
            {
                NetworkPosition = transform.position;
                _moveAction = _inputActions.FindActionMap("Player").FindAction("Move");
                _inputActions.Enable();
            }

            // Sync visual position for all peers on spawn.
            transform.position = NetworkPosition;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (_moveAction != null)
            {
                _inputActions.Disable();
                _moveAction = null;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority) return;

            Vector2 dir = _moveAction.ReadValue<Vector2>();
            if (dir.sqrMagnitude > 0f)
            {
                NetworkPosition += new Vector3(dir.x, dir.y, 0f) * _moveSpeed * Runner.DeltaTime;
            }
        }

        /// <summary>
        /// Runs every frame. Smoothly moves the visual toward the networked position
        /// so proxies see fluid movement rather than tick-rate snapping.
        /// </summary>
        public override void Render()
        {
            transform.position = Vector3.Lerp(transform.position, NetworkPosition, 15f * Time.deltaTime);
        }
    }
}
