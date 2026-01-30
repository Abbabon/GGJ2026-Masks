using UnityEngine;
using Fusion;
using Network;

public class GameManager : MonoBehaviour
{
    [SerializeField] OpeningMenu _openingMenu;
    [Tooltip("Player prefab (NetworkObject). Assigned to the GameLauncher when starting a room.")]
    [SerializeField] NetworkPrefabRef _playerPrefab;

    public OpeningMenu OpeningMenu { get => _openingMenu; set => _openingMenu = value; }
    public NetworkPrefabRef PlayerPrefab { get => _playerPrefab; set => _playerPrefab = value; }

    void Start()
    {
        if (_openingMenu != null)
            _openingMenu.startedRoom.AddListener(OnStartedRoom);
    }

    void OnStartedRoom(string roomName)
    {
        var go = new GameObject("GameLauncher");
        var launcher = go.AddComponent<GameLauncher>();
        launcher.SessionName = roomName;
        launcher.PlayerPrefab = _playerPrefab;

        if (_openingMenu != null)
            _openingMenu.gameObject.SetActive(false);
    }
}
