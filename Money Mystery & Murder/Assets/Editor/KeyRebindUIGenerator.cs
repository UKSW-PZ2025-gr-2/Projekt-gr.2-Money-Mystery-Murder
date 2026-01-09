using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Editor tool to create key rebinding UI in Settings Scene.
/// Menu: Tools > Create Key Rebind UI
/// </summary>
public class KeyRebindUIGenerator : EditorWindow
{
    [MenuItem("Tools/Create Key Rebind UI")]
    static void Init()
    {
        KeyRebindUIGenerator window = (KeyRebindUIGenerator)EditorWindow.GetWindow(typeof(KeyRebindUIGenerator));
        window.titleContent = new GUIContent("Key Rebind UI Generator");
        window.Show();
    }
    
    void OnGUI()
    {
        GUILayout.Label("Key Rebind UI Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("This will create a complete key rebinding UI");
        GUILayout.Label("in the current scene (Settings Scene).");
        GUILayout.Space(10);
        
        if (GUILayout.Button("Generate Key Rebind UI", GUILayout.Height(40)))
        {
            GenerateUI();
        }
    }
    
    void GenerateUI()
    {
        // Ensure EventSystem exists
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            Debug.Log("[KeyRebindUIGenerator] Created EventSystem");
        }
        
        // Create or find Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        GameObject canvasObj;
        
        if (canvas == null)
        {
            canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        else
        {
            canvasObj = canvas.gameObject;
        }
        
        // Main panel (scalable with screen size)
        GameObject mainPanel = new GameObject("KeyRebindPanel");
        mainPanel.transform.SetParent(canvasObj.transform, false);
        mainPanel.SetActive(false); // Hidden by default
        
        RectTransform mainRect = mainPanel.AddComponent<RectTransform>();
        // Anchor to stretch with screen
        mainRect.anchorMin = new Vector2(0.5f, 0.5f);
        mainRect.anchorMax = new Vector2(0.5f, 0.5f);
        mainRect.pivot = new Vector2(0.5f, 0.5f);
        mainRect.anchoredPosition = Vector2.zero;
        // Wider and shorter
        mainRect.sizeDelta = new Vector2(800, 650);
        
        Image mainPanelImg = mainPanel.AddComponent<Image>();
        mainPanelImg.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
        
        // Title
        GameObject titleObj = CreateText("Title", mainPanel.transform, "STEROWANIE", 24, new Vector2(0, 285));
        
        // Scroll view for buttons
        GameObject scrollView = CreateScrollView("ScrollView", mainPanel.transform, new Vector2(750, 480), new Vector2(0, -30));
        Transform content = scrollView.transform.Find("Viewport/Content");
        
        // Listening panel (hidden by default)
        GameObject listeningPanel = CreatePanel("ListeningPanel", mainPanel.transform, new Vector2(350, 150));
        listeningPanel.SetActive(false);
        Image listeningImg = listeningPanel.GetComponent<Image>();
        listeningImg.color = new Color(0.2f, 0.2f, 0.2f, 0.98f);
        
        GameObject listeningText = CreateText("ListeningText", listeningPanel.transform, "Naciśnij klawisz...", 20, Vector2.zero);
        
        // Reset button
        GameObject resetBtn = CreateButton("ResetButton", mainPanel.transform, "RESETUJ", new Vector2(-120, -295));
        RectTransform resetRect = resetBtn.GetComponent<RectTransform>();
        resetRect.sizeDelta = new Vector2(150, 35);
        
        // Close button
        GameObject closeBtn = CreateButton("CloseButton", mainPanel.transform, "ZAMKNIJ", new Vector2(120, -295));
        RectTransform closeRect = closeBtn.GetComponent<RectTransform>();
        closeRect.sizeDelta = new Vector2(150, 35);
        Button closeBtnComponent = closeBtn.GetComponent<Button>();
        closeBtnComponent.onClick.AddListener(() => 
        {
            mainPanel.SetActive(false);
            // Re-show main settings panel
            var settingsPanel = Object.FindFirstObjectByType<SettingsPanel>();
            if (settingsPanel != null)
                settingsPanel.gameObject.SetActive(true);
        });
        // Create KeyBindings manager if doesn't exist
        if (Object.FindFirstObjectByType<KeyBindings>() == null)
        {
            GameObject kbObj = new GameObject("KeyBindings");
            kbObj.AddComponent<KeyBindings>();
            Debug.Log("[KeyRebindUIGenerator] Created KeyBindings GameObject");
        }
        
        // Add KeyRebindPanel component
        KeyRebindPanel rebindPanel = mainPanel.AddComponent<KeyRebindPanel>();
        SerializedObject so = new SerializedObject(rebindPanel);
        so.FindProperty("rebindButtonContainer").objectReferenceValue = content;
        so.FindProperty("listeningPanel").objectReferenceValue = listeningPanel;
        so.FindProperty("listeningText").objectReferenceValue = listeningText.GetComponent<TMP_Text>();
        so.FindProperty("resetButton").objectReferenceValue = resetBtn.GetComponent<Button>();
        so.ApplyModifiedProperties();
        
        EditorUtility.DisplayDialog("Success!", "Key Rebind UI created successfully!\n\nThe panel will automatically populate with all rebindable keys when you run the game.", "OK");
        Debug.Log("[KeyRebindUIGenerator] UI created successfully");
    }
    
    GameObject CreatePanel(string name, Transform parent, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;
        
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        
        return panel;
    }
    
    GameObject CreateText(string name, Transform parent, string text, int fontSize, Vector2 position)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(600, 50);
        
        TMP_Text tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        return textObj;
    }
    
    GameObject CreateButton(string name, Transform parent, string buttonText, Vector2 position)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        
        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(200, 40);
        
        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.3f, 0.5f, 0.8f, 1f);
        
        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.3f, 0.5f, 0.8f, 1f);
        colors.highlightedColor = new Color(0.4f, 0.6f, 0.9f, 1f);
        colors.pressedColor = new Color(0.2f, 0.4f, 0.7f, 1f);
        btn.colors = colors;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        TMP_Text tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = buttonText;
        tmp.fontSize = 16;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        return btnObj;
    }
    
    GameObject CreateScrollView(string name, Transform parent, Vector2 size, Vector2 position)
    {
        GameObject scrollView = new GameObject(name);
        scrollView.transform.SetParent(parent, false);
        
        RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0.5f, 0.5f);
        scrollRect.anchorMax = new Vector2(0.5f, 0.5f);
        scrollRect.sizeDelta = size;
        scrollRect.anchoredPosition = position;
        
        Image scrollImg = scrollView.AddComponent<Image>();
        scrollImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
        
        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.pivot = new Vector2(0.5f, 0.5f);
        
        Image viewportImg = viewport.AddComponent<Image>();
        viewportImg.color = Color.clear;
        
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        
        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 1000);
        contentRect.anchoredPosition = Vector2.zero;
        
        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.childControlHeight = false;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 8;
        vlg.padding = new RectOffset(10, 10, 10, 10);
        
        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Create Scrollbar
        GameObject scrollbar = new GameObject("Scrollbar");
        scrollbar.transform.SetParent(scrollView.transform, false);
        
        RectTransform scrollbarRect = scrollbar.AddComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(1, 0);
        scrollbarRect.anchorMax = new Vector2(1, 1);
        scrollbarRect.pivot = new Vector2(1, 1);
        scrollbarRect.sizeDelta = new Vector2(20, 0);
        scrollbarRect.anchoredPosition = Vector2.zero;
        
        Image scrollbarBg = scrollbar.AddComponent<Image>();
        scrollbarBg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        Scrollbar scrollbarComponent = scrollbar.AddComponent<Scrollbar>();
        scrollbarComponent.direction = Scrollbar.Direction.BottomToTop;
        
        // Scrollbar Handle
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(scrollbar.transform, false);
        
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.anchorMin = Vector2.zero;
        handleRect.anchorMax = Vector2.one;
        handleRect.sizeDelta = Vector2.zero;
        handleRect.anchoredPosition = Vector2.zero;
        
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        
        scrollbarComponent.handleRect = handleRect;
        scrollbarComponent.targetGraphic = handleImg;
        
        scroll.content = contentRect;
        scroll.viewport = viewportRect;
        scroll.verticalScrollbar = scrollbarComponent;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        
        return scrollView;
    }
}
