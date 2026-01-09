using UnityEngine;

/// <summary>
/// Ensures camera shows complete blackness where there's no light (Among Us style).
/// Sets camera background to black so nothing is visible outside light range.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraBlackoutSetup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] [Tooltip("Automatically configure camera on Start")]
    private bool autoConfigureOnStart = true;
    
    private Camera _camera;
    
    void Start()
    {
        if (autoConfigureOnStart)
        {
            ConfigureCamera();
        }
    }
    
    [ContextMenu("Configure Camera for Complete Blackout")]
    public void ConfigureCamera()
    {
        _camera = GetComponent<Camera>();
        
        if (_camera == null)
        {
            Debug.LogError("[CameraBlackoutSetup] No Camera component found!");
            return;
        }
        
        // Set background to solid black
        _camera.clearFlags = CameraClearFlags.SolidColor;
        _camera.backgroundColor = Color.black;
        
        Debug.Log($"[CameraBlackoutSetup] Configured camera {gameObject.name} for complete blackout");
    }
}
