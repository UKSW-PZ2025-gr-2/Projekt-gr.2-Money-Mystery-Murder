using UnityEngine;
using TMPro;

// Displays player HUD: phase, balance, health, role, time
public class PlayerHUD : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Player player; // reference to local player

    [Header("Text Fields")]
    [SerializeField] private TMP_Text phaseText;
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text roleText;
    [SerializeField] private TMP_Text timeText;

    [Header("Refresh Settings")]
    [SerializeField] private float refreshInterval = 0.25f; // update UI every X seconds
    private float _timer;

    void Awake()
    {
        if (player == null)
        {   
            player = Object.FindFirstObjectByType<Player>();
        }
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= refreshInterval)
        {
            _timer = 0f;
            Refresh();
        }
    }

    public void SetPlayer(Player p)
    {
        player = p;
        Refresh();
    }

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
