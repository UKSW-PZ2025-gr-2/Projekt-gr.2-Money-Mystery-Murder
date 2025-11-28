using UnityEngine;
using TMPro;

/// <summary>
/// Displays player HUD information including phase, balance, health, role, and time.
/// Automatically refreshes at a configurable interval and retrieves data from <see cref="Player"/> and <see cref="GameManager"/>.
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    /// <summary>
    /// Reference to the local <see cref="Player"/> whose data is displayed.
    /// Set this in the Unity Inspector.
    /// </summary>
    [Header("Targets")]
    [SerializeField] private Player player;

    /// <summary>
    /// TextMeshPro component displaying the current game phase.
    /// Set this in the Unity Inspector.
    /// </summary>
    [Header("Text Fields")]
    [SerializeField] private TMP_Text phaseText;
    
    /// <summary>
    /// TextMeshPro component displaying the player's balance.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private TMP_Text balanceText;
    
    /// <summary>
    /// TextMeshPro component displaying the player's health.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private TMP_Text healthText;
    
    /// <summary>
    /// TextMeshPro component displaying the player's role.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private TMP_Text roleText;
    
    /// <summary>
    /// TextMeshPro component displaying the in-game time.
    /// Set this in the Unity Inspector.
    /// </summary>
    [SerializeField] private TMP_Text timeText;

    /// <summary>
    /// Refresh interval in seconds.
    /// Set this in the Unity Inspector.
    /// </summary>
    [Header("Refresh Settings")]
    [SerializeField] private float refreshInterval = 0.25f;
    
    /// <summary>Internal timer for refresh intervals.</summary>
    private float _timer;

    /// <summary>
    /// Finds the <see cref="Player"/> if not assigned.
    /// </summary>
    void Awake()
    {
        if (player == null)
        {   
            player = Object.FindFirstObjectByType<Player>();
        }
    }

    /// <summary>
    /// Updates the HUD at the configured refresh interval.
    /// </summary>
    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= refreshInterval)
        {
            _timer = 0f;
            Refresh();
        }
    }

    /// <summary>
    /// Sets the <see cref="Player"/> reference and refreshes the HUD.
    /// </summary>
    /// <param name="p">The player to display data for.</param>
    public void SetPlayer(Player p)
    {
        player = p;
        Refresh();
    }

    /// <summary>
    /// Updates the phase label. Not yet fully implemented.
    /// </summary>
    /// <param name="phase">The phase string to display.</param>
    public void UpdatePhaseLabel(string phase)
    {
        // TODO: Logic
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Updates the money display. Not yet fully implemented.
    /// </summary>
    /// <param name="amount">The money amount to display.</param>
    public void UpdateMoney(int amount)
    {
        // TODO: Logic
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Updates the health display. Not yet fully implemented.
    /// </summary>
    /// <param name="ratio">The health ratio (0-1) to display.</param>
    public void UpdateHealth(float ratio)
    {
        // TODO: Logic
        throw new System.NotImplementedException();
    }

    /// <summary>Refreshes all HUD text fields with current data from <see cref="Player"/> and <see cref="GameManager"/>.</summary>
    private void Refresh()
    {
        if (phaseText != null)
        {
            var gm = GameManager.Instance;
            phaseText.text = gm ? $"Phase: {gm.CurrentPhase}" : "Phase: -";
        }
        if (balanceText != null)
        {
            balanceText.text = player ? $"Balance: {player.Balance}" : "Balance: -";
        }
        if (healthText != null)
        {
            healthText.text = player ? $"HP: {player.CurrentHealth}/{player.MaxHealth}" : "HP: -";
        }
        if (roleText != null)
        {
            roleText.text = player ? $"Role: {player.Role}" : "Role: -";
        }
        if (timeText != null)
        {
            var gm = GameManager.Instance;
            timeText.text = gm ? $"Time: {gm.CurrentTimeFormatted}" : "Time: -";
        }
    }
}
