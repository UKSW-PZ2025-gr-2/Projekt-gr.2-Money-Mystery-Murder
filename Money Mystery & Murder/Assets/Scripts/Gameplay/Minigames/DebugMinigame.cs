using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Simple debug minigame implementation. Logs start/end events to the Console.
// Can auto-end after a configurable duration or be manually ended with Escape.
public class DebugMinigame : MinigameBase
{
    [SerializeField] private float autoEndSeconds = 3f; // <= 0 to disable auto-end
    [SerializeField] private int rewardBalance = 10; // amount of balance granted on successful end
    [SerializeField] private int startCost = 1; // amount of balance charged at start
    private float _elapsed;

    protected override void OnStartGame()
    {
        _elapsed = 0f;
        Debug.Log($"[DebugMinigame] Started (host={(Host ? Host.name : "null")})");
    }

    protected override void OnEndGame()
    {
        Debug.Log("[DebugMinigame] Ended");
    }

    protected override int GetRewardBalance()
    {
        return rewardBalance; // configurable reward
    }

    protected override int GetStartCost()
    {
        return startCost; // configurable start cost
    }

    protected override bool AllowNegativeBalanceOnStart()
    {
        return true; // debug minigame always allows balance to go negative when starting
    }

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
