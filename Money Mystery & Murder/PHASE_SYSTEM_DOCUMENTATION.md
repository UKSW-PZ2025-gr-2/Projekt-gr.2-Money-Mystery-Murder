# Phase-Based Gameplay System - Implementation Summary

## Overview
A complete day/night cycle system that restricts minigames, shops, and attacking based on the current game phase.

## Phase Restrictions

### Day Phase
- **Minigames**: ? Available
- **Shops**: ? Closed
- **Attacking**: ? Disabled

### Evening Phase
- **Minigames**: ? Closed
- **Shops**: ? Available
- **Attacking**: ? Disabled

### Night Phase
- **Minigames**: ? Closed
- **Shops**: ? Closed
- **Attacking**: ? Available

## Modified Files

### 1. MinigameActivator.cs
**Changes:**
- Added phase restriction system (default: Day phase only)
- Added `enforcePhaseRestriction` toggle (true by default)
- Added `allowedPhase` setting (Day phase by default)
- Shows debug message when player tries to use minigame during wrong phase
- Added `IsAvailableInCurrentPhase` property for external checks

**Usage:**
```csharp
// In Unity Inspector:
// - enforcePhaseRestriction: true (to enable phase checking)
// - allowedPhase: Day
```

### 2. ShopActivator.cs
**Changes:**
- Added phase restriction system (default: Evening phase only)
- Added `enforcePhaseRestriction` toggle (true by default)
- Added `allowedPhase` setting (Evening phase by default)
- Shows debug message when player tries to open shop during wrong phase
- Added `IsAvailableInCurrentPhase` property for external checks

**Usage:**
```csharp
// In Unity Inspector:
// - enforcePhaseRestriction: true (to enable phase checking)
// - allowedPhase: Evening
```

### 3. WeaponSystem.cs
**Changes:**
- Added phase restriction system (default: Night phase only)
- Added `enforcePhaseRestriction` toggle (true by default)
- Added `allowedPhase` setting (Night phase by default)
- Shows debug warning when player tries to attack during wrong phase (rate-limited to every 2 seconds)
- Integrated phase check into `CanAttack()` method
- Attack attempts during wrong phase show warning message

**Usage:**
```csharp
// In Unity Inspector:
// - enforcePhaseRestriction: true (to enable phase checking)
// - allowedPhase: Night
```

## New Files

### 4. PhaseIndicator.cs (New Component)
**Purpose:** Visual indicator to show players when a feature is available.

**Features:**
- Shows "Available" (green) or "Closed" (red) text above objects
- Automatically updates based on current phase
- Configurable text, colors, and positioning
- Can be attached to any GameObject to indicate availability

**Usage:**
```csharp
// Attach to Minigame objects:
// - requiredPhase: Day
// - availableText: "Open for Games"
// - unavailableText: "Closed"

// Attach to Shop objects:
// - requiredPhase: Evening
// - availableText: "Shop Open"
// - unavailableText: "Shop Closed"
```

## Configuration Options

All phase restrictions can be **disabled individually** by setting `enforcePhaseRestriction` to `false` in the Unity Inspector.

### For Testing/Debugging:
You can disable phase restrictions on specific systems:
- **MinigameActivator**: Set `enforcePhaseRestriction = false` to allow minigames at any time
- **ShopActivator**: Set `enforcePhaseRestriction = false` to allow shops at any time
- **WeaponSystem**: Set `enforcePhaseRestriction = false` to allow attacking at any time

### For Custom Phases:
Change the `allowedPhase` field to customize when features are available:
- Example: Make a special minigame only available at Night
- Example: Make a weapon shop only available during Day

## Integration with Existing Systems

### GameManager
- Already has `CurrentPhase` property that returns the active phase
- PhaseManager handles automatic phase transitions based on time

### PhaseManager
- Existing system that manages Day/Evening/Night phases
- Default schedule:
  - Day: 6:00 - 18:00
  - Evening: 18:00 - 21:00
  - Night: 21:00 - 6:00

## Player Feedback

### Debug Messages:
When players try to use restricted features, they'll see console messages:
- **Minigames**: "[MinigameActivator] Minigames are only available during Day phase!"
- **Shops**: "[ShopActivator] Shops are only available during Evening phase!"
- **Attacking**: "[WeaponSystem] Attacking is only available during Night phase!"

### Visual Indicators:
Use the `PhaseIndicator` component to show availability status in-world:
- Green "Available" text when feature is usable
- Red "Closed" text when feature is restricted

## Testing

### Manual Phase Testing:
You can test phases by calling these methods in GameManager:
```csharp
GameManager.Instance.PhaseManager.SetPhase(GamePhase.Day);
GameManager.Instance.PhaseManager.SetPhase(GamePhase.Evening);
GameManager.Instance.PhaseManager.SetPhase(GamePhase.Night);
```

### Quick Time Change:
```csharp
GameManager.Instance.SetTime(6, 0);   // Day
GameManager.Instance.SetTime(18, 0);  // Evening
GameManager.Instance.SetTime(21, 0);  // Night
```

## Notes

1. **Bot Exclusion**: Bots are excluded from minigame and shop interactions (player-only features)
2. **Backward Compatibility**: All phase restrictions can be disabled per-component
3. **Performance**: Phase checks are lightweight and use singleton pattern
4. **Extensibility**: Easy to add more phase-restricted features using the same pattern

## Future Enhancements

Potential additions:
1. UI notification system instead of debug logs
2. Sound effects when trying to use restricted features
3. Countdown timers showing when features become available
4. Different restriction sets for different player roles
5. Phase-specific minigames or weapons
