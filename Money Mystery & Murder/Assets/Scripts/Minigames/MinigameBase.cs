using UnityEngine;

// Base class for all minigames. Derive from this to implement concrete minigames.
// Override OnStartGame / OnEndGame for custom logic.
public abstract class MinigameBase : MonoBehaviour
{
    public bool IsRunning { get; private set; }
    public MonoBehaviour Host { get; private set; } // generalized to avoid dependency issues

    // Called by host immediately after creation/assignment
    public void Initialize(MonoBehaviour host)
    {
        Host = host;
    }

    // Open / start the minigame
    public void StartGame()
    {
        if (IsRunning) return;
        IsRunning = true;
        gameObject.SetActive(true);
        OnStartGame();
    }

    // Close / finish the minigame
    public void EndGame()
    {
        if (!IsRunning) return;
        IsRunning = false;
        OnEndGame();
        gameObject.SetActive(false);
    }

    protected virtual void OnStartGame() { }
    protected virtual void OnEndGame() { }
}
