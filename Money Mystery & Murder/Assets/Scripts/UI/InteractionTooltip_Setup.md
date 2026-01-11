# Interaction Tooltip Setup Guide

## Overview
The `InteractionTooltip` component displays a prompt to players when they're near interactable objects like minigames. It automatically shows the correct key binding from the player's settings.

## Setup Instructions

### 1. Create the Tooltip UI

1. In your Unity scene, create a new Canvas if one doesn't exist:
   - Right-click in Hierarchy ? UI ? Canvas
   - Set the Canvas Render Mode to "Screen Space - Overlay"

2. Create the tooltip UI element:
   - Right-click on the Canvas ? UI ? Panel
   - Rename it to "InteractionTooltip"
   - Add the `InteractionTooltip` component to this GameObject

3. Create the text element:
   - Right-click on InteractionTooltip ? UI ? Text - TextMeshPro
   - Rename it to "TooltipText"
   - Position it centered in the panel
   - Set the text alignment to Center

4. Configure the InteractionTooltip component:
   - Drag the TooltipText object to the "Tooltip Text" field
   - Set "Base Message" to "to activate minigame" (or your preferred text)
   - Check "Follow World Position" to make it appear above objects
   - Adjust "Screen Offset" as needed (default: 0, 50, 0)

### 2. Link to MinigameActivator

For each minigame activator in your scene:

1. Select the GameObject with the `MinigameActivator` component
2. In the Inspector, find the "UI" section
3. Drag your InteractionTooltip GameObject to the "Interaction Tooltip" field
4. Optionally customize:
   - "Tooltip Message": Change the text that appears
   - "Tooltip Height Offset": Adjust how high above the object the tooltip appears (default: 1.5)

### 3. Test the Setup

1. Enter Play Mode
2. Move your player character near the minigame activator
3. The tooltip should appear above the minigame showing "Press [E] to activate minigame"
   - The key shown will match the player's current key bindings
   - The tooltip will disappear when:
     - The player moves out of range
     - The minigame starts
     - The game is in a phase where minigames are restricted

## Features

- **Automatic Key Display**: Shows the correct key from `KeyBindings`
- **World Space Positioning**: Follows the minigame object in 3D space
- **Phase Awareness**: Only shows when minigames are available
- **Customizable Messages**: Each activator can have its own message
- **Performance Optimized**: Only updates when needed

## Troubleshooting

**Tooltip doesn't appear:**
- Ensure the InteractionTooltip GameObject is active in the hierarchy
- Check that the Canvas is in "Screen Space - Overlay" mode
- Verify the tooltip is linked in the MinigameActivator's Inspector

**Wrong key is displayed:**
- Check the KeyBindings settings in your Settings Manager
- Ensure KeyBindings.Instance is properly initialized

**Tooltip positioned incorrectly:**
- Adjust the "Screen Offset" in the InteractionTooltip component
- Adjust the "Tooltip Height Offset" in the MinigameActivator component
- Ensure your camera is tagged as "MainCamera"

## Technical Details

### InteractionTooltip Component Properties

- **Tooltip Text** (TextMeshProUGUI): The text component to display the message
- **Base Message** (string): The message to display after the key prompt
- **Follow World Position** (bool): Whether to follow a 3D object
- **Screen Offset** (Vector3): Pixel offset from the world position

### MinigameActivator Integration

The MinigameActivator now includes:
- **Interaction Tooltip** (InteractionTooltip): Reference to the tooltip UI
- **Tooltip Message** (string): Custom message for this activator
- **Tooltip Height Offset** (float): Height above the object

The tooltip automatically:
- Shows when a player is in range and the minigame is available
- Hides when the player leaves range or starts the minigame
- Updates its position to stay above the minigame object
