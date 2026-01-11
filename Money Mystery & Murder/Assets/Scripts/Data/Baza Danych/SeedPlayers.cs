using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Seeds the backend with sample player records for testing/login.
/// PUT this script on any active GameObject and set `baseUrl` if needed.
/// It will POST several sample players sequentially at Start().
/// </summary>
public class SeedPlayers : MonoBehaviour
{
    /// <summary>
    /// API base URL for the players endpoint.
    /// </summary>
    [Tooltip("API base URL (include /players)")]
    [SerializeField] private string baseUrl = "http://localhost:5100/players";

    [Header("Sequential Seeding")]
    /// <summary>
    /// If greater than 0, seed this many sequential players named using Base Name and index (e.g. gracz(1)).
    /// </summary>
    [Tooltip("If >0, seed this many sequential players named using Base Name and index (e.g. gracz(1))")]
    [SerializeField] private int sequentialCount = 0;

    /// <summary>
    /// Base name for sequential players (e.g. 'gracz' will create gracz(1), gracz(2), ...).
    /// </summary>
    [Tooltip("Base name for sequential players (e.g. 'gracz' will create gracz(1), gracz(2), ...)")]
    [SerializeField] private string sequentialBaseName = "gracz";

    /// <summary>
    /// Sample players to seed into the database.
    /// </summary>
    private List<PlayerStats> _samples = new List<PlayerStats>()
    {
        new PlayerStats("PlayerOne", 5, 1000, 1, 3),
        new PlayerStats("PlayerTwo", 2, 250, 0, 1),
        new PlayerStats("TestUser", 0, 500, 0, 0)
    };

    /// <summary>
    /// Unity lifecycle method called before the first frame update.
    /// Initiates the seeding process for sample and sequential players.
    /// </summary>
    private void Start()
    {
        StartCoroutine(SeedRoutine());

        if (sequentialCount > 0)
        {
            StartCoroutine(SeedSequential(sequentialBaseName, sequentialCount));
        }
    }

    /// <summary>
    /// Coroutine that seeds all sample players sequentially.
    /// </summary>
    /// <returns>IEnumerator for coroutine execution.</returns>
    private IEnumerator SeedRoutine()
    {
        for (int i = 0; i < _samples.Count; i++)
        {
            yield return StartCoroutine(EnsureAndPost(_samples[i]));
            // small delay between requests
            yield return new WaitForSeconds(0.2f);
        }

        Debug.Log($"[SeedPlayers] Seeding finished ({_samples.Count} players attempted)");
    }

    /// <summary>
    /// Coroutine that creates and seeds sequential players with numbered names.
    /// </summary>
    /// <param name="baseName">The base name for the sequential players.</param>
    /// <param name="count">The number of sequential players to create.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
    private IEnumerator SeedSequential(string baseName, int count)
    {
        for (int i = 1; i <= count; i++)
        {
            string name = baseName + "(" + i + ")";
            var p = new PlayerStats(name, 0, 0, 0, 0);
            yield return StartCoroutine(EnsureAndPost(p));
            yield return new WaitForSeconds(0.15f);
        }
        Debug.Log($"[SeedPlayers] Sequential seeding finished ({count} players)");
    }

    /// <summary>
    /// Check if player already exists (by Name) by fetching all players and searching the response string.
    /// If not found, POST the player.
    /// This is a simple, server-agnostic check useful for seeding during development.
    /// </summary>
    /// <param name="p">The PlayerStats to check and potentially add.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
    private IEnumerator EnsureAndPost(PlayerStats p)
    {
        using (UnityWebRequest getReq = UnityWebRequest.Get(baseUrl))
        {
            yield return getReq.SendWebRequest();

            if (getReq.result == UnityWebRequest.Result.Success)
            {
                string resp = getReq.downloadHandler.text;
                if (!string.IsNullOrEmpty(resp) && resp.Contains($"\"Name\":\"{p.Name}\""))
                {
                    Debug.Log($"[SeedPlayers] Player '{p.Name}' already exists — skipping POST.");
                    yield break;
                }
            }
            else
            {
                Debug.LogWarning($"[SeedPlayers] Could not GET players to check existence: {getReq.error} — will attempt POST anyway.");
            }
        }

        yield return StartCoroutine(PostPlayer(p));
    }

    /// <summary>
    /// Sends a POST request to add a player to the database.
    /// </summary>
    /// <param name="p">The PlayerStats to add.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
    private IEnumerator PostPlayer(PlayerStats p)
    {
        string json = JsonUtility.ToJson(p);

        using (UnityWebRequest req = new UnityWebRequest(baseUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[SeedPlayers] Added player: {p.Name}  Response: {req.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"[SeedPlayers] Failed to add {p.Name}: {req.error} ({req.responseCode})");
            }
        }
    }
}

/// <summary>
/// Data structure representing player statistics for database storage.
/// </summary>
[System.Serializable]
public class PlayerStats
{
    /// <summary>
    /// The unique name/identifier of the player.
    /// </summary>
    public string Name;
    
    /// <summary>
    /// Total number of kills by this player.
    /// </summary>
    public int Kills;
    
    /// <summary>
    /// Total money/currency accumulated by this player.
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
    /// Initializes a new instance of PlayerStats.
    /// </summary>
    /// <param name="name">Player name.</param>
    /// <param name="kills">Kill count.</param>
    /// <param name="money">Money accumulated.</param>
    /// <param name="wins">Wins count.</param>
    /// <param name="totalGames">Total games played.</param>
    public PlayerStats(string name, int kills, long money, int wins, int totalGames)
    {
        Name = name;
        Kills = kills;
        Money = money;
        Wins = wins;
        TotalGames = totalGames;
    }
}
