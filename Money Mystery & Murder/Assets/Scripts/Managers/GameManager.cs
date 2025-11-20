using UnityEngine;
using System.Collections.Generic;

public enum GamePhase
{
    None,
    Lobby,
    Day,
    Evening,
    Night,
    End
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int playerCount = 1; // current number of players
    [SerializeField] private List<PlayerRole> rolePoolDebugView = new(); // debug: shows generated pool
    [SerializeField] private bool assignUniqueRolesOnStart = false; // enable unique assignment for multiplayer

    // --- Game Phase ---
    [Header("Game Phase")]
    [SerializeField] private GamePhase currentPhase = GamePhase.Day; // default phase is Day

    [Header("Phase Schedule (Hours)")]
    [Tooltip("Hour when Day starts (inclusive)")]
    [Range(0,23)] [SerializeField] private int dayStartHour = 6;
    [Tooltip("Hour when Evening starts (inclusive). Must be after Day start")]
    [Range(0,23)] [SerializeField] private int eveningStartHour = 18;
    [Tooltip("Hour when Night starts (inclusive). Must be after Evening start")]
    [Range(0,23)] [SerializeField] private int nightStartHour = 21;

    // --- Time System ---
    [Header("Game Time")]
    [SerializeField] private int startHour = 6; // game starts at 06:00 by default
    [SerializeField] private int startMinute = 0;
    [SerializeField] private float timeScale = 1f; // in-game minutes progressed per real-time second
    private float _currentTimeMinutes; // minutes since midnight (0..1439)

    public int PlayerCount => playerCount;

    // Exposed time properties
    public float CurrentTimeMinutes => _currentTimeMinutes; // raw minute counter
    public int CurrentHour => Mathf.FloorToInt(_currentTimeMinutes / 60f) % 24;
    public int CurrentMinute => Mathf.FloorToInt(_currentTimeMinutes % 60f);
    public string CurrentTimeFormatted => $"{CurrentHour:00}:{CurrentMinute:00}";
    public GamePhase CurrentPhase => currentPhase;

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

    void Start()
    {
        if (assignUniqueRolesOnStart)
        {
            AssignRolesToPlayers();
        }
        else
        {
            // Keep a preview of the pool based on current player count
            rolePoolDebugView = BuildRolePool(Mathf.Max(1, playerCount));
        }

        // Initialize in-game clock
        startHour = Mathf.Clamp(startHour, 0, 23);
        startMinute = Mathf.Clamp(startMinute, 0, 59);
        _currentTimeMinutes = startHour * 60 + startMinute;

        // Sanitize schedule order
        eveningStartHour = Mathf.Clamp(eveningStartHour, 0, 23);
        nightStartHour = Mathf.Clamp(nightStartHour, 0, 23);
        dayStartHour = Mathf.Clamp(dayStartHour, 0, 23);
        if (eveningStartHour <= dayStartHour) eveningStartHour = Mathf.Min(23, dayStartHour + 1);
        if (nightStartHour <= eveningStartHour) nightStartHour = Mathf.Min(23, eveningStartHour + 1);

        // Enforce default phase Day if none or lobby
        if (currentPhase == GamePhase.None || currentPhase == GamePhase.Lobby)
        {
            currentPhase = GamePhase.Day;
        }

        // Ensure correct initial phase by time
        var phaseByTime = DeterminePhaseByTime(CurrentHour);
        if (phaseByTime != currentPhase)
        {
            SetPhase(phaseByTime);
        }
    }

    void Update()
    {
        TickGameTime();
        // After time tick, update phase by schedule
        var phaseByTime = DeterminePhaseByTime(CurrentHour);
        if (phaseByTime != currentPhase)
        {
            SetPhase(phaseByTime);
        }
    }

    private void TickGameTime()
    {
        if (timeScale <= 0f) return; // paused
        _currentTimeMinutes += Time.deltaTime * timeScale;
        // wrap around after 24h
        if (_currentTimeMinutes >= 1440f) _currentTimeMinutes %= 1440f;
    }

    private GamePhase DeterminePhaseByTime(int hour)
    {
        // Night: [nightStart, 24) and [0, dayStart)
        if (hour >= nightStartHour || hour < dayStartHour)
            return GamePhase.Night;
        // Evening: [eveningStart, nightStart)
        if (hour >= eveningStartHour)
            return GamePhase.Evening;
        // Otherwise Day
        return GamePhase.Day;
    }

    public void SetTime(int hour, int minute)
    {
        hour = Mathf.Clamp(hour, 0, 23);
        minute = Mathf.Clamp(minute, 0, 59);
        _currentTimeMinutes = hour * 60 + minute;
        // Immediately sync phase after manual change
        var phaseByTime = DeterminePhaseByTime(hour);
        if (phaseByTime != currentPhase)
        {
            SetPhase(phaseByTime);
        }
    }

    public void SetPhase(GamePhase phase)
    {
        currentPhase = phase;
        Debug.Log($"[GameManager] Phase set to {currentPhase}");
    }

    public void AdvancePhase()
    {
        // Cycle: Day -> Evening -> Night -> Day (simplified, End can be set manually)
        switch (currentPhase)
        {
            case GamePhase.Day: currentPhase = GamePhase.Evening; break;
            case GamePhase.Evening: currentPhase = GamePhase.Night; break;
            case GamePhase.Night: currentPhase = GamePhase.Day; break;
            case GamePhase.End: currentPhase = GamePhase.Day; break; // restart after End
            default: currentPhase = GamePhase.Day; break;
        }
        Debug.Log($"[GameManager] Phase advanced to {currentPhase}");
    }

    public void SetPlayerCount(int count)
    {
        if (count < 0) count = 0;
        playerCount = count;
    }

    public void IncrementPlayerCount() => playerCount++;
    public void DecrementPlayerCount() { if (playerCount > 0) playerCount--; }

    // ----- Role Assignment -----
    public void AssignRolesToPlayers()
    {
        var players = Object.FindObjectsByType<Player>(FindObjectsSortMode.None);
        if (players == null || players.Length == 0) return;

        // Sync stored playerCount with actual objects in scene.
        playerCount = players.Length;

        // Build role pool according to rules.
        var pool = BuildRolePool(playerCount);
        rolePoolDebugView = new List<PlayerRole>(pool); // keep a copy for inspector view

        // Shuffle pool for randomness.
        Shuffle(pool);

        int index = 0;
        foreach (var p in players)
        {
            if (index >= pool.Count) { p.SetRole(PlayerRole.Civilian); continue; } // fallback
            p.SetRole(pool[index]);
            index++;
        }
    }   

    // Local testing helper: pick a random role from the pool (does not consume)
    public PlayerRole PickRandomRoleFromPool()
    {
        int count = playerCount;
        // If not set, base on players in scene.
        var players = Object.FindObjectsByType<Player>(FindObjectsSortMode.None);
        if (players != null && players.Length > 0) count = players.Length;
        var pool = BuildRolePool(Mathf.Max(1, count));
        if (pool.Count == 0) return PlayerRole.Civilian;
        int idx = Random.Range(0, pool.Count);
        return pool[idx];
    }

    private List<PlayerRole> BuildRolePool(int totalPlayers)
    {
        var result = new List<PlayerRole>(totalPlayers);
        if (totalPlayers <= 0) return result;

        // Mafia count: round(total * 0.25)
        int mafiaCount = Mathf.RoundToInt(totalPlayers * 0.25f);
        // Detective: 1 if players >= 6
        int detectiveCount = totalPlayers >= 6 ? 1 : 0;
        // Civilians: remaining
        int civiliansCount = Mathf.Max(0, totalPlayers - mafiaCount - detectiveCount);

        // Ensure counts do not exceed total players due to rounding edge cases.
        int totalAllocated = mafiaCount + detectiveCount + civiliansCount;
        if (totalAllocated > totalPlayers)
        {
            // Reduce civilians first if overflow happens.
            int overflow = totalAllocated - totalPlayers;
            civiliansCount = Mathf.Max(0, civiliansCount - overflow);
        }
        else if (totalAllocated < totalPlayers)
        {
            // Add remaining as civilians if under-allocated (should rarely happen)
            civiliansCount += (totalPlayers - totalAllocated);
        }

        for (int i = 0; i < mafiaCount; i++) result.Add(PlayerRole.Murderer); // Using Murderer enum value as Mafia
        for (int i = 0; i < detectiveCount; i++) result.Add(PlayerRole.Detective);
        for (int i = 0; i < civiliansCount; i++) result.Add(PlayerRole.Civilian);

        return result;
    }

    private void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
