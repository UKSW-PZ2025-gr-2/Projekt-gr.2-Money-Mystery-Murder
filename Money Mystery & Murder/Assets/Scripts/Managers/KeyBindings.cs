using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Centralized key bindings manager that stores all configurable controls.
/// Saves and loads key bindings from PlayerPrefs.
/// Used by PlayerMovement, BlackjackMinigame, MinigameActivator, and other systems.
/// </summary>
public class KeyBindings : MonoBehaviour
{
    public static KeyBindings Instance { get; private set; }
    
    // Movement keys
    public Key MoveUp { get; private set; } = Key.W;
    public Key MoveDown { get; private set; } = Key.S;
    public Key MoveLeft { get; private set; } = Key.A;
    public Key MoveRight { get; private set; } = Key.D;
    
    // Combat keys
    public Key Shoot { get; private set; } = Key.Space;
    public Key SwitchWeapon { get; private set; } = Key.Q;
    
    // Inventory keys
    public Key UseItem { get; private set; } = Key.F;
    public Key OpenInventory { get; private set; } = Key.I;
    
    // Interaction keys
    public Key Interact { get; private set; } = Key.E;
    public Key OpenShop { get; private set; } = Key.B;
    
    // Hotbar keys
    public Key ToggleHotbar { get; private set; } = Key.Tab;
    
    // Blackjack keys
    public Key BlackjackIncreaseBet { get; private set; } = Key.L;
    public Key BlackjackDecreaseBet { get; private set; } = Key.J;
    public Key BlackjackStart { get; private set; } = Key.Space;
    public Key BlackjackHit { get; private set; } = Key.H;
    public Key BlackjackStand { get; private set; } = Key.S;
    
    // Events fired when keys are rebound
    public event Action OnKeysChanged;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        LoadBindings();
        
        Debug.Log("[KeyBindings] Initialized and loaded bindings");
    }
    
    /// <summary>
    /// Ensures KeyBindings instance exists, creating one if needed.
    /// Call this early in game initialization (e.g., from a bootstrap script).
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureInstance()
    {
        if (Instance == null)
        {
            GameObject obj = new GameObject("KeyBindings");
            obj.AddComponent<KeyBindings>();
            Debug.Log("[KeyBindings] Auto-created instance");
        }
    }
    
    /// <summary>
    /// Sets a new key binding for the specified action.
    /// </summary>
    public void SetBinding(string actionName, Key newKey)
    {
        switch (actionName)
        {
            case "MoveUp": MoveUp = newKey; break;
            case "MoveDown": MoveDown = newKey; break;
            case "MoveLeft": MoveLeft = newKey; break;
            case "MoveRight": MoveRight = newKey; break;
            case "Shoot": Shoot = newKey; break;
            case "SwitchWeapon": SwitchWeapon = newKey; break;
            case "UseItem": UseItem = newKey; break;
            case "OpenInventory": OpenInventory = newKey; break;
            case "Interact": Interact = newKey; break;
            case "OpenShop": OpenShop = newKey; break;
            case "ToggleHotbar": ToggleHotbar = newKey; break;
            case "BlackjackIncreaseBet": BlackjackIncreaseBet = newKey; break;
            case "BlackjackDecreaseBet": BlackjackDecreaseBet = newKey; break;
            case "BlackjackStart": BlackjackStart = newKey; break;
            case "BlackjackHit": BlackjackHit = newKey; break;
            case "BlackjackStand": BlackjackStand = newKey; break;
            default:
                Debug.LogWarning($"[KeyBindings] Unknown action: {actionName}");
                return;
        }
        
        SaveBindings();
        OnKeysChanged?.Invoke();
        Debug.Log($"[KeyBindings] {actionName} rebound to {newKey}");
    }
    
    /// <summary>
    /// Gets the current key for the specified action.
    /// </summary>
    public Key GetBinding(string actionName)
    {
        return actionName switch
        {
            "MoveUp" => MoveUp,
            "MoveDown" => MoveDown,
            "MoveLeft" => MoveLeft,
            "MoveRight" => MoveRight,
            "Shoot" => Shoot,
            "SwitchWeapon" => SwitchWeapon,
            "UseItem" => UseItem,
            "OpenInventory" => OpenInventory,
            "Interact" => Interact,
            "OpenShop" => OpenShop,
            "ToggleHotbar" => ToggleHotbar,
            "BlackjackIncreaseBet" => BlackjackIncreaseBet,
            "BlackjackDecreaseBet" => BlackjackDecreaseBet,
            "BlackjackStart" => BlackjackStart,
            "BlackjackHit" => BlackjackHit,
            "BlackjackStand" => BlackjackStand,
            _ => Key.None
        };
    }
    
    /// <summary>
    /// Resets all bindings to default values.
    /// </summary>
    public void ResetToDefaults()
    {
        MoveUp = Key.W;
        MoveDown = Key.S;
        MoveLeft = Key.A;
        MoveRight = Key.D;
        Shoot = Key.Space;
        SwitchWeapon = Key.Q;
        UseItem = Key.F;
        OpenInventory = Key.I;
        Interact = Key.E;
        OpenShop = Key.B;
        ToggleHotbar = Key.Tab;
        BlackjackIncreaseBet = Key.L;
        BlackjackDecreaseBet = Key.J;
        BlackjackStart = Key.Space;
        BlackjackHit = Key.H;
        BlackjackStand = Key.S;
        
        SaveBindings();
        OnKeysChanged?.Invoke();
        Debug.Log("[KeyBindings] Reset to defaults");
    }
    
    /// <summary>
    /// Saves all key bindings to PlayerPrefs.
    /// </summary>
    private void SaveBindings()
    {
        PlayerPrefs.SetInt("Key_MoveUp", (int)MoveUp);
        PlayerPrefs.SetInt("Key_MoveDown", (int)MoveDown);
        PlayerPrefs.SetInt("Key_MoveLeft", (int)MoveLeft);
        PlayerPrefs.SetInt("Key_MoveRight", (int)MoveRight);
        PlayerPrefs.SetInt("Key_Shoot", (int)Shoot);
        PlayerPrefs.SetInt("Key_SwitchWeapon", (int)SwitchWeapon);
        PlayerPrefs.SetInt("Key_UseItem", (int)UseItem);
        PlayerPrefs.SetInt("Key_OpenInventory", (int)OpenInventory);
        PlayerPrefs.SetInt("Key_Interact", (int)Interact);
        PlayerPrefs.SetInt("Key_OpenShop", (int)OpenShop);
        PlayerPrefs.SetInt("Key_ToggleHotbar", (int)ToggleHotbar);
        PlayerPrefs.SetInt("Key_BlackjackIncreaseBet", (int)BlackjackIncreaseBet);
        PlayerPrefs.SetInt("Key_BlackjackDecreaseBet", (int)BlackjackDecreaseBet);
        PlayerPrefs.SetInt("Key_BlackjackStart", (int)BlackjackStart);
        PlayerPrefs.SetInt("Key_BlackjackHit", (int)BlackjackHit);
        PlayerPrefs.SetInt("Key_BlackjackStand", (int)BlackjackStand);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Loads all key bindings from PlayerPrefs.
    /// </summary>
    private void LoadBindings()
    {
        MoveUp = (Key)PlayerPrefs.GetInt("Key_MoveUp", (int)Key.W);
        MoveDown = (Key)PlayerPrefs.GetInt("Key_MoveDown", (int)Key.S);
        MoveLeft = (Key)PlayerPrefs.GetInt("Key_MoveLeft", (int)Key.A);
        MoveRight = (Key)PlayerPrefs.GetInt("Key_MoveRight", (int)Key.D);
        Shoot = (Key)PlayerPrefs.GetInt("Key_Shoot", (int)Key.Space);
        SwitchWeapon = (Key)PlayerPrefs.GetInt("Key_SwitchWeapon", (int)Key.Q);
        UseItem = (Key)PlayerPrefs.GetInt("Key_UseItem", (int)Key.F);
        OpenInventory = (Key)PlayerPrefs.GetInt("Key_OpenInventory", (int)Key.I);
        Interact = (Key)PlayerPrefs.GetInt("Key_Interact", (int)Key.E);
        OpenShop = (Key)PlayerPrefs.GetInt("Key_OpenShop", (int)Key.B);
        ToggleHotbar = (Key)PlayerPrefs.GetInt("Key_ToggleHotbar", (int)Key.Tab);
        BlackjackIncreaseBet = (Key)PlayerPrefs.GetInt("Key_BlackjackIncreaseBet", (int)Key.L);
        BlackjackDecreaseBet = (Key)PlayerPrefs.GetInt("Key_BlackjackDecreaseBet", (int)Key.J);
        BlackjackStart = (Key)PlayerPrefs.GetInt("Key_BlackjackStart", (int)Key.Space);
        BlackjackHit = (Key)PlayerPrefs.GetInt("Key_BlackjackHit", (int)Key.H);
        BlackjackStand = (Key)PlayerPrefs.GetInt("Key_BlackjackStand", (int)Key.S);
        
        Debug.Log("[KeyBindings] Loaded from PlayerPrefs");
    }
    
    /// <summary>
    /// Checks if a key is pressed this frame using current bindings.
    /// </summary>
    public bool IsKeyPressed(Key key)
    {
        var k = Keyboard.current;
        return k != null && k[key].isPressed;
    }
    
    /// <summary>
    /// Checks if a key was pressed this frame using current bindings.
    /// </summary>
    public bool WasKeyPressedThisFrame(Key key)
    {
        var k = Keyboard.current;
        return k != null && k[key].wasPressedThisFrame;
    }
}
