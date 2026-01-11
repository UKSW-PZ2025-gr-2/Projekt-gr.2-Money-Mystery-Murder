using UnityEngine;
using TMPro;

/// <summary>
/// Visual indicator that shows whether an interaction is available based on the current game phase.
/// Can be attached to minigames, shops, or other phase-restricted objects.
/// </summary>
public class PhaseIndicator : MonoBehaviour
{
    [Header("Settings")]
    /// <summary>
    /// The game phase required for this interaction to be available.
    /// </summary>
    [SerializeField] private GamePhase requiredPhase = GamePhase.Day;
    
    /// <summary>
    /// How often to check and update the status display (in seconds).
    /// </summary>
    [SerializeField] private float updateInterval = 0.5f;
    
    /// <summary>
    /// Vertical offset above the object for the status text display.
    /// </summary>
    [SerializeField] private float heightOffset = 2f;

    [Header("Display")]
    /// <summary>
    /// Text displayed when the interaction is available.
    /// </summary>
    [SerializeField] private string availableText = "Available";
    
    /// <summary>
    /// Text displayed when the interaction is unavailable.
    /// </summary>
    [SerializeField] private string unavailableText = "Closed";
    
    /// <summary>
    /// Color of the status text when available.
    /// </summary>
    [SerializeField] private Color availableColor = Color.green;
    
    /// <summary>
    /// Color of the status text when unavailable.
    /// </summary>
    [SerializeField] private Color unavailableColor = Color.red;
    
    /// <summary>
    /// Font size of the status text.
    /// </summary>
    [SerializeField] private int fontSize = 32;

    /// <summary>
    /// The TextMesh component used to display the status.
    /// </summary>
    private TextMesh statusText;
    
    /// <summary>
    /// Timer for tracking update intervals.
    /// </summary>
    private float updateTimer;
    
    /// <summary>
    /// Tracks whether the interaction is currently available.
    /// </summary>
    private bool isAvailable;

    /// <summary>
    /// Unity lifecycle method called before the first frame update.
    /// Creates the status text display and performs initial status update.
    /// </summary>
    void Start()
    {
        CreateStatusText();
        UpdateStatus();
    }

    /// <summary>
    /// Unity lifecycle method called once per frame.
    /// Periodically updates the status based on the update interval.
    /// </summary>
    void Update()
    {
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            UpdateStatus();
        }
    }

    /// <summary>
    /// Creates the TextMesh GameObject for displaying phase status.
    /// </summary>
    private void CreateStatusText()
    {
        GameObject textObj = new GameObject("PhaseStatusText");
        textObj.transform.SetParent(transform, false);
        textObj.transform.localPosition = new Vector3(0, heightOffset, 0);

        statusText = textObj.AddComponent<TextMesh>();
        statusText.fontSize = fontSize;
        statusText.characterSize = 0.1f;
        statusText.alignment = TextAlignment.Center;
        statusText.anchor = TextAnchor.MiddleCenter;
    }

    /// <summary>
    /// Updates the status display based on the current game phase.
    /// </summary>
    private void UpdateStatus()
    {
        if (GameManager.Instance == null)
        {
            isAvailable = true;
        }
        else
        {
            isAvailable = GameManager.Instance.CurrentPhase == requiredPhase;
        }

        if (statusText != null)
        {
            statusText.text = isAvailable ? availableText : unavailableText;
            statusText.color = isAvailable ? availableColor : unavailableColor;
        }
    }

    /// <summary>
    /// Sets the required phase for this interaction and updates the display.
    /// </summary>
    /// <param name="phase">The game phase to require for availability.</param>
    public void SetRequiredPhase(GamePhase phase)
    {
        requiredPhase = phase;
        UpdateStatus();
    }
}
