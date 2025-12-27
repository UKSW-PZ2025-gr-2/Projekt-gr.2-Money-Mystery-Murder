using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// Classic slot machine minigame with weighted symbols and payout logic.
/// Costs a fixed balance amount to start. Generates 3 random symbols and displays them.
/// Sprites are created dynamically in world space on start and destroyed on end.
/// Inherits from <see cref="MinigameBase"/> and integrates with <see cref="Player"/>.
/// Can award a rare Golden Knife weapon on jackpot (triple 7s).
/// </summary>
public class SlotMachineMinigame : MinigameBase
{
    /// <summary>
    /// Fixed cost to play the slot machine.
    /// Set this in the Unity Inspector.
    /// </summary>
    [Header("Slot Machine Settings")]
    [SerializeField] private int startCost = 10;
    
    /// <summary>
    /// Horizontal spacing between slot display sprites in world units.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private float spacing = 1.5f;
    
    /// <summary>
    /// Vertical offset from the slot machine position for the sprites.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private float verticalOffset = 2f;
    
    /// <summary>
    /// Scale for the sprite renderers.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private Vector3 spriteScale = new Vector3(5f, 5f, 5f);
    
    /// <summary>
    /// Auto-end duration in seconds after game starts.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private float autoEndSeconds = 3f;

    /// <summary>
    /// Sorting layer name for the slot sprites.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private string sortingLayerName = "Default";
    
    /// <summary>
    /// Sorting order for the slot sprites.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private int sortingOrder = 10;

    [Header("Symbol Sprites")]
    [Tooltip("Sprite for CHERRY symbol")]
    [SerializeField] private Sprite cherrySprite;
    
    [Tooltip("Sprite for LEMON symbol")]
    [SerializeField] private Sprite lemonSprite;
    
    [Tooltip("Sprite for ORANGE symbol")]
    [SerializeField] private Sprite orangeSprite;
    
    [Tooltip("Sprite for PLUM symbol")]
    [SerializeField] private Sprite plumSprite;
    
    [Tooltip("Sprite for BELL symbol")]
    [SerializeField] private Sprite bellSprite;
    
    [Tooltip("Sprite for 7 symbol")]
    [SerializeField] private Sprite sevenSprite;
    
    [Tooltip("Sprite for unknown/placeholder symbol")]
    [SerializeField] private Sprite questionSprite;

    [Header("Payout Settings")]
    [Tooltip("Payout for three 7 symbols (jackpot)")]
    [SerializeField] private int payout7 = 500;
    
    [Tooltip("Payout for three BELL symbols")]
    [SerializeField] private int payoutBell = 100;
    
    [Tooltip("Payout for three PLUM symbols")]
    [SerializeField] private int payoutPlum = 50;
    
    [Tooltip("Payout for three ORANGE symbols")]
    [SerializeField] private int payoutOrange = 20;
    
    [Tooltip("Payout for three LEMON symbols")]
    [SerializeField] private int payoutLemon = 10;
    
    [Tooltip("Payout for three CHERRY symbols")]
    [SerializeField] private int payoutCherry = 5;
    
    [Tooltip("Payout for two CHERRY symbols")]
    [SerializeField] private int payoutTwoCherries = 2;
    
    [Tooltip("Payout for one CHERRY symbol")]
    [SerializeField] private int payoutOneCherry = 1;

    /// <summary>Prefab for the Golden Knife weapon awarded on jackpot.</summary>
    [Header("Rare Rewards")]
    [Tooltip("Golden Knife weapon data awarded on triple 7s jackpot")]
    [SerializeField] private WeaponData goldenKnifeData;

    /// <summary>Parent GameObject for the dynamically created sprites.</summary>
    private GameObject _spriteRoot;
    
    /// <summary>Array of SpriteRenderer components displaying the slot symbols.</summary>
    private SpriteRenderer[] _spriteRenderers;
    
    /// <summary>Array storing the generated symbol strings.</summary>
    private string[] _values = new string[3];
    
    /// <summary>Elapsed time since game started.</summary>
    private float _elapsed;

    /// <summary>Weighted symbol definitions with weights summing to 100.</summary>
    private static readonly (string symbol, int weight)[] _symbolWeights = new (string, int)[]
    {
        ("CHERRY", 40),
        ("LEMON", 30),
        ("ORANGE", 20),
        ("PLUM", 7),
        ("BELL", 2),
        ("7", 1)
    };

    /// <summary>
    /// Called when the minigame starts. Generates symbols, builds sprites, calculates payout, and credits the <see cref="Player"/>.
    /// On triple 7s jackpot, awards the rare Golden Knife weapon instead of money.
    /// </summary>
    protected override void OnStartGame()
    {
        _elapsed = 0f;
        GenerateSymbols();
        BuildSprites();
        UpdateSprites();
        
        // Check for jackpot (triple 7s) first
        bool isJackpot = _values[0] == "7" && _values[1] == "7" && _values[2] == "7";
        if (isJackpot && goldenKnifeData != null && ActivatingPlayer != null)
        {
            // Award rare Golden Knife weapon
            ActivatingPlayer.AcquireWeapon(goldenKnifeData);
            ActivatingPlayer.EquipWeapon(goldenKnifeData);
            Debug.Log($"[SlotMachineMinigame] JACKPOT! Awarded Golden Knife to {ActivatingPlayer.name}!");
        }
        else
        {
            // Normal payout
            int payout = CalculatePayout();
            if (payout > 0)
            {
                if (ActivatingPlayer != null)
                {
                    ActivatingPlayer.AddBalance(payout);
                    Debug.Log($"[SlotMachineMinigame] Payout: {payout} to {ActivatingPlayer.name}");
                }
            }
        }
        
        Debug.Log($"[SlotMachineMinigame] Started. Symbols: {_values[0]}, {_values[1]}, {_values[2]} (autoEnd={autoEndSeconds}s)");
    }

    /// <summary>
    /// Called when the minigame ends. Cleans up the dynamically created sprites.
    /// </summary>
    protected override void OnEndGame()
    {
        CleanupSprites();
        Debug.Log("[SlotMachineMinigame] Ended.");
    }

    /// <summary>
    /// Returns the cost required to start the minigame.
    /// </summary>
    /// <returns>The start cost in balance.</returns>
    protected override int GetStartCost() => startCost;
    
    /// <summary>
    /// Returns whether to allow the player's balance to go negative when paying start cost.
    /// </summary>
    /// <returns>False (does not allow negative balance).</returns>
    protected override bool AllowNegativeBalanceOnStart() => false;

    /// <summary>Generates three random weighted symbols for the slot machine.</summary>
    private void GenerateSymbols()
    {
        for (int i = 0; i < _values.Length; i++)
        {
            _values[i] = PickWeightedSymbol();
        }
    }

    /// <summary>
    /// Picks a single weighted symbol using random roll.
    /// </summary>
    /// <returns>A symbol string.</returns>
    private string PickWeightedSymbol()
    {
        int total = 0;
        for (int i = 0; i < _symbolWeights.Length; i++) total += _symbolWeights[i].weight;
        int roll = Random.Range(0, total); // 0..total-1
        int cumulative = 0;
        for (int i = 0; i < _symbolWeights.Length; i++)
        {
            cumulative += _symbolWeights[i].weight;
            if (roll < cumulative) return _symbolWeights[i].symbol;
        }
        return _symbolWeights[_symbolWeights.Length - 1].symbol; // fallback safety
    }

    /// <summary>
    /// Calculates the payout based on the generated symbols.
    /// </summary>
    /// <returns>Payout amount in balance.</returns>
    private int CalculatePayout()
    {
        string a = _values[0];
        string b = _values[1];
        string c = _values[2];

        // 3-of-a-kind
        if (a == b && b == c)
        {
            return a switch
            {
                "7" => payout7,
                "BELL" => payoutBell,
                "PLUM" => payoutPlum,
                "ORANGE" => payoutOrange,
                "LEMON" => payoutLemon,
                "CHERRY" => payoutCherry,
                _ => 0
            };
        }

        // Any cherries logic
        int cherryCount = 0;
        if (a == "CHERRY") cherryCount++;
        if (b == "CHERRY") cherryCount++;
        if (c == "CHERRY") cherryCount++;
        if (cherryCount == 2) return payoutTwoCherries;
        if (cherryCount == 1) return payoutOneCherry;
        return 0;
    }

    /// <summary>Builds the sprite GameObjects and SpriteRenderer components for displaying slot symbols in world space.</summary>
    private void BuildSprites()
    {
        CleanupSprites();

        _spriteRoot = new GameObject("SlotMachineSprites");
        _spriteRoot.transform.SetParent(transform, false);
        _spriteRoot.transform.localPosition = Vector3.zero;

        _spriteRenderers = new SpriteRenderer[3];

        for (int i = 0; i < 3; i++)
        {
            var go = new GameObject($"Slot_{i}");
            go.transform.SetParent(_spriteRoot.transform, false);
            
            // Position relative to slot machine
            Vector3 localPos = new Vector3((i - 1) * spacing, verticalOffset, 0f);
            go.transform.localPosition = localPos;
            go.transform.localScale = spriteScale;
            
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = questionSprite;
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = sortingOrder;

            _spriteRenderers[i] = sr;
        }
    }

    /// <summary>Updates the sprite renderers with the current symbol sprites.</summary>
    private void UpdateSprites()
    {
        if (_spriteRenderers == null) return;
        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            if (_spriteRenderers[i] != null)
            {
                _spriteRenderers[i].sprite = GetSpriteForSymbol(_values[i]);
            }
        }
    }

    /// <summary>
    /// Returns the appropriate sprite for a given symbol string.
    /// </summary>
    /// <param name="symbol">The symbol string.</param>
    /// <returns>The corresponding sprite, or questionSprite if not found.</returns>
    private Sprite GetSpriteForSymbol(string symbol)
    {
        return symbol switch
        {
            "CHERRY" => cherrySprite != null ? cherrySprite : questionSprite,
            "LEMON" => lemonSprite != null ? lemonSprite : questionSprite,
            "ORANGE" => orangeSprite != null ? orangeSprite : questionSprite,
            "PLUM" => plumSprite != null ? plumSprite : questionSprite,
            "BELL" => bellSprite != null ? bellSprite : questionSprite,
            "7" => sevenSprite != null ? sevenSprite : questionSprite,
            _ => questionSprite
        };
    }

    /// <summary>Destroys the sprite root GameObject and clears references.</summary>
    private void CleanupSprites()
    {
        if (_spriteRoot != null)
        {
            Destroy(_spriteRoot);
            _spriteRoot = null;
        }
        _spriteRenderers = null;
    }

    /// <summary>
    /// Updates the minigame timer and handles Escape key to end the game.
    /// </summary>
    void Update()
    {
        if (!IsRunning) return;
        
        _elapsed += Time.deltaTime;
        if (autoEndSeconds > 0f && _elapsed >= autoEndSeconds)
        {
            EndGame();
            return;
        }
        
        var k = Keyboard.current;
        if (k != null && k.escapeKey.wasPressedThisFrame)
        {
            EndGame();
        }
    }
}
