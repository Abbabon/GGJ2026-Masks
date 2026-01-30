using UnityEngine;
using Network;

public class GameManager : MonoBehaviour
{
    [SerializeField] OpeningMenu _openingMenu;
    [SerializeField] GameLauncher _gameLauncher;

    public OpeningMenu OpeningMenu { get => _openingMenu; set => _openingMenu = value; }
    public GameLauncher GameLauncher { get => _gameLauncher; set => _gameLauncher = value; }

    void Start()
    {
        if (_openingMenu != null)
            _openingMenu.startedRoom.AddListener(OnStartedRoom);
    }

    void OnStartedRoom(string roomName)
    {
        if (_gameLauncher != null)
        {
            _gameLauncher.SessionName = roomName;
            _gameLauncher.enabled = true;
        }

        if (_openingMenu != null)
            _openingMenu.gameObject.SetActive(false);
    }
}
