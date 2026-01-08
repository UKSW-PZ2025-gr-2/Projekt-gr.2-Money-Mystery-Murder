#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor helper to create sample WeaponData assets: Katana, Rifle, Grenade.
/// Use Tools -> Create Sample Weapons
/// </summary>
public static class CreateSampleWeapons
{
    [MenuItem("Tools/Create Sample Weapons (Katana, Rifle, Grenade)")]
    public static void Create()
    {
        string folder = "Assets/Weapons";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets", "Weapons");
        }

        // Katana - Melee
        var katana = ScriptableObject.CreateInstance<WeaponData>();
        katana.displayName = "Katana";
        katana.weaponType = WeaponType.Melee;
        katana.damage = 35;
        katana.cooldown = 0.8f;
        katana.range = 1.4f;
        katana.hitArcDegrees = 120f;
        katana.raycastSpread = 5;
        katana.description = "A sharp blade for close combat.";
        AssetDatabase.CreateAsset(katana, "Assets/Weapons/Katana.asset");

        // Rifle - Ranged hitscan
        var rifle = ScriptableObject.CreateInstance<WeaponData>();
        rifle.displayName = "Rifle";
        rifle.weaponType = WeaponType.Ranged;
        rifle.damage = 20;
        rifle.cooldown = 0.15f;
        rifle.range = 18f;
        rifle.usesAmmo = true;
        rifle.maxAmmo = 30;
        rifle.projectileSpeed = 0f; // hitscan when no projectile prefab
        rifle.description = "A semi-automatic rifle for medium-range combat.";
        AssetDatabase.CreateAsset(rifle, "Assets/Weapons/Rifle.asset");

        // Grenade - Ranged (player will have to supply projectile prefab for arc behavior)
        var grenade = ScriptableObject.CreateInstance<WeaponData>();
        grenade.displayName = "Grenade";
        grenade.weaponType = WeaponType.Ranged;
        grenade.damage = 70;
        grenade.cooldown = 2.0f;
        grenade.range = 10f;
        grenade.usesAmmo = false;
        grenade.description = "Explosive device; provide a projectile prefab for arc/area effect.";
        AssetDatabase.CreateAsset(grenade, "Assets/Weapons/Grenade.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Create Sample Weapons", "Created Katana, Rifle and Grenade assets in Assets/Weapons.", "OK");
    }
}
#endif
