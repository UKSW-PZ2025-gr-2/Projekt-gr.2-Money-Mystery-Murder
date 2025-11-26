using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Game/Weapon", order = 10)]
public class Weapon : ScriptableObject
{
    [Header("General")] public string displayName = "Weapon";
    public Sprite icon;

    [Header("Combat Stats")] public int damage = 10;
    public float fireRate = 1f;
    public float range = 10f;

    [Header("Ammo")] public bool usesAmmo = false;
    public int maxAmmo = 0;

    [TextArea] public string description;
}
