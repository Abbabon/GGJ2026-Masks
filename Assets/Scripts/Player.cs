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

        _networkObject = GetComponent<NetworkObject>();
        _replicatedPosition = GetComponent<ReplicatedPosition>();
        _runner = UnityEngine.Object.FindObjectOfType<NetworkRunner>();
        _lobbyState = UnityEngine.Object.FindObjectOfType<LobbyState>();
    }

    /// <summary>True if we should control this character locally (input + Mover). When no network, always true for single-player. When networked, true only for the local Human.</summary>
    bool IsLocalHuman()
    {
        if (_runner == null) _runner = UnityEngine.Object.FindObjectOfType<NetworkRunner>();
        if (_lobbyState == null) _lobbyState = UnityEngine.Object.FindObjectOfType<LobbyState>();
        if (_networkObject == null) _networkObject = GetComponent<NetworkObject>();

        // No network (no runner, or no lobby, or game not started): control locally so single-player / editor works.
        if (_runner == null || _lobbyState == null || !_lobbyState.Id.IsValid || !_lobbyState.GameStarted)
            return true;
        // Networked: only the local Human controls this object.
        if (_networkObject == null) return true;
        if (_lobbyState.HumanPlayer != _runner.LocalPlayer) return false;
        if (!_runner.TryGetPlayerObject(_runner.LocalPlayer, out var myObj) || myObj != _networkObject) return false;
        return true;
    }

    /// <summary>True if this object is the Human's object on the other client (we should apply shared position from network).</summary>
    bool IsHumanProxy()
    {
        if (_runner == null || _lobbyState == null || _networkObject == null || !_lobbyState.Id.IsValid || !_lobbyState.GameStarted)
            return false;
        if (_lobbyState.HumanPlayer.IsNone) return false;
        if (!_runner.TryGetPlayerObject(_lobbyState.HumanPlayer, out var humanObj) || humanObj != _networkObject)
            return false;
        return true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (mover == null) return;
        mover.Stop();
        excludedDirection = lastMoveDirection;
    }

    void Update()
    {
        // When controlled by the other side (Human proxy), apply shared position to transform.
        if (!IsLocalHuman() && IsHumanProxy())
        {
            if (_replicatedPosition != null && _replicatedPosition.Id.IsValid)
                transform.position = _replicatedPosition.Position;
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
                actionable.ActionStop();
            actionPressedLastFrame = actionPressed;
        }
    }

    void LateUpdate()
    {
        // When we are the local Human, push our position to the network so the other side (God) gets it. Run after physics so transform is up to date.
        if (!IsLocalHuman()) return;
        if (_replicatedPosition == null) _replicatedPosition = GetComponent<ReplicatedPosition>();
        if (_replicatedPosition != null && _replicatedPosition.Id.IsValid)
            _replicatedPosition.Position = transform.position;
    }
}
