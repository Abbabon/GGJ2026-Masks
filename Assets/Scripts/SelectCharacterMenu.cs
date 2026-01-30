using Fusion;
using Network;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SelectCharacterMenu : MonoBehaviour
{
    public const int RoleNone = 0;
    public const int RoleGod = 1;
    public const int RoleHuman = 2;

    [Header("Lobby")]
    [Tooltip("Prefab with NetworkObject + LobbyState. Assign the prefab asset here (drag LobbyState prefab). Alternatively use Lobby State Prefab Ref if set in Fusion config.")]
    [SerializeField] NetworkObject _lobbyStatePrefabObject;

    [Tooltip("Optional: use if LobbyState is registered in Fusion Network Project Config. If set, takes precedence over Lobby State Prefab Object.")]
    [SerializeField] NetworkPrefabRef _lobbyStatePrefabRef;

    [Header("Callbacks")]
    [Tooltip("Invoked when the user selects God or Human. Argument: RoleGod (1) or RoleHuman (2).")]
    public UnityEvent<int> onRoleSelected = new UnityEvent<int>();

    [Tooltip("Invoked when the 'Button' (ready) is clicked, after both God and Human are selected.")]
    public UnityEvent onBothSelected = new UnityEvent();

    const string GodButtonName = "God";
    const string HumanButtonName = "Human";
    const string ReadyButtonName = "Button";

    NetworkRunner _runner;
    LobbyState _lobbyState;
    Button _godButton;
    Button _humanButton;
    Button _readyButton;
    int _myChoice = RoleNone;

    /// <summary>Which role the local player selected: RoleNone (0), RoleGod (1), or RoleHuman (2).</summary>
    public int MyChoice => _myChoice;

    public bool IsGodSelected => _myChoice == RoleGod;
    public bool IsHumanSelected => _myChoice == RoleHuman;

    void OnEnable()
    {
        _runner = FindObjectOfType<NetworkRunner>();
        if (_runner == null)
        {
            Debug.Log("[SelectCharacterMenu] OnEnable: No NetworkRunner found.");
            return;
        }

        EnsureLobbyState();
        FindButtons();

        Debug.Log($"[SelectCharacterMenu] OnEnable: GodButton={_godButton != null}, HumanButton={_humanButton != null}, ReadyButton={_readyButton != null}, LobbyState={_lobbyState != null}");

        if (_readyButton != null)
            _readyButton.interactable = false;

        SubscribeButtons();
    }

    void OnDisable()
    {
        UnsubscribeButtons();
    }

    void Update()
    {
        if (_lobbyState == null) return;

        // Disable when role is taken (network) OR when local player already chose that role (so we don't re-enable before RPC replicates)
        bool godAvailable = _lobbyState.GodPlayer.IsNone && _myChoice != RoleGod;
        bool humanAvailable = _lobbyState.HumanPlayer.IsNone && _myChoice != RoleHuman;

        if (_godButton != null)
        {
            if (_godButton.interactable != godAvailable)
                Debug.Log($"[SelectCharacterMenu] Update: God interactable={godAvailable} (GodPlayer.IsNone={_lobbyState.GodPlayer.IsNone}, _myChoice={_myChoice})");
            _godButton.interactable = godAvailable;
        }
        if (_humanButton != null)
        {
            if (_humanButton.interactable != humanAvailable)
                Debug.Log($"[SelectCharacterMenu] Update: Human interactable={humanAvailable} (HumanPlayer.IsNone={_lobbyState.HumanPlayer.IsNone}, _myChoice={_myChoice})");
            _humanButton.interactable = humanAvailable;
        }

        if (_readyButton != null)
            _readyButton.interactable = _lobbyState.BothSelected;
    }

    void TryEnsureRunnerAndLobbyState()
    {
        if (_runner == null)
            _runner = FindObjectOfType<NetworkRunner>();
        if (_runner != null && _lobbyState == null)
            EnsureLobbyState();
    }

    void EnsureLobbyState()
    {
        _lobbyState = FindObjectOfType<LobbyState>();
        if (_lobbyState != null)
        {
            Debug.Log("[SelectCharacterMenu] EnsureLobbyState: Found existing LobbyState.");
            return;
        }

        // Prefer NetworkPrefabRef (from Fusion config) if valid; otherwise use direct prefab reference
        if (_lobbyStatePrefabRef.IsValid)
        {
            var no = _runner.Spawn(_lobbyStatePrefabRef);
            _lobbyState = no.GetComponent<LobbyState>();
            Debug.Log(_lobbyState != null
                ? "[SelectCharacterMenu] EnsureLobbyState: Spawned LobbyState via prefab ref."
                : "[SelectCharacterMenu] EnsureLobbyState: Spawned object has no LobbyState component.");
            return;
        }

        if (_lobbyStatePrefabObject != null)
        {
            var no = _runner.Spawn(_lobbyStatePrefabObject);
            _lobbyState = no.GetComponent<LobbyState>();
            Debug.Log(_lobbyState != null
                ? "[SelectCharacterMenu] EnsureLobbyState: Spawned LobbyState via prefab object."
                : "[SelectCharacterMenu] EnsureLobbyState: Spawned object has no LobbyState component.");
            return;
        }

        Debug.LogWarning("[SelectCharacterMenu] EnsureLobbyState: Assign 'Lobby State Prefab Object' in the Inspector (drag the LobbyState prefab). Create prefab: empty GameObject + NetworkObject + LobbyState script, save as prefab.");
    }

    void FindButtons()
    {
        // Find by name (direct child only); if no Button on that GO, get from children (e.g. God -> child with Button)
        var godGo = transform.Find(GodButtonName);
        var humanGo = transform.Find(HumanButtonName);
        var readyGo = transform.Find(ReadyButtonName);

        _godButton = godGo != null ? godGo.GetComponentInChildren<Button>(true) : null;
        _humanButton = humanGo != null ? humanGo.GetComponentInChildren<Button>(true) : null;
        _readyButton = readyGo != null ? readyGo.GetComponentInChildren<Button>(true) : null;

        if (godGo == null) Debug.Log("[SelectCharacterMenu] FindButtons: No GameObject named '" + GodButtonName + "' under " + transform.name);
        else if (_godButton == null) Debug.Log("[SelectCharacterMenu] FindButtons: '" + GodButtonName + "' has no Button (self or children).");
        if (humanGo == null) Debug.Log("[SelectCharacterMenu] FindButtons: No GameObject named '" + HumanButtonName + "' under " + transform.name);
        else if (_humanButton == null) Debug.Log("[SelectCharacterMenu] FindButtons: '" + HumanButtonName + "' has no Button (self or children).");
        if (readyGo == null) Debug.Log("[SelectCharacterMenu] FindButtons: No GameObject named '" + ReadyButtonName + "' under " + transform.name);
        else if (_readyButton == null) Debug.Log("[SelectCharacterMenu] FindButtons: '" + ReadyButtonName + "' has no Button (self or children).");
    }

    void SubscribeButtons()
    {
        if (_godButton != null) _godButton.onClick.AddListener(OnGodClicked);
        if (_humanButton != null) _humanButton.onClick.AddListener(OnHumanClicked);
        if (_readyButton != null) _readyButton.onClick.AddListener(OnReadyClicked);
    }

    void UnsubscribeButtons()
    {
        if (_godButton != null) _godButton.onClick.RemoveListener(OnGodClicked);
        if (_humanButton != null) _humanButton.onClick.RemoveListener(OnHumanClicked);
        if (_readyButton != null) _readyButton.onClick.RemoveListener(OnReadyClicked);
    }

    void OnGodClicked()
    {
        TryEnsureRunnerAndLobbyState();
        if (_runner == null || _lobbyState == null)
        {
            Debug.LogWarning($"[SelectCharacterMenu] OnGodClicked: runner={_runner != null}, lobbyState={_lobbyState != null}. Assign LobbyState prefab and ensure Fusion has started.");
            return;
        }

        Debug.Log("[SelectCharacterMenu] OnGodClicked: setting role God, LocalPlayer=" + _runner.LocalPlayer);
        _myChoice = RoleGod;
        _lobbyState.RPC_SelectRole(_runner.LocalPlayer, wantGod: true);
        if (_godButton != null) _godButton.interactable = false;
        onRoleSelected?.Invoke(RoleGod);
    }

    void OnHumanClicked()
    {
        TryEnsureRunnerAndLobbyState();
        if (_runner == null || _lobbyState == null)
        {
            Debug.LogWarning($"[SelectCharacterMenu] OnHumanClicked: runner={_runner != null}, lobbyState={_lobbyState != null}. Assign LobbyState prefab and ensure Fusion has started.");
            return;
        }

        Debug.Log("[SelectCharacterMenu] OnHumanClicked: setting role Human, LocalPlayer=" + _runner.LocalPlayer);
        _myChoice = RoleHuman;
        _lobbyState.RPC_SelectRole(_runner.LocalPlayer, wantGod: false);
        if (_humanButton != null) _humanButton.interactable = false;
        onRoleSelected?.Invoke(RoleHuman);
    }

    void OnReadyClicked()
    {
        if (_lobbyState != null && _lobbyState.BothSelected)
            onBothSelected?.Invoke();
    }
}
