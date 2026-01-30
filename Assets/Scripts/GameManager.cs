using UnityEngine;
using Fusion;
using Network;

public class GameManager : MonoBehaviour
{
    [SerializeField] OpeningMenu _openingMenu;
    [SerializeField] SelectCharacterMenu _selectCharacterMenu;
    [Tooltip("Player prefab (NetworkObject). Assigned to the GameLauncher when starting a room.")]
    [SerializeField] NetworkPrefabRef _playerPrefab;

    public OpeningMenu OpeningMenu { get => _openingMenu; set => _openingMenu = value; }
    public SelectCharacterMenu SelectCharacterMenu { get => _selectCharacterMenu; set => _selectCharacterMenu = value; }
    public NetworkPrefabRef PlayerPrefab { get => _playerPrefab; set => _playerPrefab = value; }

    void Start()
    {
        if (_openingMenu != null)
            _openingMenu.startedRoom.AddListener(OnStartedRoom);
        if (_selectCharacterMenu != null)
            _selectCharacterMenu.onBothSelected.AddListener(OnStartGame);
    }

    void OnStartGame()
    {
        var launcher = FindObjectOfType<GameLauncher>();
        if (launcher != null)
            launcher.NotifyStartGame();
        if (_selectCharacterMenu != null)
            _selectCharacterMenu.gameObject.SetActive(false);
    }

    void OnStartedRoom(string roomName)
    {
        var go = new GameObject("GameLauncher");
        var launcher = go.AddComponent<GameLauncher>();
        launcher.SessionName = roomName;
        launcher.PlayerPrefab = _playerPrefab;
        launcher.onFullRoom.AddListener(() => OnFullRoom(launcher));
        launcher.onSelectCharacter.AddListener(OnSelectCharacter);

        if (_openingMenu != null)
            _openingMenu.gameObject.SetActive(false);
    }

    void OnSelectCharacter()
    {
        if (_selectCharacterMenu != null)
            _selectCharacterMenu.gameObject.SetActive(true);
    }

    void OnFullRoom(GameLauncher launcher)
    {
        if (launcher != null && launcher.gameObject != null)
            Destroy(launcher.gameObject);

        if (_openingMenu != null)
        {
            _openingMenu.gameObject.SetActive(true);
            _openingMenu.ClearInput();
        }
    }
}
