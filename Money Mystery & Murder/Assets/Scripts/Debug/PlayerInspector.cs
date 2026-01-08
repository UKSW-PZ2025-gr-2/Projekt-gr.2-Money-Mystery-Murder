using UnityEngine;

/// <summary>
/// Runtime helper that logs all Collider2D objects in the scene and whether they
/// have a `Player` component, their layer, Rigidbody2D presence and IsActive/IsAlive.
/// Attach to any active GameObject and enable `autoRunOnPlay` to run on Start.
/// </summary>
public class PlayerInspector : MonoBehaviour
{
    [Tooltip("Automatically run the inspection when Play starts")] public bool autoRunOnPlay = true;
    [Tooltip("Only log colliders on these layers (set to All to inspect everything)")] public LayerMask inspectLayers = ~0;

    private void Start()
    {
        if (autoRunOnPlay) LogAllColliders();
    }

    [ContextMenu("Log All Colliders")]
    public void LogAllColliders()
    {
        Collider2D[] colliders = Object.FindObjectsOfType<Collider2D>();
        Debug.Log($"[PlayerInspector] Found {colliders.Length} Collider2D objects (filter layers={inspectLayers})");

        foreach (var col in colliders)
        {
            if (((1 << col.gameObject.layer) & inspectLayers.value) == 0) continue;

            var go = col.gameObject;
            var player = go.GetComponent<Player>() ?? go.GetComponentInParent<Player>() ?? go.GetComponentInChildren<Player>();
            var rb = go.GetComponent<Rigidbody2D>() ?? go.GetComponentInParent<Rigidbody2D>() ?? go.GetComponentInChildren<Rigidbody2D>();

            string playerInfo = player != null ? $"Player component present (name={player.gameObject.name}, IsAlive={player.IsAlive}, enabled={player.enabled})" : "No Player component";
            string rbInfo = rb != null ? $"Rigidbody2D present (bodyType={rb.bodyType})" : "No Rigidbody2D";

            Debug.Log($"[PlayerInspector] Collider='{go.name}' layer='{LayerMask.LayerToName(go.layer)}' {playerInfo}; {rbInfo}; activeInHierarchy={go.activeInHierarchy}");
        }
    }
}
