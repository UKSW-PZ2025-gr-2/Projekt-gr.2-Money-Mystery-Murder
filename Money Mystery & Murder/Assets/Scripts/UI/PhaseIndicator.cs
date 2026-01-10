using UnityEngine;
using TMPro;

/// <summary>
/// Visual indicator that shows whether an interaction is available based on the current game phase.
/// Can be attached to minigames, shops, or other phase-restricted objects.
/// </summary>
public class PhaseIndicator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GamePhase requiredPhase = GamePhase.Day;
    [SerializeField] private float updateInterval = 0.5f;
    [SerializeField] private float heightOffset = 2f;

    [Header("Display")]
    [SerializeField] private string availableText = "Available";
    [SerializeField] private string unavailableText = "Closed";
    [SerializeField] private Color availableColor = Color.green;
    [SerializeField] private Color unavailableColor = Color.red;
    [SerializeField] private int fontSize = 32;

    private TextMesh statusText;
    private float updateTimer;
    private bool isAvailable;

    void Start()
    {
        CreateStatusText();
        UpdateStatus();
    }

    void Update()
    {
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            UpdateStatus();
        }
    }

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

    public void SetRequiredPhase(GamePhase phase)
    {
        requiredPhase = phase;
        UpdateStatus();
    }
}
