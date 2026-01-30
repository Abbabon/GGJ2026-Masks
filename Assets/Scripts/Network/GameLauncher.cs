using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Network
{
    /// <summary>
    /// Manages the Photon Fusion 2 session lifecycle (Shared Mode for WebGL compatibility).
    ///
    /// Responsibilities:
    ///   - Creates a NetworkRunner and starts the game in Shared mode.
    ///   - Each player spawns their own character on join (distributed authority).
    ///   - Despawns the player prefab when a player leaves.
    ///   - Collects local input each tick via Unity's Input System and feeds it to Fusion.
    ///
    /// Setup:
    ///   1. Attach to an empty GameObject in the scene.
    ///   2. Assign _playerPrefab (must have NetworkObject + NetworkTransform + PlayerController).
    ///   3. Ensure the scene is in Build Settings (File > Build Settings > Add Open Scenes).
    /// </summary>
    public class GameLauncher : MonoBehaviour, INetworkRunnerCallbacks
    {
        [Header("Player Setup")]
        [Tooltip("Prefab with NetworkObject, NetworkTransform, and PlayerController components. Can be set from GameManager.")]
        [SerializeField] private NetworkPrefabRef _playerPrefab;

        public NetworkPrefabRef PlayerPrefab { get => _playerPrefab; set => _playerPrefab = value; }

        [Tooltip("World position where newly joined players spawn.")]
        [SerializeField] private Vector2 _spawnPoint = Vector2.zero;

        [Header("Session")]
        [Tooltip("Photon session/room name. All players must use the same name to connect.")]
        [SerializeField] private string _sessionName = "default-room";

        /// <summary>Session/room name used when starting the game. Set before enabling this GameObject if using a menu flow.</summary>
        public string SessionName { get => _sessionName; set => _sessionName = value ?? ""; }

        [Header("Callbacks")]
        [Tooltip("Invoked when the room is full (e.g. cannot join). Use to show opening menu again and clear input.")]
        public UnityEvent onFullRoom = new UnityEvent();

        [Tooltip("Invoked when buildIndex is 0 or 1 (e.g. lobby). Use to show select character menu.")]
        public UnityEvent onSelectCharacter = new UnityEvent();

        private NetworkRunner _runner;

        private async void Start()
        {
            await Launch();
        }

        private async System.Threading.Tasks.Task Launch()
        {
            // NetworkRunner can only be used once — create a fresh one each time.
            var runnerGO = new GameObject("NetworkRunner");
            _runner = runnerGO.AddComponent<NetworkRunner>();
            _runner.AddCallbacks(this);

            var sceneManager = runnerGO.AddComponent<NetworkSceneManagerDefault>();

            // Build NetworkSceneInfo from the active scene's build index.
            var sceneInfo = new NetworkSceneInfo();
            var buildIndex = SceneManager.GetActiveScene().buildIndex;
            
            if (buildIndex >= 2)
            {
                Debug.Log("Full room");
                if (runnerGO != null)
                    Destroy(runnerGO);
                onFullRoom?.Invoke();
                return;
            }
            if (buildIndex >= 0)
            {
                sceneInfo.AddSceneRef(SceneRef.FromIndex(buildIndex));
            }

            if (buildIndex <= 1)
                onSelectCharacter?.Invoke();

            var result = await _runner.StartGame(new StartGameArgs
            {
                GameMode     = GameMode.Shared,
                SessionName  = _sessionName,
                Scene        = sceneInfo,
                SceneManager = sceneManager,
            });

            if (result.Ok)
            {
                Debug.Log($"[GameLauncher] Fusion started. Mode: {_runner.GameMode}");
            }
            else
            {
                Debug.LogError($"[GameLauncher] Fusion failed to start: {result.ShutdownReason}");
            }
        }

        // =========================================================================
        // INetworkRunnerCallbacks — Session lifecycle
        // =========================================================================

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            // In Shared Mode, each peer spawns their own player object.
            if (player != runner.LocalPlayer) return;

            Vector3 spawnPos = new Vector3(_spawnPoint.x, _spawnPoint.y, 0f);
            NetworkObject playerObject = runner.Spawn(
                _playerPrefab,
                spawnPos,
                Quaternion.identity,
                player // this peer gets StateAuthority over its own object
            );
            runner.SetPlayerObject(player, playerObject);
            Debug.Log($"[GameLauncher] Spawned local player for {player}");
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (runner.TryGetPlayerObject(player, out NetworkObject playerObject))
            {
                if (playerObject.HasStateAuthority)
                {
                    runner.Despawn(playerObject);
                    Debug.Log($"[GameLauncher] Despawned player for {player}");
                }
            }
        }

        // =========================================================================
        // INetworkRunnerCallbacks — Input
        // =========================================================================

        /// <summary>
        /// Called every tick on the local peer. Reads the Unity Input System
        /// and packs the result into PlayerInputData for Fusion to send to the host.
        /// </summary>
        public void OnInput(NetworkRunner runner, NetworkInput input) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Debug.Log($"[GameLauncher] Shutdown: {shutdownReason}");
        }

        // =========================================================================
        // INetworkRunnerCallbacks — Unused (required by interface)
        // =========================================================================

        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }

        private void OnDestroy() { }
    }
}
