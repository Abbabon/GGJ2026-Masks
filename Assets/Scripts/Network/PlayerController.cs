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
        public const byte RoleNone = 0;
        public const byte RoleGod = 1;
        public const byte RoleHuman = 2;

        [Header("Movement")]
        [Tooltip("Movement speed in units per second.")]
        [SerializeField] private float _moveSpeed = 5f;

        [Header("Input")]
        [Tooltip("Drag the InputSystem_Actions asset here.")]
        [SerializeField] private InputActionAsset _inputActions;

        [Header("Visual (Human tint)")]
        [Tooltip("Tint applied to sprite when role is Human. Ignored if no SpriteRenderer.")]
        [SerializeField] private Color _humanTint = new Color(0.85f, 0.75f, 0.65f, 1f);

        /// <summary>
        /// Networked position — Fusion replicates this to all peers automatically.
        /// </summary>
        [Networked] public Vector3 NetworkPosition { get; set; }

        /// <summary>
        /// Role assigned when game starts from lobby: RoleNone (0), RoleGod (1), RoleHuman (2).
        /// </summary>
        [Networked] public byte Role { get; set; }

        /// <summary>
        /// World position of the God cursor (mouse). Written by the God player; replicated to all peers for GodCursor visuals.
        /// </summary>
        [Networked] public Vector3 GodCursorWorldPosition { get; set; }

        private InputAction _moveAction;
        private byte _lastAppliedRole = byte.MaxValue;

        void ApplyRoleVisuals()
        {
            if (Role == RoleGod)
                gameObject.name = "God";
            else if (Role == RoleHuman)
            {
                gameObject.name = "Human";
                var sr = GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.color = _humanTint;
            }
        }

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
            _lastAppliedRole = byte.MaxValue;
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

            // Human movement and position sync are driven by Player.cs + ReplicatedPosition (no PlayerController reference).
            if (Role == RoleHuman)
                return;

            // God does not move this character with input.
            if (Role == RoleGod) return;

            Vector2 dir = _moveAction.ReadValue<Vector2>();
            if (dir.sqrMagnitude > 0f)
            {
                NetworkPosition += new Vector3(dir.x, dir.y, 0f) * _moveSpeed * Runner.DeltaTime;
            }
        }

        public override void Render()
        {
            // Human position is updated by Player.cs from ReplicatedPosition; don't overwrite transform here.
            if (Role == RoleHuman)
            {
                ApplyRoleVisualsIfChanged();
                return;
            }

            // Use Fusion's snapshot interpolation for smooth tick-to-tick blending (God / non-Human proxies).
            var interpolator = new NetworkBehaviourBufferInterpolator(this);
            if (interpolator)
            {
                transform.position = interpolator.Vector3(nameof(NetworkPosition));
            }
            else
            {
                transform.position = NetworkPosition;
            }

            ApplyRoleVisualsIfChanged();
        }

        void ApplyRoleVisualsIfChanged()
        {
            if (Role != _lastAppliedRole)
            {
                _lastAppliedRole = Role;
                ApplyRoleVisuals();
            }
        }
    }
}
