using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Simple debug minigame implementation for testing minigame systems.
/// Logs start/end events to the Console and can auto-end after a configurable duration.
/// Can also be manually ended with the Escape key.
/// </summary>
public class DebugMinigame : MinigameBase
{
    /// <summary>
    /// Duration in seconds before the minigame auto-ends. Set to 0 or less to disable auto-end.
    /// </summary>
    [SerializeField] private float autoEndSeconds = 3f;
    
    /// <summary>
    /// Amount of balance granted to the player when the minigame ends successfully.
    /// </summary>
    [SerializeField] private int rewardBalance = 10;
    
    /// <summary>
    /// Amount of balance charged when the minigame starts.
    /// </summary>
    [SerializeField] private int startCost = 1;
    
    /// <summary>
    /// Elapsed time since the minigame started.
    /// </summary>
    private float _elapsed;

    /// <summary>
    /// Called when the minigame starts. Resets the timer and logs the event.
    /// </summary>
    protected override void OnStartGame()
    {
        _elapsed = 0f;
        Debug.Log($"[DebugMinigame] Started (host={(Host ? Host.name : "null")})");
    }

    /// <summary>
    /// Called when the minigame ends. Logs the event.
    /// </summary>
    protected override void OnEndGame()
    {
        Debug.Log("[DebugMinigame] Ended");
    }

    /// <summary>
    /// Gets the reward balance granted to the player on successful completion.
    /// </summary>
    /// <returns>The reward amount.</returns>
    protected override int GetRewardBalance()
    {
        return rewardBalance;
    }

    /// <summary>
    /// Gets the cost to start the minigame.
    /// </summary>
    /// <returns>The start cost.</returns>
    protected override int GetStartCost()
    {
        return startCost;
    }

    /// <summary>
    /// Gets whether the minigame allows the player's balance to go negative when paying the start cost.
    /// </summary>
    /// <returns>True (debug minigame always allows negative balance).</returns>
    protected override bool AllowNegativeBalanceOnStart()
    {
        return true;
    }

    /// <summary>
    /// Unity lifecycle method called once per frame.
    /// Handles auto-end timer and manual end via Escape key.
    /// </summary>
    void Update()
    {
        if (!IsRunning) return;
        _elapsed += Time.deltaTime;
        if (autoEndSeconds > 0f && _elapsed >= autoEndSeconds)
        {
            Debug.Log($"[DebugMinigame] Auto ending after {autoEndSeconds:F2}s");
            EndGame();
            return;
        }
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        if (k != null && k.escapeKey.wasPressedThisFrame)
        {
            Debug.Log("[DebugMinigame] Manually ended by Escape key (InputSystem)");
            EndGame();
        }
#else
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("[DebugMinigame] Manually ended by Escape key (Legacy)");
            EndGame();
        }
#endif
    }
}
