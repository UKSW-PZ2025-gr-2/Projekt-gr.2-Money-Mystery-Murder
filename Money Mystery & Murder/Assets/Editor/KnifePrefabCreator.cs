using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

// Editor utility to create a sample Knife prefab with a simple stab animation and a hit effect prefab.
// Usage: in Unity Editor go to Tools -> Create Knife Prefab
public static class KnifePrefabCreator
{
    [MenuItem("Tools/Create Knife Prefab (example)")]
    public static void CreateKnifePrefab()
    {
        // Ensure prefabs folder exists
        var prefabsFolder = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabsFolder))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        // 1) Create a simple hit effect (ParticleSystem) prefab
        string hitEffectPath = prefabsFolder + "/KnifeHitEffect.prefab";
        GameObject hitEffectGO = new GameObject("KnifeHitEffect");
        var ps = hitEffectGO.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.5f;
        main.startLifetime = 0.3f;
        main.startSpeed = 1f;
        main.startSize = 0.2f;
        var em = ps.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 8) });
        var renderer = hitEffectGO.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        PrefabUtility.SaveAsPrefabAsset(hitEffectGO, hitEffectPath);
        Object.DestroyImmediate(hitEffectGO);

        // 2) Create a simple animation clip that moves the knife forward and back (stab)
        string clipPath = prefabsFolder + "/Knife_Stab.anim";
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 60;

        // Create a curve for localPosition.z to simulate a small forward stab
        var curve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.08f, 0.25f),
            new Keyframe(0.18f, 0f)
        );

        // Bind to the root transform localPosition.z
        var binding = EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.z");
        AnimationUtility.SetEditorCurve(clip, binding, curve);

        AssetDatabase.CreateAsset(clip, clipPath);

        // 3) Create an AnimatorController with a Stab trigger that plays the clip
        string controllerPath = prefabsFolder + "/Knife_Animator.controller";
        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddParameter("Stab", AnimatorControllerParameterType.Trigger);
        var sm = controller.layers[0].stateMachine;
        var stabState = sm.AddState("Stab");
        stabState.motion = clip;
        // Transition from AnyState -> Stab on trigger
        var anyToStab = sm.AddAnyStateTransition(stabState);
        anyToStab.AddCondition(AnimatorConditionMode.If, 0, "Stab");
        anyToStab.hasExitTime = false;

        AssetDatabase.SaveAssets();

        // 4) Create Knife prefab with Knife component, Animator and assign hitEffectPrefab
        string knifePrefabPath = prefabsFolder + "/Knife.prefab";
        GameObject knifeGO = new GameObject("Knife");
        // Position reset
        knifeGO.transform.localPosition = Vector3.zero;

        // Add Knife component (script must exist in project)
        var knifeComp = knifeGO.AddComponent<Knife>();

        // Add Animator and assign controller
        var animator = knifeGO.AddComponent<Animator>();
        animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);

        // Add a simple visible blade so the knife is visible in scene/prefab
        GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
        blade.name = "Blade";
        blade.transform.SetParent(knifeGO.transform, false);
        blade.transform.localPosition = new Vector3(0f, 0f, 0.3f);
        blade.transform.localScale = new Vector3(0.05f, 0.05f, 0.6f);
        // Remove collider from the visual blade
        var bladeCollider = blade.GetComponent<Collider>();
        if (bladeCollider != null) Object.DestroyImmediate(bladeCollider);
        // Give it a simple metallic-ish material
        var bladeRenderer = blade.GetComponent<MeshRenderer>();
        if (bladeRenderer != null)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.8f, 0.8f, 0.85f);
            bladeRenderer.sharedMaterial = mat;
        }

        // Load and assign hit effect prefab to the private serialized field
        var effectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(hitEffectPath);
        var so = new SerializedObject(knifeComp);
        var prop = so.FindProperty("hitEffectPrefab");
        if (prop != null)
        {
            prop.objectReferenceValue = effectPrefab;
            so.ApplyModifiedProperties();
        }

        PrefabUtility.SaveAsPrefabAsset(knifeGO, knifePrefabPath);
        Object.DestroyImmediate(knifeGO);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Knife Prefab Created", "Created Knife prefab, hit effect prefab and animator in Assets/Prefabs.", "OK");
    }
}
