#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

/// <summary>
/// Editor utility to add PlayerAnimator component to selected GameObjects/prefabs
/// and ensure their AnimatorController has required parameters: Attack, Hit, Death (Triggers) and IsMoving (Bool).
/// Use from menu: Tools/Setup Bot Animators
/// </summary>
public static class BotAnimatorSetup
{
    [MenuItem("Tools/Setup Bot Animators (Selected)")]
    public static void SetupSelected()
    {
        var objs = Selection.gameObjects;
        if (objs == null || objs.Length == 0)
        {
            EditorUtility.DisplayDialog("Setup Bot Animators", "No GameObjects selected. Select scene objects or prefabs in Project view.", "OK");
            return;
        }

        int processed = 0;
        foreach (var go in objs)
        {
            if (PrefabUtility.IsPartOfPrefabAsset(go) || PrefabUtility.IsPartOfPrefabInstance(go))
            {
                ProcessGameObject(go);
                processed++;
            }
            else
            {
                // also process scene objects
                ProcessGameObject(go);
                processed++;
            }
        }

        EditorUtility.DisplayDialog("Setup Bot Animators", $"Processed {processed} objects.", "OK");
    }

    private static void ProcessGameObject(GameObject go)
    {
        if (go == null) return;

        // Add PlayerAnimator component if missing
        var playerAnim = go.GetComponent<PlayerAnimator>();
        if (playerAnim == null)
        {
            playerAnim = Undo.AddComponent<PlayerAnimator>(go);
            Debug.Log($"[BotAnimatorSetup] Added PlayerAnimator to {go.name}");
        }

        // Ensure Animator reference is set
        var animator = go.GetComponent<Animator>();
        if (animator == null)
        {
            // try to find an Animator in children (e.g., weapon visual)
            animator = go.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                Debug.Log($"[BotAnimatorSetup] Found child Animator on {animator.gameObject.name} for {go.name} and will assign it to PlayerAnimator.");
            }
            else
            {
                Debug.LogWarning($"[BotAnimatorSetup] {go.name} has no Animator component. Add one and set a Controller to enable animations.");
                // continue: we still added PlayerAnimator so user can manually assign later
            }
        }

        // Try to get AnimatorController
        var controller = animator.runtimeAnimatorController as AnimatorController;
        if (controller == null)
        {
            Debug.LogWarning($"[BotAnimatorSetup] Animator on {go.name} has no AnimatorController asset or uses an incompatible controller.");
            return;
        }

        EnsureParameter(controller, "Attack", AnimatorControllerParameterType.Trigger);
        EnsureParameter(controller, "Hit", AnimatorControllerParameterType.Trigger);
        EnsureParameter(controller, "Death", AnimatorControllerParameterType.Trigger);
        EnsureParameter(controller, "IsMoving", AnimatorControllerParameterType.Bool);

        // Save asset after modifications
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        
        // If we have a PlayerAnimator and an Animator found, assign it to the serialized field
        if (playerAnim != null && animator != null)
        {
            var so = new SerializedObject(playerAnim);
            var prop = so.FindProperty("animator");
            if (prop != null)
            {
                prop.objectReferenceValue = animator;
                so.ApplyModifiedProperties();
                Debug.Log($"[BotAnimatorSetup] Assigned Animator '{animator.gameObject.name}' to PlayerAnimator on '{go.name}'.");
            }
        }
    }

    private static void EnsureParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
    {
        foreach (var p in controller.parameters)
        {
            if (p.name == name) return; // already present
        }

        controller.AddParameter(name, type);
        Debug.Log($"[BotAnimatorSetup] Added parameter '{name}' to AnimatorController '{controller.name}'");
    }
}
#endif
