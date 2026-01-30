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

    void Start()
    {
        mover = GetComponent<Mover>();
        if (mover == null) mover = GetComponentInChildren<Mover>();

        actionable = GetComponent<Actionable>();
        if (actionable == null) actionable = GetComponentInChildren<Actionable>();

        _networkObject = GetComponent<NetworkObject>();
        _runner = UnityEngine.Object.FindObjectOfType<NetworkRunner>();
        _lobbyState = UnityEngine.Object.FindObjectOfType<LobbyState>();
    }

    /// <summary>True if this object is the local Human (we control it and replicate to network).</summary>
    bool IsLocalHuman()
    {
        if (_runner == null) _runner = UnityEngine.Object.FindObjectOfType<NetworkRunner>();
        if (_lobbyState == null) _lobbyState = UnityEngine.Object.FindObjectOfType<LobbyState>();
        if (_networkObject == null) _networkObject = GetComponent<NetworkObject>();
        if (_runner == null || _lobbyState == null || _networkObject == null || !_lobbyState.Id.IsValid || !_lobbyState.GameStarted)
            return false;
        if (_lobbyState.HumanPlayer != _runner.LocalPlayer) return false;
        if (!_runner.TryGetPlayerObject(_runner.LocalPlayer, out var myObj) || myObj != _networkObject) return false;
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
        // Only drive Mover when we are the local Human; otherwise transform comes from PlayerController.Render().
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
}
