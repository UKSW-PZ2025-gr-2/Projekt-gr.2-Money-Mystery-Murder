using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Editor helper to quickly configure night vision system in the scene.
/// </summary>
public class NightVisionQuickSetup : MonoBehaviour
{
    [Header("Quick Setup")]
    [SerializeField] [Tooltip("Click to automatically configure global light for complete darkness at night")]
    private bool setupGlobalLight = false;
    
    [SerializeField] [Tooltip("Reference to Global Light 2D (will find automatically if not set)")]
    private Light2D globalLight;
    
    void OnValidate()
    {
        if (setupGlobalLight)
        {
            setupGlobalLight = false;
            SetupGlobalLightForNight();
        }
    }
    
    [ContextMenu("Setup Global Light for Night Vision")]
    private void SetupGlobalLightForNight()
    {
        if (globalLight == null)
        {
            globalLight = FindGlobalLight();
        }
        
        if (globalLight == null)
        {
            Debug.LogError("[NightVisionQuickSetup] No Global Light 2D found in scene! Please add one first.");
            return;
        }
        
        // Add GlobalLightingController if not present
        GlobalLightingController controller = FindFirstObjectByType<GlobalLightingController>();
        
        if (controller == null)
        {
            GameObject go = new GameObject("GlobalLightingController");
            controller = go.AddComponent<GlobalLightingController>();
            Debug.Log("[NightVisionQuickSetup] Created GlobalLightingController");
        }
        
        Debug.Log($"[NightVisionQuickSetup] Global light configured! Make sure Night Light Intensity is set to 0.0 in GlobalLightingController");
    }
    
    [ContextMenu("Setup All Players for Night Vision")]
    private void SetupAllPlayers()
    {
        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);
        
        if (players.Length == 0)
        {
            Debug.LogWarning("[NightVisionQuickSetup] No players found in scene!");
            return;
        }
        
        foreach (Player player in players)
        {
            SetupPlayerNightVision(player.gameObject);
        }
        
        Debug.Log($"[NightVisionQuickSetup] Configured {players.Length} players for night vision");
    }
    
    [ContextMenu("Update All NightVisionControllers to New Defaults")]
    private void UpdateAllNightVisionControllers()
    {
        NightVisionController[] controllers = FindObjectsByType<NightVisionController>(FindObjectsSortMode.None);
        
        if (controllers.Length == 0)
        {
            Debug.LogWarning("[NightVisionQuickSetup] No NightVisionControllers found in scene!");
            return;
        }
        
        int updated = 0;
        foreach (NightVisionController controller in controllers)
        {
            // Force reset to code defaults using reflection
            var type = controller.GetType();
            
            // Update vision ranges (Among Us style - larger range)
            SetPrivateField(controller, "dayVisionRange", 25f);
            SetPrivateField(controller, "eveningVisionRange", 15f);
            SetPrivateField(controller, "nightVisionRange", 12f);
            
            // Update intensities (darker at night)
            SetPrivateField(controller, "dayIntensity", 1.5f);
            SetPrivateField(controller, "eveningIntensity", 1.0f);
            SetPrivateField(controller, "nightIntensity", 0.5f);
            
            // Update other settings
            SetPrivateField(controller, "transitionSpeed", 3f);
            SetPrivateField(controller, "innerRadiusMultiplier", 0.3f);
            SetPrivateField(controller, "useDistanceFalloff", true);
            
            updated++;
            Debug.Log($"[NightVisionQuickSetup] Updated {controller.gameObject.name} to new defaults");
        }
        
        Debug.Log($"[NightVisionQuickSetup] Updated {updated} NightVisionControllers to new default values!");
    }
    
    private void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            field.SetValue(obj, value);
        }
    }
    
    private void SetupPlayerNightVision(GameObject playerObj)
    {
        // Add Light2D if not present
        Light2D light = playerObj.GetComponent<Light2D>();
        if (light == null)
        {
            light = playerObj.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.pointLightOuterRadius = 15f;
            light.intensity = 1f;
            Debug.Log($"[NightVisionQuickSetup] Added Light2D to {playerObj.name}");
        }
        
        // Add NightVisionController if not present
        NightVisionController visionController = playerObj.GetComponent<NightVisionController>();
        if (visionController == null)
        {
            visionController = playerObj.AddComponent<NightVisionController>();
            Debug.Log($"[NightVisionQuickSetup] Added NightVisionController to {playerObj.name}");
        }
        
        // Add AutoShadowCaster2D if not present (optional but recommended)
        if (playerObj.GetComponent<SpriteRenderer>() != null)
        {
            AutoShadowCaster2D shadowCaster = playerObj.GetComponent<AutoShadowCaster2D>();
            if (shadowCaster == null)
            {
                shadowCaster = playerObj.AddComponent<AutoShadowCaster2D>();
                Debug.Log($"[NightVisionQuickSetup] Added AutoShadowCaster2D to {playerObj.name}");
            }
        }
    }
    
    private Light2D FindGlobalLight()
    {
        Light2D[] lights = FindObjectsByType<Light2D>(FindObjectsSortMode.None);
        
        foreach (Light2D light in lights)
        {
            if (light.lightType == Light2D.LightType.Global)
            {
                return light;
            }
        }
        
        return null;
    }
    
    [ContextMenu("Print Night Vision Status")]
    private void PrintStatus()
    {
        Debug.Log("===== NIGHT VISION SYSTEM STATUS =====");
        
        // Check Global Light
        Light2D global = FindGlobalLight();
        if (global != null)
        {
            Debug.Log($"✓ Global Light found: {global.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("✗ No Global Light 2D found!");
        }
        
        // Check GlobalLightingController
        GlobalLightingController controller = FindFirstObjectByType<GlobalLightingController>();
        if (controller != null)
        {
            Debug.Log("✓ GlobalLightingController found");
        }
        else
        {
            Debug.LogWarning("✗ GlobalLightingController not found!");
        }
        
        // Check Players
        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);
        Debug.Log($"Found {players.Length} players in scene");
        
        foreach (Player player in players)
        {
            bool hasLight = player.GetComponent<Light2D>() != null;
            bool hasController = player.GetComponent<NightVisionController>() != null;
            bool hasShadow = player.GetComponent<AutoShadowCaster2D>() != null;
            
            Debug.Log($"  Player {player.gameObject.name}: Light2D={hasLight}, NightVision={hasController}, Shadow={hasShadow}");
        }
        
        // Check all Light2D in scene
        Light2D[] allLights = FindObjectsByType<Light2D>(FindObjectsSortMode.None);
        Debug.Log($"Total Light2D components in scene: {allLights.Length}");
        
        foreach (Light2D light in allLights)
        {
            if (light.lightType != Light2D.LightType.Global)
            {
                bool hasNVC = light.GetComponent<NightVisionController>() != null;
                Debug.Log($"  Light on {light.gameObject.name}: Type={light.lightType}, Enabled={light.enabled}, HasNightVision={hasNVC}");
            }
        }
        
        Debug.Log("=====================================");
    }
    
    [ContextMenu("Disable All Non-Player Lights")]
    private void DisableNonPlayerLights()
    {
        Light2D[] allLights = FindObjectsByType<Light2D>(FindObjectsSortMode.None);
        int disabled = 0;
        
        foreach (Light2D light in allLights)
        {
            // Skip global lights and lights on players
            if (light.lightType == Light2D.LightType.Global) continue;
            if (light.GetComponent<Player>() != null) continue;
            if (light.GetComponentInParent<Player>() != null) continue;
            
            light.enabled = false;
            disabled++;
            Debug.Log($"[NightVisionQuickSetup] Disabled light on {light.gameObject.name}");
        }
        
        Debug.Log($"[NightVisionQuickSetup] Disabled {disabled} non-player lights");
    }
    
    [ContextMenu("Setup All Sprites to Use Lit Material")]
    private void SetupAllSpritesForLighting()
    {
        SpriteRenderer[] allSprites = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        int configured = 0;
        
        Shader litShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
        if (litShader == null)
        {
            Debug.LogError("[NightVisionQuickSetup] Sprite-Lit-Default shader not found! Make sure URP is configured.");
            return;
        }
        
        foreach (SpriteRenderer sprite in allSprites)
        {
            // Check if already using a lit shader (use sharedMaterial to avoid creating instances)
            if (sprite.sharedMaterial != null && sprite.sharedMaterial.shader.name.Contains("Sprite-Lit"))
            {
                continue; // Already configured
            }
            
            // Add LitSpriteSetup component if not present
            LitSpriteSetup litSetup = sprite.GetComponent<LitSpriteSetup>();
            if (litSetup == null)
            {
                litSetup = sprite.gameObject.AddComponent<LitSpriteSetup>();
            }
            
            // Setup the sprite
            litSetup.SetupLitSprite();
            configured++;
        }
        
        Debug.Log($"[NightVisionQuickSetup] Configured {configured} sprites to use Lit material");
    }
    
    [ContextMenu("Remove Lights from All Bots/NPCs")]
    private void RemoveLightsFromBots()
    {
        // Find all objects with "BOT" in name or without Player component
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int removed = 0;
        
        foreach (GameObject obj in allObjects)
        {
            // Skip if it's a player
            if (obj.GetComponent<Player>() != null) continue;
            
            // Check if it has Light2D
            Light2D[] lights = obj.GetComponents<Light2D>();
            foreach (Light2D light in lights)
            {
                if (light.lightType != Light2D.LightType.Global)
                {
                    DestroyImmediate(light);
                    removed++;
                    Debug.Log($"[NightVisionQuickSetup] Removed Light2D from {obj.name}");
                }
            }
        }
        
        Debug.Log($"[NightVisionQuickSetup] Removed {removed} lights from non-player objects");
    }
    
    [ContextMenu("FULL SETUP - Among Us Style Night Vision")]
    private void FullAmongUsSetup()
    {
        Debug.Log("===== STARTING AMONG US STYLE SETUP =====");
        
        // 1. Fix camera for complete blackout
        FixCamera();
        
        // 2. Setup global lighting
        SetupGlobalLightForNight();
        
        // 3. Update all night vision controllers
        UpdateAllNightVisionControllers();
        
        // 4. Setup all players
        SetupAllPlayers();
        
        // 5. Remove lights and NightVisionController from ALL bots
        RemoveAllBotsLighting();
        
        // 6. Setup all sprites for lighting
        SetupAllSpritesForLighting();
        
        // 7. Force sprites to respect lighting 100%
        ForceSpritesRespectLighting();
        
        // 8. Disable non-player lights
        DisableNonPlayerLights();
        
        Debug.Log("===== AMONG US STYLE SETUP COMPLETE =====");
        Debug.Log("Summary:");
        Debug.Log("- Camera configured for complete black background");
        Debug.Log("- Global light configured for complete darkness at night");
        Debug.Log("- Player has vision range of 12 units at night");
        Debug.Log("- ALL bots have NO lights (you only see your light)");
        Debug.Log("- All sprites use Lit material and respect lighting 100%");
        Debug.Log("- All non-player lights removed/disabled");
        Debug.Log("Test by changing time to 22:00 - NOTHING should be visible outside your light!");
    }
    
    [ContextMenu("Remove ALL Lighting from Bots (Aggressive)")]
    private void RemoveAllBotsLighting()
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int lightsRemoved = 0;
        int controllersRemoved = 0;
        
        foreach (GameObject obj in allObjects)
        {
            // Check if it's a bot (has "BOT" in name or doesn't have Player component as main player)
            bool isBot = obj.name.Contains("BOT");
            Player player = obj.GetComponent<Player>();
            
            // If it's clearly a bot
            if (isBot)
            {
                // FIRST: Remove NightVisionController (it depends on Light2D)
                NightVisionController controller = obj.GetComponent<NightVisionController>();
                if (controller != null)
                {
                    DestroyImmediate(controller);
                    controllersRemoved++;
                    Debug.Log($"[NightVisionQuickSetup] Removed NightVisionController from {obj.name}");
                }
                
                // THEN: Remove ALL Light2D components
                Light2D[] lights = obj.GetComponents<Light2D>();
                foreach (Light2D light in lights)
                {
                    if (light.lightType != Light2D.LightType.Global)
                    {
                        DestroyImmediate(light);
                        lightsRemoved++;
                        Debug.Log($"[NightVisionQuickSetup] Removed Light2D from {obj.name}");
                    }
                }
            }
        }
        
        Debug.Log($"[NightVisionQuickSetup] Removed {controllersRemoved} controllers and {lightsRemoved} lights from bots");
    }
    
    [ContextMenu("Verify Among Us Setup")]
    private void VerifySetup()
    {
        Debug.Log("===== VERIFYING AMONG US SETUP =====");
        
        // Check Global Light
        GlobalLightingController glc = FindFirstObjectByType<GlobalLightingController>();
        if (glc != null)
        {
            Debug.Log("✓ GlobalLightingController found - check that 'Force Complete Blackness' is enabled");
        }
        else
        {
            Debug.LogWarning("✗ GlobalLightingController not found!");
        }
        
        // Check if bots have lights
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int botsWithLights = 0;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("BOT"))
            {
                Light2D light = obj.GetComponent<Light2D>();
                if (light != null && light.lightType != Light2D.LightType.Global)
                {
                    botsWithLights++;
                    Debug.LogWarning($"✗ BOT {obj.name} still has Light2D!");
                }
            }
        }
        
        if (botsWithLights == 0)
        {
            Debug.Log("✓ No bots have lights");
        }
        
        // Check sprites
        SpriteRenderer[] allSprites = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        int spritesWithLit = 0;
        int spritesWithoutLit = 0;
        
        foreach (SpriteRenderer sprite in allSprites)
        {
            if (sprite.sharedMaterial != null && sprite.sharedMaterial.shader.name.Contains("Sprite-Lit"))
            {
                spritesWithLit++;
            }
            else
            {
                spritesWithoutLit++;
                Debug.LogWarning($"Sprite on {sprite.gameObject.name} not using Lit shader!");
            }
        }
        
        Debug.Log($"Sprites: {spritesWithLit} using Lit shader, {spritesWithoutLit} NOT using Lit shader");
        
        Debug.Log("===== VERIFICATION COMPLETE =====");
    }
    
    [ContextMenu("Fix Camera for Complete Darkness")]
    private void FixCamera()
    {
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        
        foreach (Camera cam in cameras)
        {
            // Add CameraBlackoutSetup if not present
            CameraBlackoutSetup blackout = cam.GetComponent<CameraBlackoutSetup>();
            if (blackout == null)
            {
                blackout = cam.gameObject.AddComponent<CameraBlackoutSetup>();
            }
            
            blackout.ConfigureCamera();
            Debug.Log($"[NightVisionQuickSetup] Configured camera {cam.gameObject.name}");
        }
    }
    
    [ContextMenu("Force Sprites to Respect Lighting 100%")]
    private void ForceSpritesRespectLighting()
    {
        SpriteRenderer[] allSprites = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        int fixedCount = 0;
        
        foreach (SpriteRenderer sprite in allSprites)
        {
            // Ensure using Lit shader
            if (sprite.sharedMaterial == null || !sprite.sharedMaterial.shader.name.Contains("Sprite-Lit"))
            {
                Shader litShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
                if (litShader != null)
                {
                    sprite.sharedMaterial = new Material(litShader);
                }
            }
            
            // DON'T force white color - keep original colors
            // Disable mask interaction
            sprite.maskInteraction = SpriteMaskInteraction.None;
            
            fixedCount++;
        }
        
        Debug.Log($"[NightVisionQuickSetup] Fixed {fixedCount} sprites to respect lighting (kept original colors)");
    }
    
    [ContextMenu("Debug: Why Can I See Black Silhouettes?")]
    private void DebugBlackSilhouettes()
    {
        Debug.Log("===== DEBUGGING BLACK SILHOUETTES =====");
        
        // Check camera
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Debug.Log($"Camera Clear Flags: {mainCam.clearFlags} (should be SolidColor)");
            Debug.Log($"Camera Background Color: {mainCam.backgroundColor} (should be black)");
            
            if (mainCam.clearFlags != CameraClearFlags.SolidColor)
            {
                Debug.LogWarning("⚠ Camera Clear Flags is NOT SolidColor! Run 'Fix Camera for Complete Darkness'");
            }
            if (mainCam.backgroundColor != Color.black)
            {
                Debug.LogWarning($"⚠ Camera Background is NOT black! It's {mainCam.backgroundColor}");
            }
        }
        
        // Check Global Light
        Light2D globalLight = FindGlobalLight();
        if (globalLight != null)
        {
            Debug.Log($"Global Light Intensity: {globalLight.intensity} (should be 0 at night)");
            Debug.Log($"Global Light Color: {globalLight.color}");
        }
        
        // Check for rogue lights
        Light2D[] allLights = FindObjectsByType<Light2D>(FindObjectsSortMode.None);
        int rogueLights = 0;
        foreach (Light2D light in allLights)
        {
            if (light.lightType != Light2D.LightType.Global && light.enabled)
            {
                GameObject obj = light.gameObject;
                if (!obj.name.Contains("Player") && !obj.GetComponent<Player>())
                {
                    Debug.LogWarning($"⚠ Found rogue light on {obj.name} - this may illuminate bots!");
                    rogueLights++;
                }
            }
        }
        
        if (rogueLights == 0)
        {
            Debug.Log("✓ No rogue lights found");
        }
        
        // Check sprite shaders
        SpriteRenderer[] sprites = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        int wrongShader = 0;
        foreach (SpriteRenderer sprite in sprites)
        {
            if (sprite.sharedMaterial == null || !sprite.sharedMaterial.shader.name.Contains("Sprite-Lit"))
            {
                Debug.LogWarning($"⚠ {sprite.gameObject.name} is NOT using Lit shader!");
                wrongShader++;
            }
        }
        
        if (wrongShader > 0)
        {
            Debug.LogWarning($"⚠ {wrongShader} sprites not using Lit shader - run 'Force Sprites to Respect Lighting 100%'");
        }
        else
        {
            Debug.Log("✓ All sprites using Lit shader");
        }
        
        Debug.Log("===== DIAGNOSIS COMPLETE =====");
        Debug.Log("If you still see black silhouettes:");
        Debug.Log("1. Run 'Fix Camera for Complete Darkness'");
        Debug.Log("2. Run 'Force Sprites to Respect Lighting 100%'");
        Debug.Log("3. Check that Global Light intensity is 0 during night phase");
    }
}
