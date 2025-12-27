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
    [Tooltip("API base URL (include /players)")]
    [SerializeField] private string baseUrl = "http://localhost:5100/players";

    [Header("Sequential Seeding")]
    [Tooltip("If >0, seed this many sequential players named using Base Name and index (e.g. gracz(1))")]
    [SerializeField] private int sequentialCount = 0;

    [Tooltip("Base name for sequential players (e.g. 'gracz' will create gracz(1), gracz(2), ...)")]
    [SerializeField] private string sequentialBaseName = "gracz";

    // Sample players to seed
    private List<PlayerStats> _samples = new List<PlayerStats>()
    {
        new PlayerStats("PlayerOne", 5, 1000, 1, 3),
        new PlayerStats("PlayerTwo", 2, 250, 0, 1),
        new PlayerStats("TestUser", 0, 500, 0, 0)
    };

    private void Start()
    {
        StartCoroutine(SeedRoutine());

        if (sequentialCount > 0)
        {
            StartCoroutine(SeedSequential(sequentialBaseName, sequentialCount));
        }
    }

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

[System.Serializable]
public class PlayerStats
{
    public string Name;
    public int Kills;
    public long Money;
    public int Wins;
    public int TotalGames;

    public PlayerStats(string name, int kills, long money, int wins, int totalGames)
    {
        Name = name;
        Kills = kills;
        Money = money;
        Wins = wins;
        TotalGames = totalGames;
    }
}
