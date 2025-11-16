using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RoleAnnouncer : MonoBehaviour
{
    [SerializeField] private Text uiText; // Legacy Text for simplicity; swap to TMP_Text if needed
    [SerializeField] private float showSeconds = 2.5f;
    [SerializeField] private float fadeSeconds = 1.0f;
    [SerializeField] private Color startColor = Color.white;

    private Coroutine _routine;

    void Awake()
    {
        if (uiText == null)
        {
            uiText = GetComponentInChildren<Text>(true);
        }
        if (uiText != null)
        {
            var c = startColor; c.a = 0f; uiText.color = c;
            uiText.gameObject.SetActive(true);
        }
    }

    public void ShowRole(PlayerRole role)
    {
        if (uiText == null) return;
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ShowAndFade(role.ToString()));
    }

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
