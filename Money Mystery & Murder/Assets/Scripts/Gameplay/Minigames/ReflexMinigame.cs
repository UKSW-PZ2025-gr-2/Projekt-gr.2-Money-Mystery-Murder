using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ReflexMinigameHardcoreReusable : MinigameBase
{
    [Header("Reflex Settings")]
    [SerializeField] private float minWaitTime = 0.8f;
    [SerializeField] private float maxWaitTime = 1.4f;
    [SerializeField] private float activeTime = 0.6f;
    [SerializeField] private float minActiveTime = 0.35f;
    [SerializeField] private int rewardPerHit = 20;

    [Header("Visual Settings")]
    [SerializeField] private Vector3 indicatorScale = Vector3.one;
    [SerializeField] private float spacing = 1.6f;
    [SerializeField] private float verticalOffset = 2f;
    [SerializeField] private Sprite indicatorSprite;

    [Header("Colors")]
    [SerializeField] private Color inactiveColor = Color.red;
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color hitColor = Color.cyan;
    [SerializeField] private Color distractColor = Color.yellow;

    private List<SpriteRenderer> _indicators = new();
    private int _currentIndex;
    private bool _isActive;
    private int _hits;

    private TextMesh _scoreText;

    private float _timeRemaining;
    private bool _gameRunning;

    protected override void OnStartGame()
    {
        _hits = 0;
        _currentIndex = -1;
        _isActive = false;
        _gameRunning = true;
        _timeRemaining = 10f;

        if (_indicators.Count == 0) BuildIndicators();
        else
        {
            foreach (var sr in _indicators)
            {
                if (sr != null) sr.gameObject.SetActive(true);
            }
        }

        if (_scoreText == null) BuildScoreText();
        else _scoreText.gameObject.SetActive(true);

        UpdateScore();
        Invoke(nameof(ActivateNext), Random.Range(minWaitTime, maxWaitTime));
    }

    private void BuildIndicators()
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject go = new GameObject($"Indicator_{i}");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3((i - 1) * spacing, verticalOffset, 0);
            go.transform.localScale = indicatorScale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = indicatorSprite;
            sr.color = inactiveColor;
            sr.sortingOrder = 10;

            _indicators.Add(sr);
        }
    }

    private void BuildScoreText()
    {
        GameObject go = new GameObject("ScoreText");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(3f, verticalOffset, 0);

        _scoreText = go.AddComponent<TextMesh>();
        _scoreText.fontSize = 48;
        _scoreText.characterSize = 0.1f;
        _scoreText.color = Color.white;
    }

    private void ResetIndicators()
    {
        foreach (var sr in _indicators)
            sr.color = inactiveColor;
    }

    private void ActivateNext()
    {
        if (!IsRunning || !_gameRunning) return;

        CancelInvoke(nameof(Deactivate));
        CancelInvoke(nameof(ActivateNext));

        ResetIndicators();

        foreach (var i in _indicators)
        {
            if (Random.value < 0.3f) i.color = distractColor;
        }

        _currentIndex = Random.Range(0, _indicators.Count);
        _indicators[_currentIndex].color = activeColor;
        _isActive = true;

        Invoke(nameof(Deactivate), activeTime);
    }

    private void Deactivate()
    {
        if (_currentIndex >= 0 && _indicators[_currentIndex] != null)
            _indicators[_currentIndex].color = inactiveColor;

        _isActive = false;
        if (_gameRunning)
        {

            activeTime = Mathf.Max(activeTime - 0.003f, minActiveTime);

            Invoke(nameof(ActivateNext), Random.Range(minWaitTime, maxWaitTime));
        }
    }

    void Update()
    {
        if (!IsRunning || !_gameRunning) return;

        _timeRemaining -= Time.deltaTime;
        UpdateScore();
        
        if (_timeRemaining <= 0)
        {
            EndGame();
            return;
        }

        var keyboard = Keyboard.current;
        if (keyboard == null || !_isActive) return;

        bool correctHit = false;

        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            if (_currentIndex == 0) correctHit = true;
            else _hits--;
        }
        if (keyboard.digit2Key.wasPressedThisFrame)
        {
            if (_currentIndex == 1) correctHit = true;
            else _hits--;
        }
        if (keyboard.digit3Key.wasPressedThisFrame)
        {
            if (_currentIndex == 2) correctHit = true;
            else _hits--;
        }

        if (_hits <= 0)
        {
            _hits = 0;
            UpdateScore();
            EndGame();
            return;
        }

        if (correctHit) RegisterHit();
    }

    private void RegisterHit()
    {
        _hits++;
        if (_currentIndex >= 0 && _indicators[_currentIndex] != null)
            _indicators[_currentIndex].color = hitColor;

        _isActive = false;

        if (ActivatingPlayer != null)
            ActivatingPlayer.AddBalance(rewardPerHit);

        CancelInvoke(nameof(Deactivate));
        Invoke(nameof(Deactivate), activeTime);
    }

    private void UpdateScore()
    {
        if (_scoreText != null)
            _scoreText.text = $"HITS: {_hits} | TIME: {_timeRemaining:F1}s";
    }

    protected override void OnEndGame()
    {
        _gameRunning = false;

        foreach (var sr in _indicators)
        {
            if (sr != null)
                sr.gameObject.SetActive(false);
        }

        if (_scoreText != null)
            _scoreText.gameObject.SetActive(false);

        Debug.Log($"GAME OVER! TOTAL HITS: {_hits}");
    }

    protected override int GetStartCost() => 0;
    protected override bool AllowNegativeBalanceOnStart() => false;
}
