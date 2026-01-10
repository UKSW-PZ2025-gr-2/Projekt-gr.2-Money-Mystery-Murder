using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages player statistics tracking and synchronization with the backend database.
/// Tracks kills, money earned, wins, and total games for each player.
/// </summary>
public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager Instance { get; private set; }

    [Header("API Configuration")]
    [SerializeField] private string apiBaseUrl = "http://localhost:5100";
    [SerializeField] private bool enableStatTracking = true;

    [Header("Debug")]
    [SerializeField] private bool debugMode = false;

    private Dictionary<string, PlayerStatistics> playerStats = new Dictionary<string, PlayerStatistics>();
    private bool isSavingStats = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Initialize statistics for a player at the start of a game.
    /// </summary>
    public void InitializePlayer(string playerName)
    {
        if (!enableStatTracking) return;
        
        if (!playerStats.ContainsKey(playerName))
        {
            playerStats[playerName] = new PlayerStatistics(playerName);
            Log($"Initialized stats for player: {playerName}");
        }
    }

    /// <summary>
    /// Record a kill by a player.
    /// </summary>
    public void RecordKill(string killerName)
    {
        if (!enableStatTracking || string.IsNullOrEmpty(killerName)) return;

        InitializePlayer(killerName);
        playerStats[killerName].Kills++;
        Log($"{killerName} kills: {playerStats[killerName].Kills}");
    }

    /// <summary>
    /// Record money earned by a player.
    /// </summary>
    public void RecordMoneyEarned(string playerName, long amount)
    {
        if (!enableStatTracking || string.IsNullOrEmpty(playerName) || amount <= 0) return;

        InitializePlayer(playerName);
        playerStats[playerName].Money += amount;
        Log($"{playerName} total money: {playerStats[playerName].Money}");
    }

    /// <summary>
    /// Mark the game as complete and record the winner.
    /// </summary>
    public void RecordGameEnd(string winningTeam, List<Player> allPlayers)
    {
        if (!enableStatTracking) return;

        foreach (var player in allPlayers)
        {
            if (player == null) continue;

            string playerName = player.gameObject.name;
            InitializePlayer(playerName);

            var stats = playerStats[playerName];
            stats.TotalGames++;

            // Determine if this player won based on their role and winning team
            bool playerWon = IsPlayerOnWinningTeam(player, winningTeam);
            if (playerWon)
            {
                stats.Wins++;
            }

            // Add final money balance
            stats.Money += player.Balance;

            Log($"{playerName} - Games: {stats.TotalGames}, Wins: {stats.Wins}, Kills: {stats.Kills}, Money: {stats.Money}");
        }

        // Save all stats to database
        StartCoroutine(SaveAllStatsToDatabase());
    }

    /// <summary>
    /// Determine if a player is on the winning team.
    /// </summary>
    private bool IsPlayerOnWinningTeam(Player player, string winningTeam)
    {
        if (winningTeam.Equals("Innocents", System.StringComparison.OrdinalIgnoreCase))
        {
            return player.Role == PlayerRole.Civilian || player.Role == PlayerRole.Detective;
        }
        else if (winningTeam.Equals("Murderers", System.StringComparison.OrdinalIgnoreCase))
        {
            return player.Role == PlayerRole.Murderer;
        }
        return false;
    }

    /// <summary>
    /// Save all tracked player statistics to the database.
    /// </summary>
    private IEnumerator SaveAllStatsToDatabase()
    {
        if (isSavingStats)
        {
            Log("Already saving stats, skipping...");
            yield break;
        }

        isSavingStats = true;
        Log("Starting to save all player stats to database...");

        foreach (var kvp in playerStats)
        {
            yield return StartCoroutine(UpdateOrCreatePlayerStats(kvp.Value));
            yield return new WaitForSeconds(0.1f); // Small delay between requests
        }

        Log("Finished saving all player stats to database.");
        isSavingStats = false;
    }

    /// <summary>
    /// Update or create player statistics in the database.
    /// </summary>
    private IEnumerator UpdateOrCreatePlayerStats(PlayerStatistics stats)
    {
        // First, check if player exists by trying to GET by name
        string getByNameUrl = $"{apiBaseUrl}/players/{UnityWebRequest.EscapeURL(stats.Name)}";
        
        using (UnityWebRequest getRequest = UnityWebRequest.Get(getByNameUrl))
        {
            yield return getRequest.SendWebRequest();

            if (getRequest.result == UnityWebRequest.Result.Success)
            {
                // Player exists, use PUT to update (accumulative)
                Log($"Player {stats.Name} exists, updating stats...");
                yield return StartCoroutine(UpdatePlayerStats(stats));
            }
            else if (getRequest.responseCode == 404)
            {
                // Player doesn't exist, create new record
                Log($"Player {stats.Name} doesn't exist, creating new record...");
                yield return StartCoroutine(CreatePlayerStats(stats));
            }
            else
            {
                LogError($"Failed to check if player exists: {getRequest.error}");
                // Try to create anyway as fallback
                yield return StartCoroutine(CreatePlayerStats(stats));
            }
        }
    }

    /// <summary>
    /// Update existing player stats in the database (accumulative).
    /// </summary>
    private IEnumerator UpdatePlayerStats(PlayerStatistics stats)
    {
        string url = $"{apiBaseUrl}/players/{UnityWebRequest.EscapeURL(stats.Name)}";
        
        // Create DTO with delta values (to be added to existing stats)
        var playerData = new PlayerStatsDTO
        {
            Name = stats.Name,
            Kills = stats.Kills,
            Money = stats.Money,
            Wins = stats.Wins,
            TotalGames = stats.TotalGames
        };

        string json = JsonUtility.ToJson(playerData);
        Log($"Updating player data: {json}");

        using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Log($"Successfully updated stats for {stats.Name}. Response: {request.downloadHandler.text}");
            }
            else
            {
                LogError($"Failed to update stats for {stats.Name}: {request.error} (Code: {request.responseCode})");
                // Fallback to create if update fails
                if (request.responseCode == 404)
                {
                    Log("Falling back to CREATE...");
                    yield return StartCoroutine(CreatePlayerStats(stats));
                }
            }
        }
    }

    /// <summary>
    /// Create a new player record in the database.
    /// </summary>
    private IEnumerator CreatePlayerStats(PlayerStatistics stats)
    {
        string url = $"{apiBaseUrl}/players";
        
        // Calculate win rate
        float winRate = stats.TotalGames > 0 ? ((float)stats.Wins / stats.TotalGames) * 100f : 0f;
        
        // Create DTO matching backend model
        var playerData = new PlayerStatsDTO
        {
            Name = stats.Name,
            Kills = stats.Kills,
            Money = stats.Money,
            Wins = stats.Wins,
            TotalGames = stats.TotalGames
        };

        string json = JsonUtility.ToJson(playerData);
        Log($"Sending player data: {json}");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Log($"Successfully saved stats for {stats.Name}. Response: {request.downloadHandler.text}");
            }
            else
            {
                LogError($"Failed to save stats for {stats.Name}: {request.error} (Code: {request.responseCode})");
            }
        }
    }

    /// <summary>
    /// Get statistics for a specific player.
    /// </summary>
    public PlayerStatistics GetPlayerStats(string playerName)
    {
        if (playerStats.ContainsKey(playerName))
        {
            return playerStats[playerName];
        }
        return null;
    }

    /// <summary>
    /// Reset all tracked statistics (for testing).
    /// </summary>
    public void ResetAllStats()
    {
        playerStats.Clear();
        Log("All player statistics reset.");
    }

    private void Log(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[PlayerStatsManager] {message}");
        }
    }

    private void LogError(string message)
    {
        Debug.LogError($"[PlayerStatsManager] {message}");
    }

    /// <summary>
    /// Data class to hold player statistics.
    /// </summary>
    public class PlayerStatistics
    {
        public string Name;
        public int Kills;
        public long Money;
        public int Wins;
        public int TotalGames;

        public PlayerStatistics(string name)
        {
            Name = name;
            Kills = 0;
            Money = 0;
            Wins = 0;
            TotalGames = 0;
        }
    }

    /// <summary>
    /// DTO matching backend PlayerStats model.
    /// </summary>
    [System.Serializable]
    public class PlayerStatsDTO
    {
        public string Name;
        public int Kills;
        public long Money;
        public int Wins;
        public int TotalGames;
    }
}
