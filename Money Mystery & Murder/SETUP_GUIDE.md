# Quick Setup Guide - Phase System

## How to Apply Phase Restrictions to Your Game

### Step 1: Existing GameObjects

Your existing minigame and shop activators will **automatically** have phase restrictions enabled:

#### Minigames (Already configured)
- All existing `MinigameActivator` components now have phase restrictions
- Default: Only available during **Day** phase
- No additional setup needed!

#### Shops (Already configured)
- All existing `ShopActivator` components now have phase restrictions
- Default: Only available during **Evening** phase
- No additional setup needed!

#### Weapons (Already configured)
- All `WeaponSystem` components now have phase restrictions
- Default: Only available during **Night** phase
- No additional setup needed!

### Step 2: Add Visual Indicators (Optional but Recommended)

To help players see when features are available:

1. Select your Minigame GameObject in Unity
2. Click "Add Component"
3. Search for "Phase Indicator"
4. Configure:
   - Required Phase: **Day**
   - Available Text: "Open for Games"
   - Unavailable Text: "Closed"
   - Available Color: Green
   - Unavailable Color: Red

5. Repeat for Shop GameObjects:
   - Required Phase: **Evening**
   - Available Text: "Shop Open"
   - Unavailable Text: "Shop Closed"

### Step 3: Testing

#### Test in Play Mode:
1. Press Play
2. Try to use minigames - should only work during Day
3. Try to open shops - should only work during Evening
4. Try to attack - should only work during Night

#### Force Phase Change for Testing:
Add this to any MonoBehaviour's Update() for quick testing:
```csharp
void Update()
{
    if (Input.GetKeyDown(KeyCode.Alpha1))
        GameManager.Instance.PhaseManager.SetPhase(GamePhase.Day);
    
    if (Input.GetKeyDown(KeyCode.Alpha2))
        GameManager.Instance.PhaseManager.SetPhase(GamePhase.Evening);
    
    if (Input.GetKeyDown(KeyCode.Alpha3))
        GameManager.Instance.PhaseManager.SetPhase(GamePhase.Night);
}
```
Then press 1, 2, or 3 to change phases instantly.

### Step 4: Customization (Optional)

If you want different behavior:

#### Disable Phase Restrictions:
Select the component in Inspector and uncheck `Enforce Phase Restriction`

#### Change Allowed Phase:
Select the component in Inspector and change `Allowed Phase` dropdown

#### Examples:
- Make a special "Night Club" minigame only available at Night
- Make a "Black Market" shop only available at Night
- Make practice weapons available during Day

## Default Configuration Summary

| Feature | Allowed Phase | Can Override? |
|---------|--------------|---------------|
| Minigames | Day | Yes |
| Shops | Evening | Yes |
| Attacking | Night | Yes |

## Phase Schedule

By default (configurable in PhaseManager):
- **Day**: 6:00 AM - 6:00 PM (12 hours)
- **Evening**: 6:00 PM - 9:00 PM (3 hours)
- **Night**: 9:00 PM - 6:00 AM (9 hours)

## Troubleshooting

### "Features are always available"
- Check if `enforcePhaseRestriction` is enabled in Inspector
- Verify GameManager and PhaseManager exist in scene
- Check console for phase changes

### "Features are never available"
- Check current phase: `Debug.Log(GameManager.Instance.CurrentPhase)`
- Verify phase schedule in PhaseManager
- Check if time is progressing: `Debug.Log(GameManager.Instance.CurrentTimeFormatted)`

### "Visual indicators not showing"
- Make sure PhaseIndicator component is attached
- Check if TextMesh is being created (check in Hierarchy)
- Adjust `heightOffset` to move text higher/lower
- Make sure the GameObject is active

## Hot Reload Notice

Since you're debugging, you can use **Hot Reload** to apply these changes without stopping the game!

Just save your changes and the phase system will start working immediately.
