using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Editor utility to quickly create a complete Hotbar UI setup.
/// Use: Tools -> Create Hotbar UI
/// </summary>
public class HotbarUICreator
{
    [MenuItem("Tools/Create Hotbar UI")]
    public static void CreateHotbarUI()
    {
        // Try to get Player from selected GameObject first
        Player player = null;
        
        if (Selection.activeGameObject != null)
        {
            player = Selection.activeGameObject.GetComponent<Player>();
            if (player != null)
            {
                Debug.Log($"[HotbarUICreator] Using selected Player: {player.gameObject.name}");
            }
        }
        
        // If no player selected, show error
        if (player == null)
        {
            Debug.LogError("[HotbarUICreator] Please select the Player GameObject in the Hierarchy first!");
            EditorUtility.DisplayDialog(
                "No Player Selected", 
                "Please select the Player GameObject in the Hierarchy before creating hotbar.", 
                "OK"
            );
            return;
        }
        
        // Clean up old hotbars
        var oldHotbars = player.GetComponentsInChildren<HotbarManager>();
        foreach (var old in oldHotbars)
        {
            Object.DestroyImmediate(old.gameObject);
        }
        
        var oldCanvases = player.GetComponentsInChildren<Canvas>();
        foreach (var oldCanvas in oldCanvases)
        {
            if (oldCanvas.name == "HotbarCanvas")
            {
                Object.DestroyImmediate(oldCanvas.gameObject);
            }
        }
        
        Debug.Log("[HotbarUICreator] Cleaned up old hotbars");
        
        // Create Canvas as child of Player
        GameObject canvasObj = new GameObject("HotbarCanvas");
        canvasObj.transform.SetParent(player.transform, false);
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(6.5f, 0.8f);
        canvasRect.localPosition = new Vector3(0, -1.0f, 0);
        
        // Create Hotbar Panel
        GameObject panelObj = new GameObject("HotbarPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(6.2f, 0.7f);
        
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f);
        
        HorizontalLayoutGroup layout = panelObj.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(0, 0, 3, 3);
        layout.spacing = 0.05f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        
        // Create 9 slots
        HotbarSlot[] slots = new HotbarSlot[9];
        for (int i = 0; i < 9; i++)
        {
            GameObject slotObj = CreateHotbarSlot(panelObj.transform, i + 1);
            slots[i] = slotObj.GetComponent<HotbarSlot>();
        }
        
        // Create HotbarManager as child of Player
        GameObject managerObj = new GameObject("HotbarManager");
        managerObj.transform.SetParent(player.transform, false);
        HotbarManager manager = managerObj.AddComponent<HotbarManager>();
        
        // Use reflection to set private fields
        var managerType = typeof(HotbarManager);
        var panelField = managerType.GetField("hotbarPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var slotsField = managerType.GetField("hotbarSlots", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var canvasField = managerType.GetField("hotbarCanvas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (panelField != null)
            panelField.SetValue(manager, panelObj);
        if (slotsField != null)
            slotsField.SetValue(manager, slots);
        if (canvasField != null)
            canvasField.SetValue(manager, canvas);
        
        Debug.Log("[HotbarUICreator] Hotbar UI created successfully for Player!");
        Debug.Log("Canvas: " + canvasObj.name + ", Panel: " + panelObj.name + ", Manager: " + managerObj.name);
        Debug.Log("[HotbarUICreator] Note: Icons will appear when you acquire weapons/abilities. Make sure WeaponData and Ability have icons set!");
        
        Selection.activeGameObject = managerObj;
    }
    
    private static GameObject CreateHotbarSlot(Transform parent, int number)
    {
        // Main slot container
        GameObject slotObj = new GameObject($"HotbarSlot_{number}");
        slotObj.transform.SetParent(parent, false);
        
        RectTransform slotRect = slotObj.AddComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(0.65f, 0.65f);
        
        Image slotImage = slotObj.AddComponent<Image>();
        slotImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        HotbarSlot slotScript = slotObj.AddComponent<HotbarSlot>();
        
        // Item Icon
        GameObject iconObj = new GameObject("ItemIcon");
        iconObj.transform.SetParent(slotObj.transform, false);
        
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = new Vector2(0.05f, 0.15f);
        iconRect.offsetMax = new Vector2(-0.05f, -0.05f);
        
        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.color = Color.white;
        iconImage.raycastTarget = false;
        iconImage.preserveAspect = true;
        iconImage.enabled = false;
        
        // Empty Slot Overlay (na dole)
        GameObject emptyObj = new GameObject("EmptySlotOverlay");
        emptyObj.transform.SetParent(slotObj.transform, false);
        emptyObj.transform.SetSiblingIndex(1); // Po ikonie
        
        RectTransform emptyRect = emptyObj.AddComponent<RectTransform>();
        emptyRect.anchorMin = Vector2.zero;
        emptyRect.anchorMax = Vector2.one;
        emptyRect.offsetMin = Vector2.zero;
        emptyRect.offsetMax = Vector2.zero;
        
        Image emptyImage = emptyObj.AddComponent<Image>();
        emptyImage.color = new Color(0, 0, 0, 0.5f);
        emptyImage.raycastTarget = false;
        
        // Cooldown Overlay
        GameObject cooldownObj = new GameObject("CooldownOverlay");
        cooldownObj.transform.SetParent(slotObj.transform, false);
        cooldownObj.transform.SetSiblingIndex(2); // Po empty overlay
        
        RectTransform cooldownRect = cooldownObj.AddComponent<RectTransform>();
        cooldownRect.anchorMin = Vector2.zero;
        cooldownRect.anchorMax = Vector2.one;
        cooldownRect.offsetMin = Vector2.zero;
        cooldownRect.offsetMax = Vector2.zero;
        
        Image cooldownImage = cooldownObj.AddComponent<Image>();
        cooldownImage.color = new Color(0, 0, 0, 0.6f);
        cooldownImage.raycastTarget = false;
        cooldownImage.type = Image.Type.Filled;
        cooldownImage.fillMethod = Image.FillMethod.Radial360;
        cooldownImage.fillOrigin = (int)Image.Origin360.Top;
        cooldownImage.fillAmount = 0f;
        
        // Cooldown Text
        GameObject cooldownTextObj = new GameObject("CooldownText");
        cooldownTextObj.transform.SetParent(slotObj.transform, false);
        cooldownTextObj.transform.SetSiblingIndex(3); // Po cooldown overlay
        
        RectTransform cooldownTextRect = cooldownTextObj.AddComponent<RectTransform>();
        cooldownTextRect.anchorMin = new Vector2(0.5f, 0.5f);
        cooldownTextRect.anchorMax = new Vector2(0.5f, 0.5f);
        cooldownTextRect.pivot = new Vector2(0.5f, 0.5f);
        cooldownTextRect.sizeDelta = new Vector2(0.25f, 0.12f);
        
        TextMeshProUGUI cooldownText = cooldownTextObj.AddComponent<TextMeshProUGUI>();
        cooldownText.text = "";
        cooldownText.fontSize = 0.28f;
        cooldownText.enableAutoSizing = false;
        cooldownText.fontStyle = FontStyles.Bold;
        cooldownText.alignment = TextAlignmentOptions.Center;
        cooldownText.color = Color.white;
        
        // Key Number Text (na samej górze - na końcu)
        GameObject keyTextObj = new GameObject("KeyNumberText");
        keyTextObj.transform.SetParent(slotObj.transform, false);
        keyTextObj.transform.SetSiblingIndex(4); // Ostatni - na wierzchu wszystkiego
        
        RectTransform keyTextRect = keyTextObj.AddComponent<RectTransform>();
        keyTextRect.anchorMin = new Vector2(0.5f, 0f);
        keyTextRect.anchorMax = new Vector2(0.5f, 0f);
        keyTextRect.pivot = new Vector2(0.5f, 0f);
        keyTextRect.anchoredPosition = new Vector2(0, 0.02f);
        keyTextRect.sizeDelta = new Vector2(0.4f, 0.12f);
        
        TextMeshProUGUI keyText = keyTextObj.AddComponent<TextMeshProUGUI>();
        keyText.text = number.ToString();
        keyText.fontSize = 0.18f;
        keyText.enableAutoSizing = false;
        keyText.alignment = TextAlignmentOptions.Center;
        keyText.color = Color.white;
        
        // Wire up references using reflection
        var slotType = typeof(HotbarSlot);
        var iconField = slotType.GetField("itemIcon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var keyTextField = slotType.GetField("keyNumberText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var emptyField = slotType.GetField("emptySlotOverlay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var cooldownOverlayField = slotType.GetField("cooldownOverlay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var cooldownTextField = slotType.GetField("cooldownText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (iconField != null)
            iconField.SetValue(slotScript, iconImage);
        if (keyTextField != null)
            keyTextField.SetValue(slotScript, keyText);
        if (emptyField != null)
            emptyField.SetValue(slotScript, emptyObj);
        if (cooldownOverlayField != null)
            cooldownOverlayField.SetValue(slotScript, cooldownImage);
        if (cooldownTextField != null)
            cooldownTextField.SetValue(slotScript, cooldownText);
        
        return slotObj;
    }
}
#endif
