using UnityEngine;
using System.Collections.Generic;

public class StatisticsRepository : MonoBehaviour
{
    public static StatisticsRepository Instance { get; private set; }

    [Header("Cached Totals (Session)")]
    public int Kills;
    public int MoneyEarned;
    public int MatchesWon;
    public int MatchesPlayed;

    [Header("Per-Player Cache")] 
    [SerializeField] private Dictionary<string, PlayerStats> playerStatsCache = new();

    private class PlayerStats
    {
        public int Kills;
        public int MoneyEarned;
        public int MatchesWon;
        public int MatchesPlayed;
    }

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
        // Optionally preload stats from DatabaseManager
        // LoadAllStats();
    }

    public void SaveMatchResult()
    {
        // 1. Increment MatchesPlayed
        // 2. Update session totals (Kills/MoneyEarned etc.)
        // 3. Persist to DB (DatabaseManager.ExecuteQuery)
        // 4. Invalidate or refresh cache
        throw new System.NotImplementedException();
    }

    public void GetPlayerStats(string playerId)
    {
        // 1. Check cache for playerId
        // 2. If not found, query DB and populate
        // 3. Return or expose via event/callback
        throw new System.NotImplementedException();
    }

    private void LoadAllStats()
    {
        // 1. Query DB for all player stats
        // 2. Populate playerStatsCache
        // 3. Aggregate session totals
        throw new System.NotImplementedException();
    }
}
