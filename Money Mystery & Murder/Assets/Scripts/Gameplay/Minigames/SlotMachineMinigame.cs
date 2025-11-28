using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Classic slot machine minigame with weighted symbols and payout logic.
/// Costs a fixed balance amount to start. Generates 3 random symbols and displays them.
/// UI (Canvas + 3 TextMeshPro objects) is created dynamically on start and destroyed on end.
/// Inherits from <see cref="MinigameBase"/> and integrates with <see cref="Player"/>.
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
    /// Color of the slot display text.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private Color textColor = Color.yellow;
    
    /// <summary>
    /// Font size for the slot display text.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private int fontSize = 64;
    
    /// <summary>
    /// Horizontal spacing between slot display numbers.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private Vector2 spacing = new(160f, 0f);
    
    /// <summary>
    /// Auto-end duration in seconds after game starts.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private float autoEndSeconds = 3f;

    /// <summary>Root GameObject for the dynamically created UI.</summary>
    private GameObject _uiRoot;
    
    /// <summary>Array of TextMeshPro components displaying the slot symbols.</summary>
    private TextMeshProUGUI[] _texts;
    
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
    /// Called when the minigame starts. Generates symbols, builds UI, calculates payout, and credits the <see cref="Player"/>.
    /// </summary>
    protected override void OnStartGame()
    {
        _elapsed = 0f;
        GenerateSymbols();
        BuildUI();
        UpdateUI();
        int payout = CalculatePayout();
        if (payout > 0)
        {
            if (ActivatingPlayer != null)
            {
                ActivatingPlayer.AddBalance(payout);
            }
            Debug.Log($"[SlotMachineMinigame] Payout: {payout} coins.");
        }
        Debug.Log($"[SlotMachineMinigame] Started. Symbols: {_values[0]}, {_values[1]}, {_values[2]} (autoEnd={autoEndSeconds}s)");
    }

    /// <summary>
    /// Called when the minigame ends. Cleans up the dynamically created UI.
    /// </summary>
    protected override void OnEndGame()
    {
        CleanupUI();
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
                "7" => 500,
                "BELL" => 100,
                "PLUM" => 50,
                "ORANGE" => 20,
                "LEMON" => 10,
                "CHERRY" => 5,
                _ => 0
            };
        }

        // Any cherries logic
        int cherryCount = 0;
        if (a == "CHERRY") cherryCount++;
        if (b == "CHERRY") cherryCount++;
        if (c == "CHERRY") cherryCount++;
        if (cherryCount == 2) return 2;
        if (cherryCount == 1) return 1;
        return 0;
    }

    /// <summary>Builds the UI Canvas and TextMeshPro components for displaying slot symbols.</summary>
    private void BuildUI()
    {
        CleanupUI();

        _uiRoot = new GameObject("SlotMachineUI");
        var canvas = _uiRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _uiRoot.AddComponent<CanvasScaler>();
        _uiRoot.AddComponent<GraphicRaycaster>();

        _texts = new TextMeshProUGUI[3];

        for (int i = 0; i < 3; i++)
        {
            var go = new GameObject($"SlotTMP_{i}");
            go.transform.SetParent(_uiRoot.transform, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = "?";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor;
            tmp.fontSize = fontSize;
            if (TMP_Settings.defaultFontAsset != null)
            {
                tmp.font = TMP_Settings.defaultFontAsset;
            }
            else
            {
                var fallback = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                if (fallback != null) tmp.font = fallback;
            }

            var rt = tmp.rectTransform;
            rt.sizeDelta = new Vector2(fontSize * 1.5f, fontSize * 1.5f);
            rt.anchoredPosition = new Vector2((i - 1) * spacing.x, (i - 1) * spacing.y);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            _texts[i] = tmp;
        }
    }

    /// <summary>Updates the UI text components with the current symbol values.</summary>
    private void UpdateUI()
    {
        if (_texts == null) return;
        for (int i = 0; i < _texts.Length; i++)
        {
            if (_texts[i] != null)
            {
                _texts[i].text = _values[i];
            }
        }
    }

    /// <summary>Destroys the UI root GameObject and clears references.</summary>
    private void CleanupUI()
    {
        if (_uiRoot != null)
        {
            Destroy(_uiRoot);
            _uiRoot = null;
        }
        _texts = null;
    }

#if ENABLE_INPUT_SYSTEM
    /// <summary>
    /// Updates the minigame timer and handles Escape key to end the game (Input System version).
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
#else
    /// <summary>
    /// Updates the minigame timer and handles Escape key to end the game (Legacy Input version).
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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EndGame();
        }
    }
#endif
}
