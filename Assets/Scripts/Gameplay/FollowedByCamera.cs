using UnityEngine;

public class FollowedByCamera : MonoBehaviour
{
    [Header("Camera Setup")]
    [Tooltip("Offset from the followed object (Z should stay -10 for default 2D).")]
    public Vector3 cameraOffset = new Vector3(0f, 0f, -10f);
    [Tooltip("Orthographic size of the camera (controls zoom level).")]
    public float orthographicSize = 5f;
    [Tooltip("If true the camera becomes a child and uses the offset relative to this object.")]
    public bool parentCameraToTarget = true;

    [Header("Follow Behaviour")]
    [Tooltip("Enable smooth damp movement instead of hard snapping.")]
    public bool smoothFollow = true;
    [Tooltip("Smoothing factor (higher = snappier).")]
    [Range(0.1f, 20f)] public float followSpeed = 8f;

    private Camera _cam;
    private Vector3 _velocity; // for SmoothDamp if needed

    void Awake()
    {
        // Try to find an existing camera already attached/child
        _cam = GetComponentInChildren<Camera>();
    }

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

    // Draw a gizmo to visualize camera offset in editor (safe at runtime, just skipped)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 worldPos = Application.isPlaying && !parentCameraToTarget ? transform.position + cameraOffset : transform.TransformPoint(cameraOffset);
        Gizmos.DrawWireSphere(worldPos, 0.25f);
        Gizmos.DrawLine(transform.position, worldPos);
    }
}
