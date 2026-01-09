using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Editor tool to automatically create a complete UI setup for BlackjackMinigame.
/// Menu: Tools > Create Blackjack UI
/// </summary>
public class BlackjackUIGenerator : EditorWindow
{
    private GameObject blackjackObject;
    
    [MenuItem("Tools/Create Blackjack UI")]
    static void Init()
    {
        BlackjackUIGenerator window = (BlackjackUIGenerator)EditorWindow.GetWindow(typeof(BlackjackUIGenerator));
        window.titleContent = new GUIContent("Blackjack UI Generator");
        window.Show();
    }
    
    void OnGUI()
    {
        GUILayout.Label("Blackjack UI Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("1. Select the GameObject with BlackjackMinigame component");
        GUILayout.Label("2. Click 'Generate UI' button");
        GUILayout.Label("3. UI will be created as child of the object");
        GUILayout.Space(10);
        
        blackjackObject = (GameObject)EditorGUILayout.ObjectField("Blackjack Object", blackjackObject, typeof(GameObject), true);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Generate Complete Blackjack UI", GUILayout.Height(40)))
        {
            if (blackjackObject == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a GameObject with BlackjackMinigame component!", "OK");
                return;
            }
            
            var blackjack = blackjackObject.GetComponent<BlackjackMinigame>();
            if (blackjack == null)
            {
                EditorUtility.DisplayDialog("Error", "Selected GameObject doesn't have BlackjackMinigame component!", "OK");
                return;
            }
            
            GenerateUI(blackjackObject, blackjack);
        }
    }
    
    void GenerateUI(GameObject parent, BlackjackMinigame blackjack)
    {
        // Create Canvas - World Space nad obiektem
        GameObject canvasObj = new GameObject("BlackjackCanvas");
        canvasObj.transform.SetParent(parent.transform, false);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        // Pozycjonowanie
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(600, 400);
        canvasRect.localPosition = new Vector3(0, 3, 0);
        canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Main UI Panel
        GameObject uiPanel = CreateUIObject("UIPanel", canvasObj.transform);
        RectTransform uiPanelRect = uiPanel.AddComponent<RectTransform>();
        uiPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
        uiPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
        uiPanelRect.sizeDelta = new Vector2(600, 400);
        uiPanelRect.anchoredPosition = Vector2.zero;
        Image uiPanelImg = uiPanel.AddComponent<Image>();
        uiPanelImg.color = new Color(0, 0, 0, 0.9f);
        
        // === BETTING PANEL ===
        GameObject bettingPanel = CreateUIObject("BettingPanel", uiPanel.transform);
        RectTransform bettingRect = bettingPanel.AddComponent<RectTransform>();
        bettingRect.anchorMin = Vector2.zero;
        bettingRect.anchorMax = Vector2.one;
        bettingRect.sizeDelta = Vector2.zero;
        bettingRect.anchoredPosition = Vector2.zero;
        
        // Title
        CreateText("Title", bettingPanel.transform, "BLACKJACK", 28, new Vector2(0, 150));
        
        // Bet amount display
        GameObject betText = CreateText("BetAmountText", bettingPanel.transform, "Bet: $10", 24, new Vector2(0, 80));
        
        // Keyboard hints
        CreateText("HintIncrease", bettingPanel.transform, "[L] +10", 18, new Vector2(-120, 30));
        CreateText("HintDecrease", bettingPanel.transform, "[J] -10", 18, new Vector2(120, 30));
        CreateText("HintStart", bettingPanel.transform, "[SPACE] START", 20, new Vector2(0, -40));
        
        // Hidden buttons (for compatibility)
        GameObject betIncBtn = CreateButton("BetIncreaseButton", bettingPanel.transform, "+", new Vector2(5000, 0));
        GameObject betDecBtn = CreateButton("BetDecreaseButton", bettingPanel.transform, "-", new Vector2(5000, 0));
        GameObject startBtn = CreateButton("StartGameButton", bettingPanel.transform, "START", new Vector2(5000, 0));
        RectTransform startBtnRect = startBtn.GetComponent<RectTransform>();
        startBtnRect.sizeDelta = new Vector2(140, 35);
        
        // === GAME PANEL ===
        GameObject gamePanel = CreateUIObject("GamePanel", uiPanel.transform);
        RectTransform gamePanelRect = gamePanel.AddComponent<RectTransform>();
        gamePanelRect.anchorMin = Vector2.zero;
        gamePanelRect.anchorMax = Vector2.one;
        gamePanelRect.sizeDelta = Vector2.zero;
        gamePanelRect.anchoredPosition = Vector2.zero;
        gamePanel.SetActive(false); // Hidden by default
        
        // Dealer section
        CreateText("DealerLabel", gamePanel.transform, "DEALER", 16, new Vector2(0, 90), TextAlignmentOptions.Center);
        GameObject dealerCardsText = CreateText("DealerCardsText", gamePanel.transform, "", 14, new Vector2(0, 65));
        GameObject dealerScoreText = CreateText("DealerScoreText", gamePanel.transform, "Dealer: 0", 14, new Vector2(0, 45));
        
        // Player section
        CreateText("PlayerLabel", gamePanel.transform, "PLAYER", 16, new Vector2(0, 10), TextAlignmentOptions.Center);
        GameObject playerCardsText = CreateText("PlayerCardsText", gamePanel.transform, "", 14, new Vector2(0, -15));
        GameObject playerScoreText = CreateText("PlayerScoreText", gamePanel.transform, "Player: 0", 14, new Vector2(0, -35));
        
        // Message
        GameObject messageText = CreateText("MessageText", gamePanel.transform, "", 20, new Vector2(0, -80));
        messageText.GetComponent<TMP_Text>().color = Color.yellow;
        
        // Keyboard hints
        CreateText("HintHit", gamePanel.transform, "[H] HIT", 18, new Vector2(-100, -140));
        CreateText("HintStand", gamePanel.transform, "[S] STAND", 18, new Vector2(100, -140));
        
        // Hidden buttons (for compatibility)
        GameObject hitBtn = CreateButton("HitButton", gamePanel.transform, "HIT", new Vector2(5000, 0));
        RectTransform hitBtnRect = hitBtn.GetComponent<RectTransform>();
        hitBtnRect.sizeDelta = new Vector2(100, 30);
        
        GameObject standBtn = CreateButton("StandButton", gamePanel.transform, "STAND", new Vector2(5000, 0));
        RectTransform standBtnRect = standBtn.GetComponent<RectTransform>();
        standBtnRect.sizeDelta = new Vector2(100, 30);
        
        // === ASSIGN REFERENCES TO BLACKJACK COMPONENT ===
        SerializedObject so = new SerializedObject(blackjack);
        
        so.FindProperty("uiPanel").objectReferenceValue = uiPanel;
        so.FindProperty("playerCardsText").objectReferenceValue = playerCardsText.GetComponent<TMP_Text>();
        so.FindProperty("dealerCardsText").objectReferenceValue = dealerCardsText.GetComponent<TMP_Text>();
        so.FindProperty("playerScoreText").objectReferenceValue = playerScoreText.GetComponent<TMP_Text>();
        so.FindProperty("dealerScoreText").objectReferenceValue = dealerScoreText.GetComponent<TMP_Text>();
        so.FindProperty("messageText").objectReferenceValue = messageText.GetComponent<TMP_Text>();
        so.FindProperty("betAmountText").objectReferenceValue = betText.GetComponent<TMP_Text>();
        so.FindProperty("hitButton").objectReferenceValue = hitBtn.GetComponent<Button>();
        so.FindProperty("standButton").objectReferenceValue = standBtn.GetComponent<Button>();
        so.FindProperty("betIncreaseButton").objectReferenceValue = betIncBtn.GetComponent<Button>();
        so.FindProperty("betDecreaseButton").objectReferenceValue = betDecBtn.GetComponent<Button>();
        so.FindProperty("startGameButton").objectReferenceValue = startBtn.GetComponent<Button>();
        so.FindProperty("bettingPanel").objectReferenceValue = bettingPanel;
        so.FindProperty("gamePanel").objectReferenceValue = gamePanel;
        
        so.ApplyModifiedProperties();
        
        EditorUtility.DisplayDialog("Success!", "Blackjack UI created successfully!\n\nAll references have been assigned to BlackjackMinigame component.", "OK");
        Debug.Log("[BlackjackUIGenerator] UI created successfully for " + parent.name);
    }
    
    GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        return obj;
    }
    
    GameObject CreatePanel(string name, Transform parent, Vector2 size)
    {
        GameObject panel = CreateUIObject(name, parent);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;
        
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        
        return panel;
    }
    
    GameObject CreateText(string name, Transform parent, string text, int fontSize, Vector2 position, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        GameObject textObj = CreateUIObject(name, parent);
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(280, 30);
        
        TMP_Text tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        
        return textObj;
    }
    
    GameObject CreateButton(string name, Transform parent, string buttonText, Vector2 position)
    {
        GameObject btnObj = CreateUIObject(name, parent);
        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(60, 30);
        
        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.6f, 0.2f, 1f);
        
        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.2f, 0.6f, 0.2f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.8f, 0.3f, 1f);
        colors.pressedColor = new Color(0.15f, 0.5f, 0.15f, 1f);
        btn.colors = colors;
        
        GameObject textObj = CreateUIObject("Text", btnObj.transform);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        TMP_Text tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = buttonText;
        tmp.fontSize = 14;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        return btnObj;
    }
}
