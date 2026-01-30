using UnityEditor;
using UnityEngine;
using Fusion;

[CustomEditor(typeof(SelectCharacterMenu))]
public class SelectCharacterMenuEditor : UnityEditor.Editor
{
    const string LobbyStatePrefabObjectProp = "_lobbyStatePrefabObject";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var menu = (SelectCharacterMenu)target;
        var so = serializedObject;
        var prop = so.FindProperty(LobbyStatePrefabObjectProp);
        if (prop == null) return;

        if (prop.objectReferenceValue != null)
            return;

        EditorGUILayout.Space(4);
        EditorGUILayout.HelpBox(
            "Lobby State Prefab Object is not set. Click the button below to find and assign the LobbyState prefab from the project.",
            MessageType.Warning);

        if (GUILayout.Button("Assign LobbyState prefab from project"))
        {
            var prefab = FindLobbyStatePrefab();
            if (prefab != null)
            {
                prop.objectReferenceValue = prefab;
                so.ApplyModifiedProperties();
                Debug.Log("[SelectCharacterMenu] Assigned LobbyState prefab: " + AssetDatabase.GetAssetPath(prefab));
            }
            else
            {
                Debug.LogWarning("[SelectCharacterMenu] No prefab with NetworkObject + LobbyState found. Create one: empty GameObject + NetworkObject + LobbyState script, save as prefab (e.g. Assets/Prefab/LobbyState.prefab).");
            }
        }
    }

    static NetworkObject FindLobbyStatePrefab()
    {
        var guids = AssetDatabase.FindAssets("LobbyState t:Prefab");
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) continue;
            var no = go.GetComponent<NetworkObject>();
            var lobby = go.GetComponent<Network.LobbyState>();
            if (no != null && lobby != null)
                return no;
        }
        return null;
    }
}
