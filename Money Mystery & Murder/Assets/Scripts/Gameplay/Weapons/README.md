# Weapon System Documentation

## Overview

The weapon system is a unified, data-driven architecture for managing all weapon types in the game. It uses a single `WeaponData` ScriptableObject to define weapon properties and a single `WeaponSystem` component to handle all weapon behavior.

## Architecture

```
WeaponData (ScriptableObject)
    ?
WeaponSystem (MonoBehaviour)
    ?
Player (MonoBehaviour)
```

### Key Components

#### 1. WeaponData (ScriptableObject)
**Location**: `Assets/Scripts/Gameplay/Weapons/WeaponData.cs`

A pure data container that defines all properties of a weapon. This is what you manage in inventory and shop.

**Properties**:
- **Display**: Name, icon, description
- **Economy**: Cost
- **Combat Stats**: Type, damage, cooldown, range
- **Melee Settings**: Hit arc, raycast spread
- **Ranged Settings**: Ammo, projectile speed, projectile prefab
- **Visual & Audio**: Weapon prefab, hit effects, animations, sounds

#### 2. WeaponSystem (MonoBehaviour)
**Location**: `Assets/Scripts/Gameplay/Weapons/WeaponSystem.cs`

Handles all weapon logic including attack execution, cooldown management, and hit detection. Attach this to Player or a weapon socket child object.

**Key Methods**:
- `Initialize(Player)` - Setup with owning player
- `EquipWeapon(WeaponData)` - Change current weapon
- `Attack()` - Execute weapon attack
- `CanAttack()` - Check if attack is available
- `GetCooldownRemaining()` - Get time until next attack

## Usage

### Creating a New Weapon

1. **Create WeaponData Asset**
   ```
   Right-click in Project ? Create ? Weapons ? Weapon Data
   ```

2. **Configure Properties**
   - Set display name, icon, description
   - Set cost for shop
   - Choose weapon type (Melee/Ranged)
   - Configure combat stats
   - Add visual/audio assets (optional)

3. **Example: Simple Knife**
   ```
   Display Name: "Basic Knife"
   Cost: 100
   Weapon Type: Melee
   Damage: 20
   Cooldown: 0.5
   Range: 0.75
   Hit Arc Degrees: 90
   Raycast Spread: 5
   ```

### Setting Up Player

```csharp
// In Player prefab, add WeaponSystem component
// WeaponSystem will be automatically found and initialized

// In code:
public class Player : MonoBehaviour
{
    [SerializeField] private WeaponData equippedWeapon;
    [SerializeField] private List<WeaponData> ownedWeapons;
    [SerializeField] private WeaponSystem weaponSystem;
    
    private void Start()
    {
        weaponSystem.Initialize(this);
        if (equippedWeapon != null)
        {
            weaponSystem.EquipWeapon(equippedWeapon);
        }
    }
}
```

### Managing Inventory

```csharp
// Add weapon to inventory
public void AcquireWeapon(WeaponData weapon)
{
    if (weapon == null || ownedWeapons.Contains(weapon)) return;
    
    ownedWeapons.Add(weapon);
    
    if (equippedWeapon == null)
    {
        EquipWeapon(weapon);
    }
}

// Equip a weapon from inventory
public void EquipWeapon(WeaponData weapon)
{
    if (weapon == null || !ownedWeapons.Contains(weapon)) return;
    
    equippedWeapon = weapon;
    weaponSystem.EquipWeapon(weapon);
}

// List all owned weapons
foreach (WeaponData weapon in ownedWeapons)
{
    Debug.Log($"{weapon.displayName}: {weapon.damage} damage");
}
```

### Shop Integration

```csharp
// In WeaponShopItem (ScriptableObject)
public class WeaponShopItem : ShopItem
{
    public WeaponData WeaponData;
}

// In Shop system
public bool BuyWeapon(Player player, WeaponShopItem item)
{
    if (player.SpendBalance(item.Price))
    {
        player.AcquireWeapon(item.WeaponData);
        return true;
    }
    return false;
}
```

### Attacking

```csharp
// In Player class
public void PerformAttack()
{
    if (weaponSystem == null) return;
    weaponSystem.Attack();
}

// Bind to input
private void HandleAttackInput()
{
    if (Mouse.current.leftButton.wasPressedThisFrame)
    {
        PerformAttack();
    }
}
```

## Weapon Types

### Melee Weapons

Uses Physics2D raycasts in a spread pattern to detect hits.

**Key Properties**:
- `hitArcDegrees` - Width of attack cone
- `raycastSpread` - Number of rays to cast
- `range` - Attack reach

**How it works**:
1. Generates multiple ray directions within the arc
2. Casts Physics2D raycasts from attack origin
3. Detects Player components on hit colliders
4. Applies damage to unique targets

### Ranged Weapons

Supports both projectile and hitscan modes.

**Key Properties**:
- `usesAmmo` - Whether weapon has limited ammo
- `maxAmmo` - Maximum ammunition
- `projectileSpeed` - Speed of projectiles
- `projectilePrefab` - Projectile object (optional)

**How it works**:
- **With projectilePrefab**: Spawns projectile GameObject with velocity
- **Without projectilePrefab**: Instant hitscan raycast

## Advanced Features

### Custom Attack Origins

```csharp
// Set a custom attack origin transform (e.g., weapon tip)
[SerializeField] private Transform attackOrigin;
weaponSystem.attackOrigin = attackOrigin;
```

### Layer Masking

```csharp
// Only hit specific layers
[SerializeField] private LayerMask hitLayers;
// Configure in WeaponSystem inspector
```

### Ammo Management

```csharp
// Check current ammo
int ammo = weaponSystem.CurrentAmmo;

// Reload (future feature)
weaponSystem.Reload();
```

### Cooldown Checks

```csharp
// Check if weapon can attack
if (weaponSystem.CanAttack())
{
    weaponSystem.Attack();
}

// Get remaining cooldown for UI
float remaining = weaponSystem.GetCooldownRemaining();
cooldownBar.fillAmount = 1f - (remaining / weapon.cooldown);
```

## Best Practices

1. **Keep WeaponData Pure** - Only data, no logic
2. **Use WeaponSystem for Behavior** - All attack logic in one place
3. **Manage Weapons as Data** - Inventory and shop use `List<WeaponData>`
4. **Create Variants with Assets** - Don't create new classes, create new WeaponData assets
5. **Test in Inspector** - All properties visible and editable in Unity

## Example Weapons

### Standard Knife
```
Display Name: Knife
Cost: 100
Type: Melee
Damage: 20
Cooldown: 0.5s
Range: 0.75
Arc: 90°
Spread: 5 rays
```

### Golden Knife
```
Display Name: Golden Knife  
Cost: 500
Type: Melee
Damage: 75
Cooldown: 0.25s
Range: 1.5
Arc: 90°
Spread: 7 rays
```

### Pistol (Future)
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

## FAQ

**Q: How do I create a new weapon type?**
A: Create a new `WeaponData` asset and configure its properties. No coding needed.

**Q: How do I add weapons to the shop?**
A: Create a `WeaponShopItem` asset and assign the `WeaponData` to it.

**Q: Can I have different attack patterns?**
A: Yes, configure `WeaponType`, `hitArcDegrees`, and `raycastSpread` to customize behavior.

**Q: How do I add weapon visuals?**
A: Assign `weaponPrefab` in WeaponData. It will be instantiated when equipped.

**Q: Can weapons have different animations?**
A: Yes, assign `attackAnimation` in WeaponData or use Animator on the weapon prefab.

**Q: What about ranged weapons?**
A: Set `weaponType` to `Ranged` and configure `projectilePrefab` or use hitscan.
