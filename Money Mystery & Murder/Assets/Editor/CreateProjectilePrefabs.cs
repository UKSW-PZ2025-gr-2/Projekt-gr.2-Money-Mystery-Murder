#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility that creates simple projectile prefabs for Rifle and Grenade
/// and assigns them to WeaponData assets if present in Assets/Weapons.
/// Menu: Tools/Create Projectile Prefabs
/// </summary>
public static class CreateProjectilePrefabs
{
    [MenuItem("Tools/Create Projectile Prefabs (Rifle & Grenade)")]
    [MenuItem("Tools/Create Projectile Prefabs")]
    public static void Create()
    {
        string folder = "Assets/Prefabs/Projectiles";
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/Prefabs", "Projectiles");

        // Rifle bullet prefab
        var bulletGO = new GameObject("RifleBullet");
        var rb = bulletGO.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        var col = bulletGO.AddComponent<CircleCollider2D>();
        col.isTrigger = false;
        var proj = bulletGO.AddComponent<Projectile>();
        proj.damage = 20;
        proj.speed = 40f;
        proj.lifetime = 3f;

        string bulletPath = folder + "/RifleBullet.prefab";
        var bulletPrefab = PrefabUtility.SaveAsPrefabAsset(bulletGO, bulletPath);
        Object.DestroyImmediate(bulletGO);

        // Grenade prefab
        var grenadeGO = new GameObject("GrenadeProjectile");
        var grb = grenadeGO.AddComponent<Rigidbody2D>();
        grb.gravityScale = 0.5f;
        var gcol = grenadeGO.AddComponent<CircleCollider2D>();
        gcol.isTrigger = false;
        var gproj = grenadeGO.AddComponent<GrenadeProjectile>();
        gproj.damage = 70;
        gproj.radius = 2.5f;
        gproj.fuse = 1.5f;

        string grenadePath = folder + "/GrenadeProjectile.prefab";
        var grenadePrefab = PrefabUtility.SaveAsPrefabAsset(grenadeGO, grenadePath);
        Object.DestroyImmediate(grenadeGO);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[CreateProjectilePrefabs] Created prefabs: {bulletPath}, {grenadePath}");

        // Try to assign prefabs to WeaponData assets if they exist
        var rifle = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Weapons/Rifle.asset");
        if (rifle != null && bulletPrefab != null)
        {
            rifle.projectilePrefab = bulletPrefab as GameObject;
            EditorUtility.SetDirty(rifle);
            Debug.Log("[CreateProjectilePrefabs] Assigned Rifle projectile prefab to Rifle.asset");
        }

        var grenade = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Weapons/Grenade.asset");
        if (grenade != null && grenadePrefab != null)
        {
            grenade.projectilePrefab = grenadePrefab as GameObject;
            EditorUtility.SetDirty(grenade);
            Debug.Log("[CreateProjectilePrefabs] Assigned Grenade projectile prefab to Grenade.asset");
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Create Projectile Prefabs", "Created projectile prefabs and assigned to WeaponData assets if found.", "OK");
    }
}
#endif
