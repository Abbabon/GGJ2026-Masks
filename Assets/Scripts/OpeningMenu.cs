using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class OpeningMenu : MonoBehaviour
{
    [Header("Room start")]
    [Tooltip("Invoked when the button is clicked, with the input field text as argument.")]
    public UnityEvent<string> startedRoom;

    const string InputChildName = "Input";
    const string ButtonChildName = "Button";
    const int MinRoomNameLength = 4;

    TMP_InputField _input;
    Button _button;

    void Start()
    {
        var inputGo = transform.Find(InputChildName);
        var buttonGo = transform.Find(ButtonChildName);

        if (inputGo != null)
            _input = inputGo.GetComponent<TMP_InputField>();
        if (buttonGo != null)
            _button = buttonGo.GetComponent<Button>();

        if (_input == null || _button == null)
        {
            Debug.LogWarning("[OpeningMenu] Could not find Input (TMP_InputField) or Button. Assign them or ensure child names are \"Input\" and \"Button\".");
            return;
        }

        _button.interactable = false;

        _input.onValueChanged.AddListener(OnInputValueChanged);
        _button.onClick.AddListener(OnButtonClicked);

        RefreshButtonState(_input.text);
    }

    void OnInputValueChanged(string value)
    {
        RefreshButtonState(value);
    }

    void RefreshButtonState(string value)
    {
        if (_button != null)
            _button.interactable = value != null && value.Length >= MinRoomNameLength;
    }

    void OnButtonClicked()
    {
        if (_input == null) return;

        string roomName = _input.text != null ? _input.text.Trim() : "";
        if (roomName.Length >= MinRoomNameLength) {
            Debug.Log($"[OpeningMenu] Starting room: {roomName}");
            startedRoom?.Invoke(roomName);
        }
    }

    /// <summary>Clears the room name input and resets button state. Call when showing the menu again after full room.</summary>
    public void ClearInput()
    {
        if (_input != null)
        {
            _input.text = "";
            RefreshButtonState("");
        }
    }
}
