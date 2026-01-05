using UnityEngine;

/// <summary>
/// Starts a Minigame when a Player enters this object's trigger collider.
/// Optionally ends the minigame when the player exits.
/// Attach to an object that has a Collider with `isTrigger = true` and a child
/// GameObject containing a `MinigameBase` (e.g. `QuizMinigame`).
/// </summary>
[RequireComponent(typeof(Collider))]
public class MinigameTriggerActivator : MonoBehaviour
{
    [Tooltip("End the minigame automatically when the player leaves the trigger")]
    [SerializeField] private bool endOnExit = true;
    [Tooltip("Create a visible semi-transparent cube in Play mode to show the trigger area")]
    [SerializeField] private bool showVisualInGame = true;

    [Tooltip("Color of the runtime visualizer")]
    [SerializeField] private Color visualColor = new Color(0f, 0.5f, 1f, 0.25f);

    private MinigameBase _minigame;

    private void Awake()
    {
        _minigame = GetComponentInChildren<MinigameBase>(true);
        if (_minigame == null)
        {
            Debug.LogWarning($"[MinigameTriggerActivator] No MinigameBase found on '{name}' or its children.");
            return;
        }
        _minigame.Initialize(this);

        var col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"[MinigameTriggerActivator] Collider on '{name}' is not a trigger. Set isTrigger = true to detect players by entry.");
        }

        // Create a simple runtime visual so the trigger is visible in Game view
        if (showVisualInGame && col != null && Application.isPlaying)
        {
            CreateRuntimeVisualizer(col);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_minigame == null) return;
        var player = other.GetComponentInParent<Player>();
        if (player == null) return;

        // Debug: ensure one side has a Rigidbody else triggers may not fire
        var rb = player.GetComponentInParent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning($"[MinigameTriggerActivator] Player '{player.name}' has no Rigidbody. Add a Rigidbody to the Player (non-kinematic) so OnTriggerEnter works reliably.");
        }

        if (!_minigame.IsRunning)
        {
            _minigame.StartGame(player);
            Debug.Log($"[MinigameTriggerActivator] Started minigame '{_minigame.name}' for player '{player.name}'");
        }
    }

    private void CreateRuntimeVisualizer(Collider col)
    {
        var vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        vis.name = "TriggerVisual_Runtime";
        vis.hideFlags = HideFlags.DontSave;
        vis.transform.SetParent(transform, false);
        // match collider bounds
        vis.transform.localPosition = col.bounds.center - transform.position;
        vis.transform.localRotation = Quaternion.identity;
        vis.transform.localScale = col.bounds.size;

        // create transparent material
        var rend = vis.GetComponent<MeshRenderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat == null)
            mat = new Material(Shader.Find("Standard"));
        mat.color = visualColor;
        rend.sharedMaterial = mat;

        // Make collider trigger not interact with visual
        var vcol = vis.GetComponent<Collider>();
        if (vcol != null) Destroy(vcol);
    }

    private void OnTriggerExit(Collider other)
    {
        if (_minigame == null)
        {
            Debug.LogWarning($"[MinigameTriggerActivator] TriggerEnter on '{name}' but no minigame assigned.");
            return;
        }

        Debug.Log($"[MinigameTriggerActivator] OnTriggerEnter: other={other.name}, layer={LayerMask.LayerToName(other.gameObject.layer)}");

        var player = other.GetComponentInParent<Player>();
        if (player == null)
        {
            Debug.Log("[MinigameTriggerActivator] OnTriggerEnter: collider has no Player in parent chain.");
            return;
        }

        if (!_minigame.IsRunning)
        {
            _minigame.StartGame(player);
            Debug.Log($"[MinigameTriggerActivator] Started minigame '{_minigame.name}' for player '{player.name}'");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Fallback in case OnTriggerEnter is missed by physics setup.
        if (_minigame == null) return;
        if (_minigame.IsRunning) return;

        var player = other.GetComponentInParent<Player>();
        if (player == null) return;

        Debug.Log($"[MinigameTriggerActivator] OnTriggerStay detected player '{player.name}', attempting to start minigame.");
        _minigame.StartGame(player);
    }
}
