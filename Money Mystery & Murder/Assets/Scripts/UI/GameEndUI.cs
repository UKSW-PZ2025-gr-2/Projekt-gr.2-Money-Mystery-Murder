using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// UI component that displays the game end screen with the winning team.
/// Shows a message with fade-in effect when the game ends.
/// </summary>
public class GameEndUI : MonoBehaviour
{
    // UI component for game end screen

    /// <summary>
    /// TextMeshPro text component for displaying the winner.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private TMP_Text winnerText;
    
    /// <summary>
    /// CanvasGroup for fading the entire UI panel.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private CanvasGroup canvasGroup;
    
    /// <summary>
    /// Duration in seconds for the fade-in transition.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private float fadeInDuration = 1.5f;
    
    /// <summary>
    /// Text to display when innocents win.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private string innocentsWinText = "INNOCENTS WIN!\nAll murderers have been eliminated.";
    
    /// <summary>
    /// Text to display when murderers win.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private string murderersWinText = "MURDERERS WIN!\nAll innocents have been eliminated.";
    
    /// <summary>
    /// Color for innocents win text.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private Color innocentsWinColor = new Color(0.2f, 0.8f, 0.2f);
    
    /// <summary>
    /// Color for murderers win text.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private Color murderersWinColor = new Color(0.8f, 0.2f, 0.2f);

    /// <summary>
    /// Initializes the UI and hides it by default. 
    /// </summary>
    void Awake()
    {
        if (winnerText == null)
        {
            winnerText = GetComponentInChildren<TMP_Text>(true);
        }
        
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        Hide();
    }

    /// <summary>
    /// Hides the game end UI immediately.
    /// </summary>
    public void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows the game end screen with the winning team.
    /// </summary>
    /// <param name="winningTeam">The name of the winning team ("Innocents" or "Murderers").</param>
    public void ShowWinner(string winningTeam)
    {
        gameObject.SetActive(true);
        
        if (winnerText != null)
        {
            bool innocentsWon = winningTeam.Equals("Innocents", System.StringComparison.OrdinalIgnoreCase);
            winnerText.text = innocentsWon ? innocentsWinText : murderersWinText;
            winnerText.color = innocentsWon ? innocentsWinColor : murderersWinColor;
        }
        
        StartCoroutine(FadeIn());
    }

    /// <summary>
    /// Coroutine that fades in the UI panel.
    /// </summary>
    /// <returns>Coroutine enumerator.</returns>
    private IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
}
