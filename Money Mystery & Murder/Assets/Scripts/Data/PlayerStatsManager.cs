using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages player statistics tracking and synchronization with the backend database.
/// Tracks kills, money earned, wins, and total games for each player.
/// Automatically saves statistics to the database at the end of each game.
/// </summary>
public class PlayerStatsManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the PlayerStatsManager.
    /// </summary>
    public static PlayerStatsManager Instance { get; private set; }

    [Header("API Configuration")]
    /// <summary>
    /// Base URL for the backend API endpoint.
    /// </summary>
    [SerializeField] private string apiBaseUrl = "http://localhost:5100";
    
    /// <summary>
    /// Whether to enable statistics tracking.
    /// </summary>
    [SerializeField] private bool enableStatTracking = true;

    [Header("Debug")]
    /// <summary>
    /// Whether to enable debug logging.
    /// </summary>
    [SerializeField] private bool debugMode = false;

    /// <summary>
    /// Dictionary mapping player names to their statistics.
    /// </summary>
    private Dictionary<string, PlayerStatistics> playerStats = new Dictionary<string, PlayerStatistics>();
    
    /// <summary>
    /// Flag indicating whether statistics are currently being saved to prevent concurrent saves.
    /// </summary>
    private bool isSavingStats = false;

    /// <summary>
    /// Unity lifecycle method called when the script instance is being loaded.
    /// Initializes the singleton instance.
    /// </summary>
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
    /// <param name="playerName">The name of the player to initialize.</param>
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
    /// <param name="killerName">The name of the player who made the kill.</param>
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
    /// <param name="playerName">The name of the player.</param>
    /// <param name="amount">The amount of money earned.</param>
    public void RecordMoneyEarned(string playerName, long amount)
    {
        if (!enableStatTracking || string.IsNullOrEmpty(playerName) || amount <= 0) return;

        InitializePlayer(playerName);
        playerStats[playerName].Money += amount;
        Log($"{playerName} total money: {playerStats[playerName].Money}");
    }

    /// <summary>
    /// Mark the game as complete and record the winner.
    /// Saves all player statistics to the database.
    /// </summary>
    /// <param name="winningTeam">The name of the winning team.</param>
    /// <param name="allPlayers">List of all players in the game.</param>
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
    /// <param name="player">The player to check.</param>
    /// <param name="winningTeam">The name of the winning team.</param>
    /// <returns>True if the player is on the winning team, false otherwise.</returns>
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
    /// <returns>IEnumerator for coroutine execution.</returns>
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
    /// First checks if player exists, then updates or creates accordingly.
    /// </summary>
    /// <param name="stats">The player statistics to save.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
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
    /// <param name="stats">The player statistics to update.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
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
    /// <param name="stats">The player statistics to create.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
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
    /// <param name="playerName">The name of the player.</param>
    /// <returns>The player's statistics, or null if not found.</returns>
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

    /// <summary>
    /// Log a debug message if debug mode is enabled.
    /// </summary>
    /// <param name="message">The message to log.</param>
    private void Log(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[PlayerStatsManager] {message}");
        }
    }

    /// <summary>
    /// Log an error message.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    private void LogError(string message)
    {
        Debug.LogError($"[PlayerStatsManager] {message}");
    }

    /// <summary>
    /// Data class to hold player statistics during a game session.
    /// </summary>
    public class PlayerStatistics
    {
        /// <summary>
        /// The player's name.
        /// </summary>
        public string Name;
        
        /// <summary>
        /// Total number of kills by this player.
        /// </summary>
        public int Kills;
        
        /// <summary>
        /// Total money accumulated by this player.
        /// </summary>
        public long Money;
        
        /// <summary>
        /// Number of games won by this player.
        /// </summary>
        public int Wins;
        
        /// <summary>
        /// Total number of games played by this player.
        /// </summary>
        public int TotalGames;

        /// <summary>
        /// Initializes a new instance of PlayerStatistics.
        /// </summary>
        /// <param name="name">The player's name.</param>
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
    /// Data Transfer Object matching the backend PlayerStats model.
    /// Used for serialization when communicating with the API.
    /// </summary>
    [System.Serializable]
    public class PlayerStatsDTO
    {
        /// <summary>
        /// The player's name.
        /// </summary>
        public string Name;
        
        /// <summary>
        /// Total number of kills.
        /// </summary>
        public int Kills;
        
        /// <summary>
        /// Total money accumulated.
        /// </summary>
        public long Money;
        
        /// <summary>
        /// Number of wins.
        /// </summary>
        public int Wins;
        
        /// <summary>
        /// Total number of games played.
        /// </summary>
        public int TotalGames;
    }
}
