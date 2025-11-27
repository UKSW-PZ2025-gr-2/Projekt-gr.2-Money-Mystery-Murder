using UnityEngine;
using UnityEngine.UI; // still needed for Canvas / CanvasScaler / GraphicRaycaster
using TMPro; // TextMeshPro
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Classic slot machine minigame with weighted symbols and payout logic.
// Costs startCost balance to start. Generates 3 random symbols and displays them.
// UI (Canvas + 3 TMP objects) is created dynamically on start and destroyed on end.
public class SlotMachineMinigame : MinigameBase
{
    [Header("Slot Machine Settings")]
    [SerializeField] private int startCost = 10; // fixed play cost
    [SerializeField] private Color textColor = Color.yellow;
    [SerializeField] private int fontSize = 64;
    [SerializeField] private Vector2 spacing = new(160f, 0f); // horizontal spacing between numbers
    [SerializeField] private float autoEndSeconds = 3f; // auto end after this duration

    private GameObject _uiRoot;
    private TextMeshProUGUI[] _texts; // switched to TMP
    private string[] _values = new string[3];
    private float _elapsed;

    // Weighted symbol definitions (total weights sum to 100)
    private static readonly (string symbol, int weight)[] _symbolWeights = new (string, int)[]
    {
        ("CHERRY", 40),
        ("LEMON", 30),
        ("ORANGE", 20),
        ("PLUM", 7),
        ("BELL", 2),
        ("7", 1)
    };

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

    protected override void OnEndGame()
    {
        CleanupUI();
        Debug.Log("[SlotMachineMinigame] Ended.");
    }

    protected override int GetStartCost() => startCost;
    protected override bool AllowNegativeBalanceOnStart() => false; // do not allow going negative

    private void GenerateSymbols()
    {
        for (int i = 0; i < _values.Length; i++)
        {
            _values[i] = PickWeightedSymbol();
        }
    }

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
