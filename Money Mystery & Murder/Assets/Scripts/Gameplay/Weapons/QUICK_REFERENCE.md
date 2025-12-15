# Weapon System - Quick Reference

## Creating a New Weapon

1. **Create Asset**
   ```
   Right-click ? Create ? Weapons ? Weapon Data
   ```

2. **Configure in Inspector**
   - Display Name (e.g., "Knife")
   - Icon (weapon sprite)
   - Cost (shop price)
   - Weapon Type (Melee/Ranged)
   - Damage, Cooldown, Range
   - Melee settings: Hit Arc, Raycast Spread
   - Visuals: Prefab, Effects, Animation, Sound

3. **Done!** No code needed.

## Using in Code

### Player - Acquire Weapon
```csharp
public WeaponData knifeData;
player.AcquireWeapon(knifeData);
```

### Player - Equip Weapon
```csharp
player.EquipWeapon(weaponData);
```

### Player - Attack
```csharp
player.PerformAttack();
```

### Player - Check Equipped Weapon
```csharp
WeaponData equipped = player.EquippedWeapon;
if (equipped != null)
{
    Debug.Log($"Equipped: {equipped.displayName}, Damage: {equipped.damage}");
}
```

### Player - List Owned Weapons
```csharp
foreach (WeaponData weapon in player.OwnedWeapons)
{
    Debug.Log($"Owned: {weapon.displayName}");
}

// First weapon in list is automatically equipped at start
if (player.OwnedWeapons.Count > 0)
{
    Debug.Log($"First weapon: {player.OwnedWeapons[0].displayName}");
}
```

### WeaponSystem - Check Attack Status
```csharp
WeaponSystem weaponSystem = GetComponent<WeaponSystem>();

if (weaponSystem.CanAttack())
{
    weaponSystem.Attack();
}

float cooldown = weaponSystem.GetCooldownRemaining();
int ammo = weaponSystem.CurrentAmmo;
```

## Shop Integration

### Create Shop Item
1. Create WeaponShopItem asset
2. Set `WeaponData` field
3. Set price, icon, description
4. Add to shop inventory list

### Shop Code
```csharp
public class WeaponShopItem : ShopItem
{
    public WeaponData WeaponData;
}

// Purchase
if (player.SpendBalance(item.Price))
{
    player.AcquireWeapon(item.WeaponData);
    return true;
}
```

## Example Weapon Stats

### Basic Knife
```
Display Name: Knife
Cost: 100
Type: Melee
Damage: 20
Cooldown: 0.5s
Range: 0.75
Hit Arc: 90°
Raycast Spread: 5
```

### Golden Knife
```
Display Name: Golden Knife
Cost: 500
Type: Melee
Damage: 75
Cooldown: 0.25s
Range: 1.5
Hit Arc: 90°
Raycast Spread: 7
```

### Pistol (Ranged Example)
```
Display Name: Pistol
Cost: 200
Type: Ranged
Damage: 15
Cooldown: 0.3s
Range: 15
Uses Ammo: Yes
Max Ammo: 12
```

## Player Setup

1. Add `WeaponSystem` component to Player (or weapon socket)
2. In Player Inspector:
   - Add weapons to `Owned Weapons` list
   - **First weapon in the list will be auto-equipped at start**
3. WeaponSystem is auto-initialized in Player.Start()

## Common Patterns

### Award Weapon as Reward
```csharp
public WeaponData rewardWeapon;

void GiveReward(Player player)
{
    player.AcquireWeapon(rewardWeapon);
    // If this is first weapon, it's automatically equipped
    // Otherwise, you can manually equip it:
    player.EquipWeapon(rewardWeapon);
}
```

### Check if Player Owns Weapon
```csharp
if (player.OwnedWeapons.Contains(weaponData))
{
    Debug.Log("Player already owns this weapon!");
}
```

### Switch Weapons
```csharp
// Assuming player owns both
player.EquipWeapon(knifeData);
// Later...
player.EquipWeapon(pistolData);
```

### Weapon UI Display
```csharp
WeaponData weapon = player.EquippedWeapon;
if (weapon != null)
{
    weaponNameText.text = weapon.displayName;
    weaponIcon.sprite = weapon.icon;
    damageText.text = $"Damage: {weapon.damage}";
    rangeText.text = $"Range: {weapon.range:F1}";
}
```

### Cooldown Bar
```csharp
WeaponSystem weaponSystem = player.GetComponentInChildren<WeaponSystem>();
float remaining = weaponSystem.GetCooldownRemaining();
float cooldown = weaponSystem.CurrentWeapon?.cooldown ?? 1f;
cooldownBar.fillAmount = 1f - (remaining / cooldown);
```

## File Structure

```
Assets/Scripts/Gameplay/Weapons/
??? WeaponData.cs           ? Data definition
??? WeaponSystem.cs         ? Behavior logic
??? README.md               ? Full documentation
??? MIGRATION_GUIDE.md      ? Migration instructions
??? REDESIGN_SUMMARY.md     ? Change summary
??? QUICK_REFERENCE.md      ? This file

? All obsolete files have been removed!
```

## See Also

- **README.md** - Complete documentation
- **MIGRATION_GUIDE.md** - Detailed migration steps
- **REDESIGN_SUMMARY.md** - What changed and why
