using UnityEngine;

/// <summary>
/// Automatically creates and manages a camera that follows this GameObject.
/// Supports parented or smooth-following modes for top-down 2D gameplay.
/// Attached to <see cref="Player"/> to provide dynamic camera behavior.
/// </summary>
public class FollowedByCamera : MonoBehaviour
{
    /// <summary>
    /// Offset from the followed object (Z should stay -10 for default 2D).
    /// Set this in the Unity Inspector.
    /// </summary>
    [Header("Camera Setup")]
    [Tooltip("Offset from the followed object (Z should stay -10 for default 2D).")]
    public Vector3 cameraOffset = new Vector3(0f, 0f, -10f);
    
    /// <summary>
    /// Orthographic size of the camera (controls zoom level).
    /// Set this in the Unity Inspector.
    /// </summary>
    [Tooltip("Orthographic size of the camera (controls zoom level).")]
    public float orthographicSize = 5f;
    
    /// <summary>
    /// If true, the camera becomes a child and uses the offset relative to this object.
    /// Set this in the Unity Inspector.
    /// </summary>
    [Tooltip("If true the camera becomes a child and uses the offset relative to this object.")]
    public bool parentCameraToTarget = true;

    /// <summary>
    /// Enable smooth damp movement instead of hard snapping.
    /// Set this in the Unity Inspector.
    /// </summary>
    [Header("Follow Behaviour")]
    [Tooltip("Enable smooth damp movement instead of hard snapping.")]
    public bool smoothFollow = true;
    
    /// <summary>
    /// Smoothing factor (higher = snappier).
    /// Set this in the Unity Inspector.
    /// </summary>
    [Tooltip("Smoothing factor (higher = snappier).")]
    [Range(0.1f, 20f)] public float followSpeed = 8f;

    /// <summary>Reference to the managed camera instance.</summary>
    private Camera _cam;
    
    /// <summary>Velocity vector for SmoothDamp if needed.</summary>
    private Vector3 _velocity;

    /// <summary>
    /// Tries to find an existing camera already attached as a child.
    /// </summary>
    void Awake()
    {
        // Try to find an existing camera already attached/child
        _cam = GetComponentInChildren<Camera>();
    }

    /// <summary>
    /// Creates and configures the camera for 2D top-down view if not already present.
    /// </summary>
    void Start()
    {
        if (_cam == null)
        {
            // Create a new camera GameObject
            GameObject camGO = new GameObject(name + "_Camera");
            if (parentCameraToTarget)
            {
                camGO.transform.SetParent(transform, false);
            }
            _cam = camGO.AddComponent<Camera>();
        }

        // Configure camera for 2D top-down
        _cam.orthographic = true;
        _cam.orthographicSize = orthographicSize;
        _cam.clearFlags = CameraClearFlags.Skybox; // user can adjust later
        _cam.transform.rotation = Quaternion.identity; // looking along -Z for 2D

        // Initial placement
        if (parentCameraToTarget)
        {
            _cam.transform.localPosition = cameraOffset;
        }
        else
        {
            _cam.transform.position = transform.position + cameraOffset;
        }
    }

    /// <summary>
    /// Updates camera position each frame based on parenting mode and smooth follow settings.
    /// </summary>
    void LateUpdate()
    {
        if (_cam == null) return;

        if (parentCameraToTarget)
        {
            // If parented, only ensure local offset stays consistent
            if (_cam.transform.localPosition != cameraOffset)
                _cam.transform.localPosition = cameraOffset;
        }
        else
        {
            Vector3 targetPos = transform.position + cameraOffset;
            if (smoothFollow)
            {
                // Exponential smoothing via Lerp
                _cam.transform.position = Vector3.Lerp(_cam.transform.position, targetPos, followSpeed * Time.deltaTime);
            }
            else
            {
                _cam.transform.position = targetPos;
            }
        }
    }

    /// <summary>
    /// Draws a gizmo to visualize the camera offset in the editor.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 worldPos = Application.isPlaying && !parentCameraToTarget ? transform.position + cameraOffset : transform.TransformPoint(cameraOffset);
        Gizmos.DrawWireSphere(worldPos, 0.25f);
        Gizmos.DrawLine(transform.position, worldPos);
    }
}
