using System.Collections;
using UnityEngine;
using TMPro;

using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// UI component that displays the player's role temporarily with fade-in/fade-out effects.
/// Used by <see cref="Player"/> to announce assigned roles at game start.
/// </summary>
public class RoleAnnouncer : MonoBehaviour
{
    /// <summary>
    /// TextMeshPro text component for displaying the role.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private TMP_Text uiText;
    
    /// <summary>
    /// Duration in seconds to show the role before fading out.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private float showSeconds = 2.5f;
    
    /// <summary>
    /// Duration in seconds for the fade-out transition.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private float fadeSeconds = 1.0f;
    
    /// <summary>
    /// Starting color for the text (alpha will be animated).
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private Color startColor = Color.white;

    /// <summary>Currently running coroutine reference.</summary>
    private Coroutine _routine;

    /// <summary>
    /// Initializes the UI text component and sets initial alpha to zero.
    /// </summary>
    void Awake()
    {
        if (uiText == null)
        {
            uiText = GetComponentInChildren<TMP_Text>(true);
        }
        if (uiText != null)
        {
            var c = startColor; c.a = 0f; uiText.color = c;
            uiText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Shows the specified <see cref="PlayerRole"/> with fade-in/fade-out animation.
    /// </summary>
    /// <param name="role">The role to display.</param>
    public void ShowRole(PlayerRole role)
    {
        if (uiText == null) return;
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ShowAndFade(role.ToString()));
    }

    /// <summary>
    /// Coroutine that fades in the text instantly, waits, then fades out.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <returns>Coroutine enumerator.</returns>
    private IEnumerator ShowAndFade(string text)
    {
        uiText.text = text;

        // fade in instantly to full alpha
        var c = startColor; c.a = 1f; uiText.color = c;

        // wait visible time
        yield return new WaitForSeconds(showSeconds);

        // fade out
        float t = 0f;
        Color from = uiText.color;
        Color to = uiText.color; to.a = 0f;
        while (t < fadeSeconds)
        {
            t += Time.deltaTime;
            float k = fadeSeconds <= 0f ? 1f : Mathf.Clamp01(t / fadeSeconds);
            uiText.color = Color.Lerp(from, to, k);
            yield return null;
        }
        uiText.color = to;
    }
}
