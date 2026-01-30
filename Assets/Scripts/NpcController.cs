using UnityEngine;
using Fusion;
using Network;

/// <summary>
/// NPC movement: when this client has state authority (Human who spawned this NPC), run movement and SEND position to network.
/// When role = God (no authority), GET transform from network. No PlayerController — uses ReplicatedPosition only.
///
/// NPC PREFAB MUST HAVE:
/// - Network Object (Photon Fusion)
/// - Replicated Position (script in Network folder)
/// - Npc (this script)
/// - Mover (optional, for movement)
/// - Rigidbody2D (optional, for physics; ReplicatedPosition uses it for sync)
/// NPCs must be SPAWNED by the Human client (e.g. when game starts) so the Human has state authority and can SEND positions.
/// </summary>
public class Npc : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] float speedVariance = 1f;

    [Header("Walk duration")]
    [SerializeField] float moveDurationSeconds = 3f;
    [SerializeField] float moveDurationVariance = 0.5f;

    [Header("Pause before next move")]
    [SerializeField] float idleTimeoutSeconds = 1f;
    [SerializeField] float idleTimeoutVariance = 0.5f;

    Mover mover;
    float moveTimer;
    float idleTimer;
    bool isIdle;
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
        if (mover == null)
            mover = GetComponentInChildren<Mover>();

        _rb = GetComponent<Rigidbody2D>();
        _networkObject = GetComponent<NetworkObject>();
        _replicatedPosition = GetComponent<ReplicatedPosition>();
        _runner = UnityEngine.Object.FindObjectOfType<NetworkRunner>();
        _lobbyState = UnityEngine.Object.FindObjectOfType<LobbyState>();

        if (mover == null) return;

        float baseSpeed = mover.Speed;
        mover.Speed = baseSpeed + Random.Range(-speedVariance, speedVariance);

        StartIdle();
    }

    /// <summary>True when the game has started (both players selected and Start clicked). NPCs only move after this.</summary>
    bool GameHasStarted()
    {
        if (_runner == null) _runner = UnityEngine.Object.FindObjectOfType<NetworkRunner>();
        if (_lobbyState == null) _lobbyState = UnityEngine.Object.FindObjectOfType<LobbyState>();
        // No network: allow movement (single-player / editor).
        if (_runner == null || _lobbyState == null || !_lobbyState.Id.IsValid)
            return true;
        return _lobbyState.GameStarted;
    }

    /// <summary>True if we have state authority over this NPC (Human who spawned it) — we SEND position. Otherwise (God) we GET position.</summary>
    bool HasNpcAuthority()
    {
        if (_runner == null) _runner = UnityEngine.Object.FindObjectOfType<NetworkRunner>();
        if (_lobbyState == null) _lobbyState = UnityEngine.Object.FindObjectOfType<LobbyState>();
        if (_networkObject == null) _networkObject = GetComponent<NetworkObject>();

        // No network: run locally (single-player / editor).
        if (_runner == null || _lobbyState == null || !_lobbyState.Id.IsValid || !_lobbyState.GameStarted)
            return true;
        if (_networkObject == null || !_networkObject.Id.IsValid)
            return true;
        // We send when we have state authority over this NPC (Human spawns NPCs, so Human has authority).
        return _networkObject.StateAuthority == _runner.LocalPlayer;
    }

    void Update()
    {
        // Only move / sync after game has started (both players selected and Start clicked).
        if (!GameHasStarted()) return;

        // God: GET transform from network (apply replicated position).
        if (!HasNpcAuthority())
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

        // Human (or no network): run movement and SEND in LateUpdate.
        if (mover == null) return;

        if (isIdle)
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f)
                StartRandomMove();
        }
        else
        {
            moveTimer -= Time.deltaTime;
            if (moveTimer <= 0f)
            {
                mover.Stop();
                StartIdle();
            }
        }
    }

    void LateUpdate()
    {
        if (!GameHasStarted()) return;
        // Human (authority): SEND our position to the network.
        if (!HasNpcAuthority()) return;
        if (_replicatedPosition == null) _replicatedPosition = GetComponent<ReplicatedPosition>();
        if (_replicatedPosition != null && _replicatedPosition.Id.IsValid)
        {
            var pos = _rb != null ? (Vector3)_rb.position : transform.position;
            pos.z = transform.position.z;
            _replicatedPosition.Position = pos;
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (mover == null) return;

        mover.Stop();
        excludedDirection = lastMoveDirection;
        StartIdle();
    }

    void StartIdle()
    {
        isIdle = true;
        float duration = idleTimeoutSeconds + Random.Range(-idleTimeoutVariance, idleTimeoutVariance);
        idleTimer = Mathf.Max(0.01f, duration);
    }

    void StartRandomMove()
    {
        if (mover == null) return;

        isIdle = false;
        Vector2 dir = PickDirectionExcluding(excludedDirection);
        excludedDirection = null;
        lastMoveDirection = dir;
        mover.Move(dir);

        float duration = moveDurationSeconds + Random.Range(-moveDurationVariance, moveDurationVariance);
        moveTimer = Mathf.Max(0.01f, duration);
    }

    // 8 directions at 45° intervals (right, up-right, up, up-left, left, down-left, down, down-right)
    static readonly Vector2[] Directions45 = new Vector2[]
    {
        new Vector2(1f, 0f), new Vector2(0.707f, 0.707f), new Vector2(0f, 1f), new Vector2(-0.707f, 0.707f),
        new Vector2(-1f, 0f), new Vector2(-0.707f, -0.707f), new Vector2(0f, -1f), new Vector2(0.707f, -0.707f)
    };

    Vector2 PickDirectionExcluding(Vector2? exclude)
    {
        if (exclude == null)
            return Directions45[Random.Range(0, Directions45.Length)];

        Vector2 ex = exclude.Value.normalized;
        int excludeIndex = 0;
        float bestDot = Vector2.Dot(Directions45[0], ex);
        for (int i = 1; i < Directions45.Length; i++)
        {
            float d = Vector2.Dot(Directions45[i], ex);
            if (d > bestDot) { bestDot = d; excludeIndex = i; }
        }

        int count = Directions45.Length - 1;
        int pick = Random.Range(0, count);
        for (int i = 0; i < Directions45.Length; i++)
        {
            if (i == excludeIndex) continue;
            if (pick == 0) return Directions45[i];
            pick--;
        }
        return Directions45[0];
    }
}
