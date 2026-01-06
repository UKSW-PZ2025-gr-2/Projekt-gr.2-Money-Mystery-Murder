#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager))]
public class GameManagerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(8);
        GUILayout.Label("Utility Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Create Sample Weapons"))
        {
            // call the menu item (works even if menu is hidden for some reason)
            EditorApplication.ExecuteMenuItem("Tools/Create Sample Weapons (Katana, Rifle, Grenade)");
        }

        if (GUILayout.Button("Create Projectile Prefabs"))
        {
            EditorApplication.ExecuteMenuItem("Tools/Create Projectile Prefabs");
        }

        if (GUILayout.Button("Setup Selected Animators"))
        {
            EditorApplication.ExecuteMenuItem("Tools/Setup Bot Animators (Selected)");
        }

        if (GUILayout.Button("Select GameManager in Hierarchy"))
        {
            var gm = target as GameManager;
            if (gm != null) Selection.activeObject = gm.gameObject;
        }
    }
}
#endif
