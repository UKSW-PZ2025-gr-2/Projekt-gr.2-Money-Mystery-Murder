using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// UI panel for rebinding keyboard controls in the Settings Scene.
/// Allows players to customize all game controls and saves them via KeyBindings.
/// </summary>
public class KeyRebindPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform rebindButtonContainer;
    [SerializeField] private GameObject listeningPanel;
    [SerializeField] private TMP_Text listeningText;
    [SerializeField] private Button resetButton;
    
    private string currentRebindAction = null;
    private bool isListening = false;
    private bool buttonsCreated = false;
    
    void Start()
    {
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetToDefaults);
    }
    
    void OnEnable()
    {
        Debug.Log("[KeyRebindPanel] OnEnable called");
        
        if (listeningPanel != null)
            listeningPanel.SetActive(false);
        
        // Create buttons only once
        if (!buttonsCreated)
        {
            Debug.Log("[KeyRebindPanel] Creating rebind buttons...");
            if (rebindButtonContainer == null)
            {
                Debug.LogError("[KeyRebindPanel] rebindButtonContainer is NULL! Assign it in Inspector.");
                return;
            }
            CreateRebindButtons();
            buttonsCreated = true;
            Debug.Log("[KeyRebindPanel] Buttons created successfully");
        }
        else
        {
            Debug.Log("[KeyRebindPanel] Refreshing button labels...");
            // Refresh button labels if buttons already exist
            RefreshButtonLabels();
        }
        
        // Subscribe to key changes
        if (KeyBindings.Instance != null)
            KeyBindings.Instance.OnKeysChanged += RefreshButtonLabels;
    }
    
    void OnDisable()
    {
        if (KeyBindings.Instance != null)
            KeyBindings.Instance.OnKeysChanged -= RefreshButtonLabels;
    }
    
    /// <summary>
    /// Creates all rebind buttons for each action.
    /// </summary>
    private void CreateRebindButtons()
    {
        if (rebindButtonContainer == null) return;
        
        Debug.Log($"[KeyRebindPanel] Container has {rebindButtonContainer.childCount} children before clearing");
        
        // Clear existing buttons
        foreach (Transform child in rebindButtonContainer)
        {
            Destroy(child.gameObject);
        }
        
        Debug.Log("[KeyRebindPanel] Starting button creation...");
        
        // Movement
        CreateRebindButton("Ruch w gore", "MoveUp");
        CreateRebindButton("Ruch w dol", "MoveDown");
        CreateRebindButton("Ruch w lewo", "MoveLeft");
        CreateRebindButton("Ruch w prawo", "MoveRight");
        
        // Combat
        CreateRebindButton("Zmiana broni", "SwitchWeapon");
        
        // Inventory
        CreateRebindButton("Uzyj przedmiot", "UseItem");
        
        // Interaction
        CreateRebindButton("Interakcja", "Interact");
        
        // Hotbar
        CreateRebindButton("Przelacz Hotbar", "ToggleHotbar");
        
        // Blackjack
        CreateRebindButton("Blackjack: Zwieksz zaklad", "BlackjackIncreaseBet");
        CreateRebindButton("Blackjack: Zmniejsz zaklad", "BlackjackDecreaseBet");
        CreateRebindButton("Blackjack: Start", "BlackjackStart");
        CreateRebindButton("Blackjack: Hit", "BlackjackHit");
        CreateRebindButton("Blackjack: Stand", "BlackjackStand");
        
        Debug.Log($"[KeyRebindPanel] Created 13 buttons. Container now has {rebindButtonContainer.childCount} children");
        
        // Force layout rebuild
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rebindButtonContainer.GetComponent<RectTransform>());
    }
    
    /// <summary>
    /// Creates a single rebind button for an action.
    /// </summary>
    private void CreateRebindButton(string displayName, string actionName)
    {
        Debug.Log($"[KeyRebindPanel] Creating button for: {displayName}");
        
        GameObject buttonObj = new GameObject($"RebindButton_{actionName}");
        buttonObj.transform.SetParent(rebindButtonContainer, false);
        
        var layoutGroup = buttonObj.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.childControlWidth = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childControlHeight = false;
        layoutGroup.spacing = 10;
        layoutGroup.padding = new RectOffset(10, 10, 5, 5);
        layoutGroup.childAlignment = TextAnchor.MiddleLeft;
        
        var layoutElement = buttonObj.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 32;
        layoutElement.preferredWidth = 700;
        
        // Label (opis akcji)
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(buttonObj.transform, false);
        TMP_Text labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = displayName;
        labelText.fontSize = 18;
        labelText.color = Color.black;
        labelText.alignment = TextAlignmentOptions.MidlineLeft;
        
        var labelLayout = labelObj.AddComponent<LayoutElement>();
        labelLayout.preferredWidth = 450;
        labelLayout.preferredHeight = 30;
        
        // Button (klawisz)
        GameObject btnObj = new GameObject("Button");
        btnObj.transform.SetParent(buttonObj.transform, false);
        Button btn = btnObj.AddComponent<Button>();
        var btnImg = btnObj.AddComponent<Image>();
        btnImg.color = Color.white;
        
        var btnLayout = btnObj.AddComponent<LayoutElement>();
        btnLayout.preferredWidth = 180;
        btnLayout.preferredHeight = 30;
        
        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        var btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.fontSize = 16;
        btnText.color = Color.black;
        
        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = Vector2.zero;
        
        // Set button text to current key
        if (KeyBindings.Instance != null)
        {
            var currentKey = KeyBindings.Instance.GetBinding(actionName);
            btnText.text = GetKeyDisplayName(currentKey);
        }
        
        // Add click listener
        btn.onClick.AddListener(() => StartRebinding(actionName, btnText));
        
        // Button hover effect
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        btn.colors = colors;
    }
    
    /// <summary>
    /// Starts listening for a new key press to rebind an action.
    /// </summary>
    private void StartRebinding(string actionName, TMP_Text buttonText)
    {
        if (isListening) return;
        
        currentRebindAction = actionName;
        isListening = true;
        
        if (listeningPanel != null)
            listeningPanel.SetActive(true);
        if (listeningText != null)
            listeningText.text = $"Naciśnij klawisz dla:\n{GetActionDisplayName(actionName)}\n\n(ESC aby anulować)";
            
        StartCoroutine(ListenForKey(actionName, buttonText));
    }
    
    /// <summary>
    /// Gets Polish display name for an action.
    /// </summary>
    private string GetActionDisplayName(string actionName)
    {
        return actionName switch
        {
            "MoveUp" => "Ruch w gore",
            "MoveDown" => "Ruch w dol",
            "MoveLeft" => "Ruch w lewo",
            "MoveRight" => "Ruch w prawo",
            "Shoot" => "Strzelanie",
            "SwitchWeapon" => "Zmiana broni",
            "UseItem" => "Uzyj przedmiot",
            "OpenInventory" => "Otworz ekwipunek",
            "Interact" => "Interakcja",
            "OpenShop" => "Otworz sklep",
            "BlackjackIncreaseBet" => "Zwieksz zaklad",
            "BlackjackDecreaseBet" => "Zmniejsz zaklad",
            "BlackjackStart" => "Start gry",
            "BlackjackHit" => "Hit",
            "BlackjackStand" => "Stand",
            _ => actionName
        };
    }
    
    /// <summary>
    /// Gets display name for a key (Polish names for common keys).
    /// </summary>
    private string GetKeyDisplayName(Key key)
    {
        return key switch
        {
            Key.Space => "Spacja",
            Key.LeftShift => "L-Shift",
            Key.RightShift => "R-Shift",
            Key.LeftCtrl => "L-Ctrl",
            Key.RightCtrl => "R-Ctrl",
            Key.LeftAlt => "L-Alt",
            Key.RightAlt => "R-Alt",
            Key.Enter => "Enter",
            Key.Backspace => "Backspace",
            Key.Tab => "Tab",
            Key.Escape => "Esc",
            _ => key.ToString()
        };
    }
    
    /// <summary>
    /// Coroutine that listens for a key press.
    /// </summary>
    private IEnumerator ListenForKey(string actionName, TMP_Text buttonText)
    {
        yield return null; // Wait a frame to avoid capturing the click
        
        while (isListening)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                yield return null;
                continue;
            }
            
            // Check for ESC to cancel
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                CancelRebinding();
                yield break;
            }
            
            // Check all keys
            foreach (Key key in System.Enum.GetValues(typeof(Key)))
            {
                if (key == Key.None || key == Key.Escape) continue;
                
                var keyControl = keyboard[key];
                if (keyControl == null) continue; // Skip keys that don't exist on current keyboard
                
                if (keyControl.wasPressedThisFrame)
                {
                    // Check if key is already used
                    if (IsKeyAlreadyBound(key, actionName))
                    {
                        // Show warning
                        if (listeningText != null)
                            listeningText.text = $"Klawisz {GetKeyDisplayName(key)} jest juz uzyty!\n\nWybierz inny klawisz\n(ESC aby anulowac)";
                        yield return new WaitForSeconds(1.5f);
                        if (listeningText != null)
                            listeningText.text = $"Nacisnij klawisz dla:\n{GetActionDisplayName(actionName)}\n\n(ESC aby anulowac)";
                        continue;
                    }
                    
                    // Rebind the key
                    if (KeyBindings.Instance != null)
                    {
                        KeyBindings.Instance.SetBinding(actionName, key);
                        if (buttonText != null)
                            buttonText.text = GetKeyDisplayName(key);
                    }
                    
                    CancelRebinding();
                    yield break;
                }
            }
            
            // Check mouse buttons
            var mouse = Mouse.current;
            if (mouse != null)
            {
                // Left mouse button
                if (mouse.leftButton.wasPressedThisFrame)
                {
                    AssignMouseButton("LeftButton", actionName, buttonText);
                    yield break;
                }
                // Right mouse button
                if (mouse.rightButton.wasPressedThisFrame)
                {
                    AssignMouseButton("RightButton", actionName, buttonText);
                    yield break;
                }
                // Middle mouse button
                if (mouse.middleButton.wasPressedThisFrame)
                {
                    AssignMouseButton("MiddleButton", actionName, buttonText);
                    yield break;
                }
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// Assigns a mouse button to an action.
    /// </summary>
    private void AssignMouseButton(string mouseButton, string actionName, TMP_Text buttonText)
    {
        // For now, store mouse button as special key name in PlayerPrefs
        PlayerPrefs.SetString($"Key_{actionName}", mouseButton);
        PlayerPrefs.Save();
        
        if (buttonText != null)
            buttonText.text = GetMouseButtonDisplayName(mouseButton);
        
        CancelRebinding();
    }
    
    /// <summary>
    /// Gets display name for mouse buttons.
    /// </summary>
    private string GetMouseButtonDisplayName(string mouseButton)
    {
        return mouseButton switch
        {
            "LeftButton" => "LPM",
            "RightButton" => "PPM",
            "MiddleButton" => "Srodkowy przycisk myszy",
            _ => mouseButton
        };
    }
    
    /// <summary>
    /// Checks if a key is already bound to another action.
    /// </summary>
    private bool IsKeyAlreadyBound(Key key, string excludeAction)
    {
        if (KeyBindings.Instance == null) return false;
        
        string[] allActions = new string[]
        {
            "MoveUp", "MoveDown", "MoveLeft", "MoveRight",
            "SwitchWeapon", "UseItem",
            "Interact",
            "BlackjackIncreaseBet", "BlackjackDecreaseBet",
            "BlackjackStart", "BlackjackHit", "BlackjackStand"
        };
        
        foreach (string action in allActions)
        {
            if (action == excludeAction) continue;
            
            if (KeyBindings.Instance.GetBinding(action) == key)
                return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Cancels the current rebinding operation.
    /// </summary>
    private void CancelRebinding()
    {
        isListening = false;
        currentRebindAction = null;
        
        if (listeningPanel != null)
            listeningPanel.SetActive(false);
    }
    
    /// <summary>
    /// Resets all key bindings to default values.
    /// </summary>
    private void ResetToDefaults()
    {
        if (KeyBindings.Instance != null)
        {
            KeyBindings.Instance.ResetToDefaults();
            RefreshButtonLabels();
        }
    }
    
    /// <summary>
    /// Refreshes all button labels to show current key bindings.
    /// </summary>
    private void RefreshButtonLabels()
    {
        if (rebindButtonContainer == null || KeyBindings.Instance == null) return;
        
        foreach (Transform child in rebindButtonContainer)
        {
            var btn = child.GetComponentInChildren<Button>();
            if (btn == null) continue;
            
            var btnText = btn.GetComponentInChildren<TMP_Text>();
            if (btnText == null) continue;
            
            // Extract action name from object name
            string objName = child.gameObject.name;
            if (objName.StartsWith("RebindButton_"))
            {
                string actionName = objName.Substring("RebindButton_".Length);
                var currentKey = KeyBindings.Instance.GetBinding(actionName);
                btnText.text = currentKey.ToString();
            }
        }
    }
}
