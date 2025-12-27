using UnityEngine;

/// <summary>
/// Base class for all minigames. Derive from this to implement concrete minigames.
/// Handles minigame lifecycle (start/end), player locking, movement disabling, cost charging, and reward granting.
/// Override <see cref="OnStartGame"/>, <see cref="OnEndGame"/>, <see cref="GetStartCost"/>, <see cref="GetRewardBalance"/>, and <see cref="AllowNegativeBalanceOnStart"/> for custom logic.
/// Used by <see cref="SlotMachineMinigame"/> and integrates with <see cref="Player"/> and <see cref="PlayerMovement"/>.
/// </summary>
public abstract class MinigameBase : MonoBehaviour
{
    /// <summary>Gets whether the minigame is currently running.</summary>
    public bool IsRunning { get; private set; }
    
    /// <summary>Gets the host MonoBehaviour that created this minigame.</summary>
    public MonoBehaviour Host { get; private set; }

    /// <summary>Gets the <see cref="Player"/> that activated and is locked by this minigame.</summary>
    public Player ActivatingPlayer { get; private set; }
    
    /// <summary>Cached <see cref="PlayerMovement"/> component that was disabled.</summary>
    private PlayerMovement _disabledMovement;

    /// <summary>
    /// Called by host immediately after creation/assignment.
    /// </summary>
    /// <param name="host">The MonoBehaviour hosting this minigame.</param>
    public void Initialize(MonoBehaviour host)
    {
        Host = host;
    }

    /// <summary>
    /// Opens/starts the minigame (legacy call without player context).
    /// </summary>
    public void StartGame()
    {
        StartGame(null);
    }

    /// <summary>
    /// Opens/starts the minigame with the activating <see cref="Player"/> (movement will be disabled).
    /// Charges the start cost from the player's balance.
    /// </summary>
    /// <param name="player">The player activating the minigame.</param>
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
                    Debug.Log($"[MinigameBase] Player {ActivatingPlayer.name} went into the negatives paying {cost} to start minigame {name}");
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

    /// <summary>
    /// Closes/finishes the minigame. Grants reward to the activating <see cref="Player"/> and re-enables movement.
    /// </summary>
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

    /// <summary>
    /// Override this method in derived classes to implement custom start logic.
    /// </summary>
    protected virtual void OnStartGame() { }
    
    /// <summary>
    /// Override this method in derived classes to implement custom end logic.
    /// </summary>
    protected virtual void OnEndGame() { }
    
    /// <summary>
    /// Override to provide reward balance for the activating <see cref="Player"/> when minigame ends.
    /// </summary>
    /// <returns>Reward amount in balance.</returns>
    protected virtual int GetRewardBalance() { return 0; }
    
    /// <summary>
    /// Override to provide a start cost that will be deducted when the minigame starts.
    /// </summary>
    /// <returns>Start cost in balance.</returns>
    protected virtual int GetStartCost() { return 0; }
    
    /// <summary>
    /// Override to allow a minigame to push the <see cref="Player"/>'s balance negative when paying start cost.
    /// </summary>
    /// <returns>True if negative balance is allowed.</returns>
    protected virtual bool AllowNegativeBalanceOnStart() { return false; }
}
