# Weapon System Migration Guide

## Overview

The weapon system has been redesigned to be simpler, more data-driven, and easier to manage. **All obsolete code has been removed** - you must migrate to the new system.

## What Changed

### Old System ? (REMOVED)
- **Weapon.cs** - ScriptableObject for weapon data
- **WeaponController.cs** - Abstract MonoBehaviour base class
- **MeleeWeapon.cs** - Abstract melee weapon implementation
- **Knife.cs** - Concrete knife implementation
- **GoldenKnife.cs** - Concrete golden knife implementation
- **KnifePrefabCreator.cs** - Editor tool for creating knife prefabs

**All of these files have been deleted.**

### New System ? (CURRENT)
- **WeaponData.cs** - Single ScriptableObject containing all weapon properties
- **WeaponSystem.cs** - Single MonoBehaviour that handles all weapon logic

## Benefits

1. **Simpler Architecture** - One data type (`WeaponData`) for all weapons
2. **Easy Inventory Management** - `List<WeaponData>` for owned weapons
3. **Easy Shop Management** - No need for prefabs, just data assets
4. **Clearer Separation** - Data (WeaponData) vs Logic (WeaponSystem)
5. **Less Code** - All weapon logic in one place
6. **No Code for New Weapons** - Data assets can be easily created in Inspector

## Migration Steps

### Step 1: Create WeaponData Assets

For each weapon type (Knife, Golden Knife, etc.):

1. Right-click in Project window
2. Select `Create > Weapons > Weapon Data`
3. Configure the weapon properties:

**Example: Knife**
```
Display Name: Knife
Cost: 100
Weapon Type: Melee
Damage: 20
Cooldown: 0.5
Range: 0.75
Hit Arc Degrees: 90
Raycast Spread: 5
```

**Example: Golden Knife**
```
Display Name: Golden Knife
Cost: 500
Weapon Type: Melee
Damage: 75
Cooldown: 0.25
Range: 1.5
Hit Arc Degrees: 90
Raycast Spread: 7
```

### Step 2: Update Player Prefabs

1. Add `WeaponSystem` component to Player (or weapon socket child object)
2. **Remove** any old weapon component references (Knife, GoldenKnife, WeaponController)
3. Assign starting weapon in Player's `Equipped Weapon` field (drag WeaponData asset)

### Step 3: Update Shop Items

Old shop items referenced `Weapon` ScriptableObject. Update them to use `WeaponData`:

1. Open existing `WeaponShopItem` assets
2. Remove old `Weapon` field reference
3. Assign the appropriate `WeaponData` asset to `WeaponData` field

### Step 4: Remove Old Prefabs

If you have any prefabs using the old components:
- Delete prefabs with `Knife` component
- Delete prefabs with `GoldenKnife` component
- Delete prefabs with `WeaponController` component
- Delete any old `Weapon` ScriptableObject assets

### Step 5: Test

- Verify weapons can be purchased in shop
- Verify weapons can be equipped
- Verify weapon attacks work correctly
- Verify weapon stats are applied correctly
