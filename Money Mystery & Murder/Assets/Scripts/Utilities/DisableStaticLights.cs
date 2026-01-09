using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Disables any Light2D components on static objects (like shops) to prevent them from being visible at night.
/// Static objects should only be visible when illuminated by player lights.
/// </summary>
public class DisableStaticLights : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] [Tooltip("Disable all Light2D on this object on Start")]
    private bool disableLightsOnStart = true;
    
    void Start()
    {
        if (disableLightsOnStart)
        {
            DisableAllLights();
        }
    }
    
    [ContextMenu("Disable All Lights")]
    public void DisableAllLights()
    {
        Light2D[] lights = GetComponentsInChildren<Light2D>(true);
        
        foreach (Light2D light in lights)
        {
            light.enabled = false;
            Debug.Log($"[DisableStaticLights] Disabled Light2D on {light.gameObject.name}");
        }
        
        if (lights.Length == 0)
        {
            Debug.LogWarning($"[DisableStaticLights] No Light2D found on {gameObject.name}");
        }
    }
}
