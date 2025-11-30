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

    [Header("Systems")]
    [SerializeField] private RoleManager roleManager;
    [SerializeField] private PhaseManager phaseManager;

    [Header("Game Phase")]
    [SerializeField] private GamePhase currentPhase = GamePhase.Day; // default phase is Day

    [Header("Phase Schedule (Hours)")]
    [Range(0,23)] [SerializeField] private int dayStartHour = 6;
    [Range(0,23)] [SerializeField] private int eveningStartHour = 18;
    [Range(0,23)] [SerializeField] private int nightStartHour = 21;

    [Header("Game Time")]
    [SerializeField] private int startHour = 6; // game starts at 06:00 by default
    [SerializeField] private int startMinute = 0;
    [SerializeField] private float timeScale = 1f; // in-game minutes progressed per real-time second
    private float _currentTimeMinutes; // minutes since midnight (0..1439)

    public int PlayerCount => playerCount;
    public float CurrentTimeMinutes => _currentTimeMinutes;
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

    void Update()
    {
        TickGameTime();
        var phaseByTime = DeterminePhaseByTime(CurrentHour);
        if (phaseByTime != currentPhase)
        {
            SetPhase(phaseByTime);
        }
    }

    private void TickGameTime()
    {
        if (timeScale <= 0f) return;
        _currentTimeMinutes += Time.deltaTime * timeScale;
        if (_currentTimeMinutes >= 1440f) _currentTimeMinutes %= 1440f;
    }

    private GamePhase DeterminePhaseByTime(int hour)
    {
        if (hour >= nightStartHour || hour < dayStartHour) return GamePhase.Night;
        if (hour >= eveningStartHour) return GamePhase.Evening;
        return GamePhase.Day;
    }

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

    public void SetPhase(GamePhase phase)
    {
        currentPhase = phase;
        Debug.Log($"[GameManager] Phase set to {currentPhase}");
    }

    public void StartGame()
    {
        // TODO: Logic
        throw new System.NotImplementedException();
    }

    public void CheckWinCondition()
    {
        // TODO: Logic
        throw new System.NotImplementedException();
    }

    public void ChangePhase(GamePhase newState)
    {
        // TODO: Logic
        throw new System.NotImplementedException();
    }

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

    public void SetPlayerCount(int count)
    {
        if (count < 0) count = 0;
        playerCount = count;
    }

    public void IncrementPlayerCount() => playerCount++;
    public void DecrementPlayerCount() { if (playerCount > 0) playerCount--; }

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

    private void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
