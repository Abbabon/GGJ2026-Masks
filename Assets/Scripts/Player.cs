using UnityEngine;
using Fusion;
using Network;

public class Player : MonoBehaviour
{
    [SerializeField] KeyCode actionKey = KeyCode.E;
    [Tooltip("Input in this direction is ignored after hitting a trigger until player chooses another direction.")]
    [SerializeField] float blockedDirectionDotThreshold = 0.7f;
    Mover mover;
    Actionable actionable;
    bool actionPressedLastFrame;
    Vector2 lastMoveDirection;
    Vector2? excludedDirection;

    Rigidbody2D _rb;
    NetworkRunner _runner;
    LobbyState _lobbyState;
    NetworkObject _networkObject;
    ReplicatedPosition _replicatedPosition;


    void Start()
    {
        mover = GetComponent<Mover>();
        if (mover == null) mover = GetComponentInChildren<Mover>();

        actionable = GetComponent<Actionable>();
        if (actionable == null) actionable = GetComponentInChildren<Actionable>();

        actionable.onActionComplete.AddListener(() =>
        {
            Debug.Log("ActionComplete");
        });
        _networkObject = GetComponent<NetworkObject>();
        _replicatedPosition = GetComponent<ReplicatedPosition>();
        _rb = GetComponent<Rigidbody2D>();
        _runner = UnityEngine.Object.FindObjectOfType<NetworkRunner>();
        _lobbyState = UnityEngine.Object.FindObjectOfType<LobbyState>();
    }

    /// <summary>True if we should control this character locally (input + Mover). No PlayerController — uses LobbyState + StateAuthority only.</summary>
    bool IsLocalHuman()
    {
        if (_runner == null) _runner = UnityEngine.Object.FindObjectOfType<NetworkRunner>();
        if (_lobbyState == null) _lobbyState = UnityEngine.Object.FindObjectOfType<LobbyState>();
        if (_networkObject == null) _networkObject = GetComponent<NetworkObject>();

        // No network: control locally so single-player / editor works.
        if (_runner == null || _lobbyState == null || !_lobbyState.Id.IsValid || !_lobbyState.GameStarted)
            return true;
        // No NetworkObject: treat as local (e.g. non-networked prefab).
        if (_networkObject == null || !_networkObject.Id.IsValid)
            return true;
        // We are the local Human if we're the Human player AND we have state authority over this object (we spawned it).
        return _lobbyState.HumanPlayer == _runner.LocalPlayer && _networkObject.StateAuthority == _runner.LocalPlayer;
    }

    /// <summary>True if this object is the Human's object (apply shared position from network when we're not the local Human).</summary>
    bool IsHumanProxy()
    {
        if (_runner == null || _lobbyState == null || _networkObject == null || !_networkObject.Id.IsValid)
            return false;
        if (!_lobbyState.Id.IsValid || !_lobbyState.GameStarted || _lobbyState.HumanPlayer.IsNone)
            return false;
        return _networkObject.StateAuthority == _lobbyState.HumanPlayer;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (mover == null) return;
        mover.Stop();
        excludedDirection = lastMoveDirection;
    }

    void Update()
    {
        // When controlled by the other side (Human proxy), apply shared position (use Rigidbody2D so physics doesn't overwrite).
        if (!IsLocalHuman() && IsHumanProxy())
        {
            if (_replicatedPosition != null && _replicatedPosition.Id.IsValid)
            {
                var pos = _replicatedPosition.Position;
                if (_rb != null)
                    _rb.position = new Vector2(pos.x, pos.y);
                else
                    transform.position = pos;
            }
            return;
        }

        // Only drive Mover when we are the local Human.
        if (!IsLocalHuman())
            return;

        if (mover != null)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector2 dir = new Vector2(h, v);

            if (dir.sqrMagnitude > 0.01f)
            {
                Vector2 normalized = dir.normalized;
                if (excludedDirection.HasValue &&
                    Vector2.Dot(normalized, excludedDirection.Value) >= blockedDirectionDotThreshold)
                {
                    mover.Stop();
                }
                else
                {
                    excludedDirection = null;
                    lastMoveDirection = normalized;
                    mover.Move(normalized);
                }
            }
            else
            {
                mover.Stop();
            }
        }

        if (actionable != null)
        {
            bool actionPressed = Input.GetKey(actionKey);
            if (actionPressed && !actionPressedLastFrame)
                actionable.ActionStart();
            else if (!actionPressed && actionPressedLastFrame)
            {
                actionable.ActionStop();
            }
                
            actionPressedLastFrame = actionPressed;
        }
    }

    void LateUpdate()
    {
        // When we are the local Human, push our position to the network (use Rigidbody2D if present so we're in sync with Mover).
        if (!IsLocalHuman()) return;
        if (_replicatedPosition == null) _replicatedPosition = GetComponent<ReplicatedPosition>();
        if (_replicatedPosition != null && _replicatedPosition.Id.IsValid)
        {
            var pos = _rb != null ? (Vector3)_rb.position : transform.position;
            pos.z = transform.position.z;
            _replicatedPosition.Position = pos;
        }
    }
}
