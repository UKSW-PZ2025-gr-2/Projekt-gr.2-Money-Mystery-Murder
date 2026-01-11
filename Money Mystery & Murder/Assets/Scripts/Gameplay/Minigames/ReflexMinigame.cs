using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ReflexMinigameHardcoreReusable : MinigameBase
{
    [Header("Reflex Settings")]
    /// <summary>
    /// Minimum wait time between indicator activations in seconds.
    /// </summary>
    [SerializeField] private float minWaitTime = 0.8f;
    
    /// <summary>
    /// Maximum wait time between indicator activations in seconds.
    /// </summary>
    [SerializeField] private float maxWaitTime = 1.4f;
    
    /// <summary>
    /// Initial time window in seconds for hitting the active indicator.
    /// </summary>
    [SerializeField] private float activeTime = 0.6f;
    
    /// <summary>
    /// Minimum active time window (difficulty cap) in seconds.
    /// </summary>
    [SerializeField] private float minActiveTime = 0.35f;
    
    /// <summary>
    /// Money reward given for each successful hit.
    /// </summary>
    [SerializeField] private int rewardPerHit = 20;

    [Header("Visual Settings")]
    /// <summary>
    /// Scale of the indicator sprites.
    /// </summary>
    [SerializeField] private Vector3 indicatorScale = Vector3.one;
    
    /// <summary>
    /// Horizontal spacing between indicators.
    /// </summary>
    [SerializeField] private float spacing = 1.6f;
    
    /// <summary>
    /// Vertical offset of indicators from the minigame object.
    /// </summary>
    [SerializeField] private float verticalOffset = 2f;
    
    /// <summary>
    /// Sprite used for the indicators.
    /// </summary>
    [SerializeField] private Sprite indicatorSprite;

    [Header("Colors")]
    /// <summary>
    /// Color of inactive indicators (default state).
    /// </summary>
    [SerializeField] private Color inactiveColor = Color.red;
    
    /// <summary>
    /// Color of the active indicator that should be hit.
    /// </summary>
    [SerializeField] private Color activeColor = Color.green;
    
    /// <summary>
    /// Color briefly shown when an indicator is successfully hit.
    /// </summary>
    [SerializeField] private Color hitColor = Color.cyan;
    
    /// <summary>
    /// Color of distractor indicators (should not be hit).
    /// </summary>
    [SerializeField] private Color distractColor = Color.yellow;

    /// <summary>
    /// List of sprite renderers for the three indicators.
    /// </summary>
    private List<SpriteRenderer> _indicators = new();
    
    /// <summary>
    /// Index of the currently active indicator (0-2).
    /// </summary>
    private int _currentIndex;
    
    /// <summary>
    /// Whether an indicator is currently active and can be hit.
    /// </summary>
    private bool _isActive;
    
    /// <summary>
    /// Total number of successful hits during this game session.
    /// </summary>
    private int _hits;

    /// <summary>
    /// TextMesh displaying the score and remaining time.
    /// </summary>
    private TextMesh _scoreText;

    /// <summary>
    /// Time remaining in the current game session.
    /// </summary>
    private float _timeRemaining;
    
    /// <summary>
    /// Whether the game is currently running.
    /// </summary>
    private bool _gameRunning;

    /// <summary>
    /// Called when the minigame starts. Initializes game state and UI.
    /// </summary>
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

    /// <summary>
    /// Creates the three indicator sprites at the start of the game.
    /// </summary>
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

    /// <summary>
    /// Creates the TextMesh for displaying score and time.
    /// </summary>
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

    /// <summary>
    /// Resets all indicators to inactive color.
    /// </summary>
    private void ResetIndicators()
    {
        foreach (var sr in _indicators)
            sr.color = inactiveColor;
    }

    /// <summary>
    /// Activates a random indicator and optionally shows distractor indicators.
    /// </summary>
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

    /// <summary>
    /// Deactivates the current indicator and schedules the next activation.
    /// Increases difficulty by reducing active time.
    /// </summary>
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

    /// <summary>
    /// Unity lifecycle method called once per frame.
    /// Handles countdown timer and key input for hitting indicators.
    /// </summary>
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

        if (correctHit) RegisterHit();
    }

    /// <summary>
    /// Registers a successful hit, awards the player, and updates visuals.
    /// </summary>
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

    /// <summary>
    /// Updates the score text display with current hits and remaining time.
    /// </summary>
    private void UpdateScore()
    {
        if (_scoreText != null)
            _scoreText.text = $"HITS: {_hits} | TIME: {_timeRemaining:F1}s";
    }

    /// <summary>
    /// Called when the minigame ends. Hides all UI elements.
    /// </summary>
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

    /// <summary>
    /// Gets the cost to start this minigame.
    /// </summary>
    /// <returns>Zero (free to play).</returns>
    protected override int GetStartCost() => 0;
    
    /// <summary>
    /// Gets whether the minigame allows the player's balance to go negative when paying the start cost.
    /// </summary>
    /// <returns>False (does not allow negative balance).</returns>
    protected override bool AllowNegativeBalanceOnStart() => false;
}
