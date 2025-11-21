using UnityEngine;

// Base class for all minigames. Derive from this to implement concrete minigames.
// Override OnStartGame / OnEndGame for custom logic.
public abstract class MinigameBase : MonoBehaviour
{
    public bool IsRunning { get; private set; }
    public MonoBehaviour Host { get; private set; } // generalized to avoid dependency issues

    // Player that activated / is locked by this minigame
    public Player ActivatingPlayer { get; private set; }
    private PlayerMovement _disabledMovement; // cached movement we disabled

    // Called by host immediately after creation/assignment
    public void Initialize(MonoBehaviour host)
    {
        Host = host;
    }

    // Open / start the minigame (legacy call without player context)
    public void StartGame()
    {
        StartGame(null);
    }

    // Open / start the minigame with activating player (movement will be disabled)
    public void StartGame(Player player)
    {
        if (IsRunning) return;
        IsRunning = true;
        ActivatingPlayer = player;

        // Charge start cost, if any
        if (ActivatingPlayer != null)
        {
            int cost = GetStartCost();
            if (cost > 0)
            {
                if (ActivatingPlayer.SpendBalance(cost, AllowNegativeBalanceOnStart()))
                {
                    Debug.Log($"[MinigameBase] Charged {cost} balance from player {ActivatingPlayer.name} to start minigame {name} (allowNegative={AllowNegativeBalanceOnStart()})");
                }
                else
                {
                    Debug.Log($"[MinigameBase] Player {ActivatingPlayer.name} could not pay {cost} to start minigame {name}");
                }
            }
        }

        if (ActivatingPlayer != null)
        {
            var mv = ActivatingPlayer.GetComponent<PlayerMovement>();
            if (mv != null && mv.enabled)
            {
                _disabledMovement = mv;
                mv.enabled = false; // block movement input
            }
        }
        // No automatic enabling/disabling of GameObjects
        OnStartGame();
    }

    // Close / finish the minigame
    public void EndGame()
    {
        if (!IsRunning) return;
        IsRunning = false;
        OnEndGame();

        // Grant reward (if any) to activating player before cleanup
        if (ActivatingPlayer != null)
        {
            int reward = GetRewardBalance();
            if (reward > 0)
            {
                ActivatingPlayer.AddBalance(reward);
                Debug.Log($"[MinigameBase] Granted {reward} balance to player {ActivatingPlayer.name} from minigame {name}");
            }
        }

        // No automatic enabling/disabling of GameObjects
        if (_disabledMovement != null)
        {
            _disabledMovement.enabled = true; // restore movement
            _disabledMovement = null;
        }
        ActivatingPlayer = null;
    }

    protected virtual void OnStartGame() { }
    protected virtual void OnEndGame() { }
    // Override to provide reward balance for activating player when minigame ends.
    protected virtual int GetRewardBalance() { return 0; }
    // Override to provide a start cost that will be deducted when the minigame starts.
    protected virtual int GetStartCost() { return 0; }
    // Override to allow a minigame to push the player's balance negative when paying start cost.
    protected virtual bool AllowNegativeBalanceOnStart() { return false; }
}
