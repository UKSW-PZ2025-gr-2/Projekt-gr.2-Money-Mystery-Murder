using UnityEngine;
using System.Collections.Generic;



/// <summary>
/// Singleton manager that controls game state and time progression.
/// Integrates with <see cref="RoleManager"/> and <see cref="PhaseManager"/>.
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>Singleton instance of the <see cref="GameManager"/>.</summary>
    public static GameManager Instance { get; private set; }

    /// <summary>When enabled, assigns unique roles to all <see cref="Player"/> instances on Start. Set this in the Unity Inspector.</summary>
    [SerializeField] private bool assignUniqueRolesOnStart = false;

    /// <summary>Reference to the <see cref="RoleManager"/> system. Set this in the Unity Inspector.</summary>
    [Header("Systems")]
    [SerializeField] private RoleManager roleManager;
    
    /// <summary>Reference to the <see cref="PhaseManager"/> system. Set this in the Unity Inspector.</summary>
    [SerializeField] private PhaseManager phaseManager;
    
    /// <summary>Reference to the <see cref="GameEndUI"/> component. Set this in the Unity Inspector.</summary>
    [SerializeField] private GameEndUI gameEndUI;

    /// <summary>Audio clip for game music. Set this in the Unity Inspector.</summary>
    [Header("Audio")]
    [SerializeField] private AudioClip gameMusic;

    /// <summary>Starting hour (0-23) when game begins. Set this in the Unity Inspector.</summary>
    [Header("Game Time")]
    [SerializeField] private int startHour = 6;
    
    /// <summary>Starting minute (0-59) when game begins. Set this in the Unity Inspector.</summary>
    [SerializeField] private int startMinute = 0;
    
    /// <summary>In-game minutes progressed per real-time second. Set this in the Unity Inspector.</summary>
    [SerializeField] private float timeScale = 1f;
    
    /// <summary>Internal time counter in minutes since midnight (0-1439).</summary>
    private float _currentTimeMinutes;
    
    /// <summary>Whether the game has ended.</summary>
    private bool _gameEnded = false;

    /// <summary>Gets the current player count from RoleManager.</summary>
    public int PlayerCount => roleManager != null ? roleManager.PlayerCount : 0;
    
    /// <summary>Gets the current time in minutes since midnight.</summary>
    public float CurrentTimeMinutes => _currentTimeMinutes;
    
    /// <summary>Gets the current hour (0-23).</summary>
    public int CurrentHour => Mathf.FloorToInt(_currentTimeMinutes / 60f) % 24;
    
    /// <summary>Gets the current minute (0-59).</summary>
    public int CurrentMinute => Mathf.FloorToInt(_currentTimeMinutes % 60f);
    
    /// <summary>Gets the current time formatted as HH:MM.</summary>
    public string CurrentTimeFormatted => $"{CurrentHour:00}:{CurrentMinute:00}";
    
    /// <summary>Gets the current game phase from the PhaseManager.</summary>
    public GamePhase CurrentPhase => phaseManager != null ? phaseManager.CurrentPhase : GamePhase.None;

    /// <summary>Gets the RoleManager instance.</summary>
    public RoleManager RoleManager => roleManager;
    
    /// <summary>Gets the PhaseManager instance.</summary>
    public PhaseManager PhaseManager => phaseManager;

    /// <summary>Initializes the singleton instance and ensures persistence across scenes.</summary>
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        if (gameEndUI == null)
        {
            // Use the newer API to find a scene instance (replaces obsolete FindObjectOfType)
            gameEndUI = Object.FindFirstObjectByType<GameEndUI>();
        }
    }

    /// <summary>Initializes game time, phase schedule, and optionally assigns roles to all <see cref="Player"/> instances.</summary>
    void Start()
    {
        if (assignUniqueRolesOnStart && roleManager != null)
        {
            roleManager.InitializeRolePool();
            roleManager.AssignRolesToPlayers();
        }

        startHour = Mathf.Clamp(startHour, 0, 23);
        startMinute = Mathf.Clamp(startMinute, 0, 59);
        _currentTimeMinutes = startHour * 60 + startMinute;

        if (phaseManager != null)
        {
            phaseManager.Initialize();
            phaseManager.UpdatePhaseByTime(CurrentHour);
        }

        // Play game music
        if (AudioManager.Instance != null && gameMusic != null)
        {
            AudioManager.Instance.PlayMusic(gameMusic);
        }
    }

    /// <summary>Updates game time and checks for phase transitions each frame.</summary>
    void Update()
    {
        if (!_gameEnded)
        {
            TickGameTime();
            
            if (phaseManager != null)
            {
                phaseManager.UpdatePhaseByTime(CurrentHour);
            }
        }
    }

    /// <summary>Advances the in-game time based on the time scale.</summary>
    private void TickGameTime()
    {
        if (timeScale <= 0f) return;
        _currentTimeMinutes += Time.deltaTime * timeScale;
        if (_currentTimeMinutes >= 1440f) _currentTimeMinutes %= 1440f;
    }

    /// <summary>
    /// Sets the game time to the specified hour and minute, and updates phase if necessary.
    /// </summary>
    /// <param name="hour">Target hour (0-23).</param>
    /// <param name="minute">Target minute (0-59).</param>
    public void SetTime(int hour, int minute)
    {
        hour = Mathf.Clamp(hour, 0, 23);
        minute = Mathf.Clamp(minute, 0, 59);
        _currentTimeMinutes = hour * 60 + minute;
        
        if (phaseManager != null)
        {
            phaseManager.UpdatePhaseByTime(hour);
        }
    }

    /// <summary>Checks win condition logic based on role survival.</summary>
    public void CheckWinCondition()
    {
        if (roleManager == null || _gameEnded) return;

        bool murderersAlive = roleManager.CountAlivePlayersByRole(PlayerRole.Murderer) > 0;
        bool innocentsAlive = roleManager.CountAlivePlayersByRole(PlayerRole.Civilian) > 0 
                           || roleManager.CountAlivePlayersByRole(PlayerRole.Detective) > 0;

        if (!murderersAlive && innocentsAlive)
        {
            Debug.Log("[GameManager] Innocents win! All murderers eliminated.");
            EndGame("Innocents");
        }
        else if (!innocentsAlive && murderersAlive)
        {
            Debug.Log("[GameManager] Murderers win! All innocents eliminated.");
            EndGame("Murderers");
        }
    }

    /// <summary>
    /// Ends the game with a winning team.
    /// </summary>
    /// <param name="winningTeam">The name of the winning team.</param>
    private void EndGame(string winningTeam)
    {
        if (_gameEnded) return;
        
        _gameEnded = true;
        
        if (phaseManager != null)
        {
            phaseManager.SetPhase(GamePhase.End);
        }
        
        if (gameEndUI != null)
        {
            gameEndUI.ShowWinner(winningTeam);
        }
        
        Debug.Log($"[GameManager] Game ended. Winner: {winningTeam}");
    }
    
    /// <summary>Gets whether the game has ended.</summary>
    public bool IsGameEnded => _gameEnded;
    
    /// <summary>Resets the game state for a new game.</summary>
    public void ResetGame()
    {
        _gameEnded = false;
        
        if (gameEndUI != null)
        {
            gameEndUI.Hide();
        }
        
        if (phaseManager != null)
        {
            phaseManager.SetPhase(GamePhase.Day);
        }
        
        _currentTimeMinutes = startHour * 60 + startMinute;
        
        Debug.Log("[GameManager] Game reset");
    }
}
