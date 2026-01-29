# Photon Fusion 2 Reference

Quick reference for Photon Fusion 2 networking in this Unity project. Based on official docs at https://doc.photonengine.com/fusion/current/fusion-2-intro

## Setup

1. Download Fusion 2 SDK from Photon website, import the `.unitypackage`
2. Create a Fusion AppId at the Photon Dashboard (select Fusion → Fusion 2)
3. Paste AppId into the Fusion Hub window in Unity (auto-opens after import)
4. A `NetworkProjectConfig` asset is created in Assets — this holds tick rate, physics, and player count settings

## Network Topologies

Fusion supports three multiplayer modes:

| Mode | GameMode | Authority | Use Case |
|------|----------|-----------|----------|
| **Shared** | `GameMode.Shared` | Distributed — each client owns objects it spawns | Simpler games, co-op, PUN-like workflow |
| **Host** | `GameMode.Host` / `GameMode.Client` | Host has full state authority | Competitive games without dedicated servers |
| **Dedicated Server** | `GameMode.Server` / `GameMode.Client` | Server has full state authority | Anti-cheat, authoritative simulation |

`GameMode.AutoClientOrHost` — first player becomes host, others join as clients.
`GameMode.Single` — local single-player with network code still running.

## Core Components

### NetworkRunner

The central Fusion component. Manages connections, simulation ticks, spawning, and state replication. One per peer.

```csharp
var runner = gameObject.AddComponent<NetworkRunner>();
await runner.StartGame(new StartGameArgs
{
    GameMode = GameMode.Shared,
    Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
    SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
});
```

**Important:** A NetworkRunner can only be used once. After disconnect, destroy it and create a new one.

### NetworkObject

Placed on any prefab/GameObject that needs network identity. Assigns a unique `NetworkId` across all peers.

- Prefabs with NetworkObject are auto-registered in Fusion's Object Table
- Use `Runner.Spawn()` to instantiate — **never** use `GameObject.Instantiate()` for networked objects
- Access via `Runner.TryFindObject(networkId, out var obj)`

### NetworkBehaviour

Extends MonoBehaviour with networking. Provides `[Networked]` properties, RPCs, and network lifecycle callbacks.

```csharp
public class PlayerHealth : NetworkBehaviour
{
    [Networked] public float Health { get; set; } = 100;

    public override void Spawned() { /* replaces Start() */ }
    public override void FixedUpdateNetwork() { /* simulation tick */ }
    public override void Render() { /* visual interpolation, runs every frame */ }
    public override void Despawned(NetworkRunner runner, bool hasState) { /* cleanup */ }
}
```

### NetworkTransform

Synchronizes position/rotation across the network. Add to any NetworkObject that moves.

- Optional: sync scale (`Sync Scale`) and parent (`Sync Parent`)
- `Teleport(pos, rot)` — instant move without interpolation

### NetworkMecanimAnimator

Syncs Unity Animator parameters from State Authority to proxies. Cannot be rewound/resimulated.

- Use its `SetTrigger()` method instead of `Animator.SetTrigger()` directly
- In Shared Mode, only modify animator in `FixedUpdateNetwork()` when `HasStateAuthority`

## Networked Properties

Properties marked `[Networked]` replicate from State Authority to all peers.

```csharp
[Networked] public int Score { get; set; }
[Networked] public Vector3 Target { get; set; }
[Networked] public NetworkBool IsReady { get; set; }
```

**Rules:**
- Must be auto-properties (`{ get; set; }`) — no custom logic in accessors
- Modify in `FixedUpdateNetwork()` for proper tick-accuracy and prediction
- Cannot access until `Spawned()` is called
- Only State Authority changes are replicated; other peers' changes get overwritten

### Supported Types

- Primitives: `byte, int, float, double`, etc.
- Unity: `Vector2/3/4, Quaternion, Color, Bounds, Rect`
- Fusion: `NetworkBool, NetworkString<_32>, PlayerRef, NetworkId, TickTimer`
- Collections (need `[Capacity]`): `NetworkArray<T>, NetworkDictionary<K,V>, NetworkLinkedList<T>`
- Custom `INetworkStruct` implementations
- References to `NetworkObject` / `NetworkBehaviour` (stored as NetworkId internally)

**No:** `string` (use `NetworkString<_N>`), `bool` (use `NetworkBool`), `char`, reference types, generics

### Collections

```csharp
[Networked, Capacity(8)]
public NetworkArray<int> Inventory { get; }

[Networked, Capacity(16)]
public NetworkDictionary<PlayerRef, int> PlayerScores { get; }

[Networked, Capacity(32)]
public NetworkString<_32> PlayerName { get; set; }
```

### Change Detection

```csharp
[Networked, OnChangedRender(nameof(OnHealthChanged))]
public float Health { get; set; }

void OnHealthChanged()
{
    healthBar.fillAmount = Health / MaxHealth;
}
```

`OnChangedRender` fires during render frames. **Not** called on initial spawn — initialize visuals in `Spawned()`.

To read previous value:

```csharp
void OnHealthChanged(NetworkBehaviourBuffer previous)
{
    var prev = GetPropertyReader<float>(nameof(Health)).Read(previous);
}
```

## INetworkStruct

Custom blittable structs for networked properties and RPC parameters.

```csharp
public struct DamageInfo : INetworkStruct
{
    public float Amount;
    public PlayerRef Source;
    public NetworkBool IsCritical;
    // No string, bool, char, or reference types allowed
}
```

Use `ref` pattern for direct modification without copying:

```csharp
[Networked]
public ref DamageInfo LastDamage => ref MakeRef<DamageInfo>();
```

## RPCs (Remote Procedure Calls)

For one-time events, not continuous state. Must have "RPC" in method name.

```csharp
[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
public void RPC_DealDamage(float damage)
{
    Health -= damage;  // Runs on State Authority only
}

[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
public void RPC_ShowEffect(Vector3 position)
{
    // Runs on all clients
}
```

**Sources:** `All, Proxies, InputAuthority, StateAuthority`
**Targets:** `All, Proxies, InputAuthority, StateAuthority`
**Options:** `Channel = RpcChannel.Reliable|Unreliable`, `InvokeLocal = true|false`

Supports: primitives, Unity types, INetworkStruct, Fusion types, strings, arrays.

## Spawning & Despawning

```csharp
// Spawn with input authority assigned to a player
var obj = Runner.Spawn(prefab, position, rotation, inputAuthority,
    (runner, o) => {
        // OnBeforeSpawned: init before replication
        o.GetComponent<PlayerData>().Name = playerName;
    });

// Despawn (State Authority only)
Runner.Despawn(obj);
```

- **Host/Server Mode:** Only host/server can spawn; clients request via RPC
- **Shared Mode:** Any client can spawn; spawner gets State Authority

### Player Objects

```csharp
Runner.SetPlayerObject(playerRef, networkObject);
Runner.TryGetPlayerObject(playerRef, out var obj);
```

## Simulation Loop

Fusion uses a **tick-based simulation** independent of frame rate. Tick rate set in `NetworkProjectConfig`.

- `FixedUpdateNetwork()` — runs per tick, where gameplay/simulation code goes
- `Render()` — runs per frame, for visual interpolation and non-simulation rendering
- `Runner.DeltaTime` — time per tick (not frame delta)

In Host/Server mode, clients predict ahead and resimulate on server state arrival. Keep state derivable (not progressively accumulated) to handle resimulation correctly.

## Matchmaking

```csharp
await runner.StartGame(new StartGameArgs
{
    GameMode = GameMode.Shared,
    SessionName = "my-room",          // omit for random/auto
    PlayerCount = 4,
    SessionProperties = new Dictionary<string, SessionProperty>
    {
        { "map", (int)MapType.Forest },
    },
});
```

- Omit `SessionName` for random matchmaking
- `SessionProperties` for filtered matching (map, mode, etc.)
- `Runner.SessionInfo` exposes room data after connection
- Browse lobbies with `Runner.JoinSessionLobby()` and `OnSessionListUpdated()` callback

## Scene Loading

```csharp
if (Runner.IsSceneAuthority)
{
    Runner.LoadScene(SceneRef.FromIndex(buildIndex), LoadSceneMode.Additive);
    Runner.UnloadScene(SceneRef.FromIndex(buildIndex));
}
```

- Requires a scene manager (`NetworkSceneManagerDefault` or custom `INetworkSceneManager`)
- Only Server/Host or Master Client (Shared Mode) can load/unload scenes
- Supports up to 8 additive scenes

## Shared Mode: Master Client

One player in Shared Mode has elevated privileges:

- Check with `Runner.IsSharedModeMasterClient`
- Can load/unload scenes
- Has State Authority over scene-baked NetworkObjects
- Auto-transfers on disconnect; manual transfer via `Runner.SetMasterClient(player)`

## Physics (2D)

Fusion supports 2D physics via `NetworkTransform` with Rigidbody2D:

- **Forecast Physics** (extrapolation): all clients run physics locally, reconcile with network. Set in NetworkProjectConfig and per-NetworkTransform. Write physics code in `FixedUpdate()` (Unity's, not Fusion's).
- **Authority-only** (forecast disabled): only authority simulates, proxies get synced positions. Proxy rigidbodies auto-set to kinematic.
- `RigidbodyConstraints2D` and `isKinematic` sync automatically; other properties (mass, drag) need manual sync.

## Authority Checks

```csharp
HasStateAuthority   // This peer controls the object's networked state
HasInputAuthority   // This peer provides input for this object (Host/Server mode)
IsProxy             // This peer has neither authority — just receiving state
Runner.LocalPlayer  // This peer's PlayerRef
```

## Common Patterns

### Shared Mode Player Controller

```csharp
public class PlayerController : NetworkBehaviour
{
    [Networked] public NetworkString<_32> PlayerName { get; set; }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;  // Only owner controls

        var move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        transform.position += move * Runner.DeltaTime * Speed;
    }
}
```

### Cross-Player Interaction via RPC

```csharp
// Any client can call this on another player's object
[Rpc(RpcSources.All, RpcTargets.StateAuthority)]
public void RPC_TakeDamage(float amount)
{
    Health -= amount;  // Executes on owning client
}
```

## Key Gotchas

- **Never** use `GameObject.Instantiate()` for networked objects — always `Runner.Spawn()`
- **Never** reuse a NetworkRunner after disconnect — destroy and create new
- Networked properties must be auto-properties — no custom get/set logic
- `OnChangedRender` not called on initial spawn — init visuals in `Spawned()`
- `bool` is not reliably blittable cross-platform — use `NetworkBool` or `int`
- `string` is not supported — use `NetworkString<_N>`
- In Shared Mode, only modify your own objects' networked properties; use RPCs for others'
- RPCs are for punctual events, not continuous state sync — use `[Networked]` for ongoing state
