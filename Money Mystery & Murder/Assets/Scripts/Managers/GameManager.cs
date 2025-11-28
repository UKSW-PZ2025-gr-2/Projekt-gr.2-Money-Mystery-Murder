using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Enum defining the different phases of gameplay.
/// </summary>
public enum GamePhase
{
    /// <summary>No phase set.</summary>
    None,
    /// <summary>Lobby/waiting phase before game starts.</summary>
    Lobby,
    /// <summary>Daytime phase.</summary>
    Day,
    /// <summary>Evening phase.</summary>
    Evening,
    /// <summary>Nighttime phase.</summary>
    Night,
    /// <summary>Game end phase.</summary>
    End
}

/// <summary>
/// Singleton manager that controls game state, phase transitions, player roles, and time progression.
/// Manages role assignment to <see cref="Player"/> instances and integrates with <see cref="RoleManager"/> and <see cref="PhaseManager"/>.
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>Singleton instance of the <see cref="GameManager"/>.</summary>
    public static GameManager Instance { get; private set; }

    /// <summary>Current number of players in the game. Set this in the Unity Inspector.</summary>
    [SerializeField] private int playerCount = 1;
    
    /// <summary>Debug view of the generated role pool. Set this in the Unity Inspector.</summary>
    [SerializeField] private List<PlayerRole> rolePoolDebugView = new();
    
    /// <summary>When enabled, assigns unique roles to all <see cref="Player"/> instances on Start. Set this in the Unity Inspector.</summary>
    [SerializeField] private bool assignUniqueRolesOnStart = false;

    /// <summary>Reference to the <see cref="RoleManager"/> system. Set this in the Unity Inspector.</summary>
    [Header("Systems")]
    [SerializeField] private RoleManager roleManager;
    
    /// <summary>Reference to the <see cref="PhaseManager"/> system. Set this in the Unity Inspector.</summary>
    [SerializeField] private PhaseManager phaseManager;

    /// <summary>Current game phase (Day, Evening, Night, etc.). Set this in the Unity Inspector.</summary>
    [Header("Game Phase")]
    [SerializeField] private GamePhase currentPhase = GamePhase.Day;

    /// <summary>Hour (0-23) when Day phase begins. Set this in the Unity Inspector.</summary>
    [Header("Phase Schedule (Hours)")]
    [Range(0,23)] [SerializeField] private int dayStartHour = 6;
    
    /// <summary>Hour (0-23) when Evening phase begins. Set this in the Unity Inspector.</summary>
    [Range(0,23)] [SerializeField] private int eveningStartHour = 18;
    
    /// <summary>Hour (0-23) when Night phase begins. Set this in the Unity Inspector.</summary>
    [Range(0,23)] [SerializeField] private int nightStartHour = 21;

    /// <summary>Starting hour (0-23) when game begins. Set this in the Unity Inspector.</summary>
    [Header("Game Time")]
    [SerializeField] private int startHour = 6;
    
    /// <summary>Starting minute (0-59) when game begins. Set this in the Unity Inspector.</summary>
    [SerializeField] private int startMinute = 0;
    
    /// <summary>In-game minutes progressed per real-time second. Set this in the Unity Inspector.</summary>
    [SerializeField] private float timeScale = 1f;
    
    /// <summary>Internal time counter in minutes since midnight (0-1439).</summary>
    private float _currentTimeMinutes;

    /// <summary>Gets the current player count.</summary>
    public int PlayerCount => playerCount;
    
    /// <summary>Gets the current time in minutes since midnight.</summary>
    public float CurrentTimeMinutes => _currentTimeMinutes;
    
    /// <summary>Gets the current hour (0-23).</summary>
    public int CurrentHour => Mathf.FloorToInt(_currentTimeMinutes / 60f) % 24;
    
    /// <summary>Gets the current minute (0-59).</summary>
    public int CurrentMinute => Mathf.FloorToInt(_currentTimeMinutes % 60f);
    
    /// <summary>Gets the current time formatted as HH:MM.</summary>
    public string CurrentTimeFormatted => $"{CurrentHour:00}:{CurrentMinute:00}";
    
    /// <summary>Gets the current game phase.</summary>
    public GamePhase CurrentPhase => currentPhase;

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
    }

    /// <summary>Initializes game time, phase schedule, and optionally assigns roles to all <see cref="Player"/> instances.</summary>
    void Start()
    {
        if (assignUniqueRolesOnStart)
        {
            AssignRolesToPlayers();
        }
        else
        {
            rolePoolDebugView = BuildRolePool(Mathf.Max(1, playerCount));
        }

        startHour = Mathf.Clamp(startHour, 0, 23);
        startMinute = Mathf.Clamp(startMinute, 0, 59);
        _currentTimeMinutes = startHour * 60 + startMinute;

        eveningStartHour = Mathf.Clamp(eveningStartHour, 0, 23);
        nightStartHour = Mathf.Clamp(nightStartHour, 0, 23);
        dayStartHour = Mathf.Clamp(dayStartHour, 0, 23);
        if (eveningStartHour <= dayStartHour) eveningStartHour = Mathf.Min(23, dayStartHour + 1);
        if (nightStartHour <= eveningStartHour) nightStartHour = Mathf.Min(23, eveningStartHour + 1);

        if (currentPhase == GamePhase.None || currentPhase == GamePhase.Lobby)
        {
            currentPhase = GamePhase.Day;
        }

        var phaseByTime = DeterminePhaseByTime(CurrentHour);
        if (phaseByTime != currentPhase)
        {
            SetPhase(phaseByTime);
        }
    }

    /// <summary>Updates game time and checks for phase transitions each frame.</summary>
    void Update()
    {
        TickGameTime();
        var phaseByTime = DeterminePhaseByTime(CurrentHour);
        if (phaseByTime != currentPhase)
        {
            SetPhase(phaseByTime);
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
    /// Determines the appropriate game phase based on the given hour.
    /// </summary>
    /// <param name="hour">The hour (0-23) to evaluate.</param>
    /// <returns>The corresponding <see cref="GamePhase"/>.</returns>
    private GamePhase DeterminePhaseByTime(int hour)
    {
        if (hour >= nightStartHour || hour < dayStartHour) return GamePhase.Night;
        if (hour >= eveningStartHour) return GamePhase.Evening;
        return GamePhase.Day;
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
        var phaseByTime = DeterminePhaseByTime(hour);
        if (phaseByTime != currentPhase)
        {
            SetPhase(phaseByTime);
        }
    }

    /// <summary>
    /// Sets the current game phase.
    /// </summary>
    /// <param name="phase">The <see cref="GamePhase"/> to set.</param>
    public void SetPhase(GamePhase phase)
    {
        currentPhase = phase;
        Debug.Log($"[GameManager] Phase set to {currentPhase}");
    }

    /// <summary>Starts the game. Logic not yet implemented.</summary>
    public void StartGame()
    {
        // TODO: Logic
        throw new System.NotImplementedException();
    }

    /// <summary>Checks win condition logic. Not yet implemented.</summary>
    public void CheckWinCondition()
    {
        // TODO: Logic
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Changes the current game phase to a new state. Not yet implemented.
    /// </summary>
    /// <param name="newState">The new <see cref="GamePhase"/> to transition to.</param>
    public void ChangePhase(GamePhase newState)
    {
        // TODO: Logic
        throw new System.NotImplementedException();
    }

    /// <summary>Advances the game phase to the next sequential phase.</summary>
    public void AdvancePhase()
    {
        switch (currentPhase)
        {
            case GamePhase.Day: currentPhase = GamePhase.Evening; break;
            case GamePhase.Evening: currentPhase = GamePhase.Night; break;
            case GamePhase.Night: currentPhase = GamePhase.Day; break;
            case GamePhase.End: currentPhase = GamePhase.Day; break;
            default: currentPhase = GamePhase.Day; break;
        }
        Debug.Log($"[GameManager] Phase advanced to {currentPhase}");
    }

    /// <summary>
    /// Sets the player count to the specified value.
    /// </summary>
    /// <param name="count">Number of players (clamped to non-negative).</param>
    public void SetPlayerCount(int count)
    {
        if (count < 0) count = 0;
        playerCount = count;
    }

    /// <summary>Increments the player count by one.</summary>
    public void IncrementPlayerCount() => playerCount++;
    
    /// <summary>Decrements the player count by one if greater than zero.</summary>
    public void DecrementPlayerCount() { if (playerCount > 0) playerCount--; }

    /// <summary>Assigns unique roles from a generated pool to all <see cref="Player"/> instances in the scene.</summary>
    public void AssignRolesToPlayers()
    {
        var players = Object.FindObjectsByType<Player>(FindObjectsSortMode.None);
        if (players == null || players.Length == 0) return;
        playerCount = players.Length;
        var pool = BuildRolePool(playerCount);
        rolePoolDebugView = new List<PlayerRole>(pool);
        Shuffle(pool);
        int index = 0;
        foreach (var p in players)
        {
            if (index >= pool.Count) { p.SetRole(PlayerRole.Civilian); continue; }
            p.SetRole(pool[index]);
            index++;
        }
    }

    /// <summary>
    /// Picks a random role from the role pool based on the current player count.
    /// </summary>
    /// <returns>A random <see cref="PlayerRole"/>.</returns>
    public PlayerRole PickRandomRoleFromPool()
    {
        int count = playerCount;
        var players = Object.FindObjectsByType<Player>(FindObjectsSortMode.None);
        if (players != null && players.Length > 0) count = players.Length;
        var pool = BuildRolePool(Mathf.Max(1, count));
        if (pool.Count == 0) return PlayerRole.Civilian;
        int idx = Random.Range(0, pool.Count);
        return pool[idx];
    }

    /// <summary>
    /// Builds a role pool with balanced distribution (25% Murderer, 1 Detective if >= 6 players, rest Civilian).
    /// </summary>
    /// <param name="totalPlayers">Total number of players.</param>
    /// <returns>A list of <see cref="PlayerRole"/> representing the role pool.</returns>
    private List<PlayerRole> BuildRolePool(int totalPlayers)
    {
        var result = new List<PlayerRole>(totalPlayers);
        if (totalPlayers <= 0) return result;
        int mafiaCount = Mathf.RoundToInt(totalPlayers * 0.25f);
        int detectiveCount = totalPlayers >= 6 ? 1 : 0;
        int civiliansCount = Mathf.Max(0, totalPlayers - mafiaCount - detectiveCount);
        int totalAllocated = mafiaCount + detectiveCount + civiliansCount;
        if (totalAllocated > totalPlayers)
        {
            int overflow = totalAllocated - totalPlayers;
            civiliansCount = Mathf.Max(0, civiliansCount - overflow);
        }
        else if (totalAllocated < totalPlayers)
        {
            civiliansCount += (totalPlayers - totalAllocated);
        }
        for (int i = 0; i < mafiaCount; i++) result.Add(PlayerRole.Murderer);
        for (int i = 0; i < detectiveCount; i++) result.Add(PlayerRole.Detective);
        for (int i = 0; i < civiliansCount; i++) result.Add(PlayerRole.Civilian);
        return result;
    }

    /// <summary>
    /// Shuffles the given list using Fisher-Yates algorithm.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to shuffle.</param>
    private void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
