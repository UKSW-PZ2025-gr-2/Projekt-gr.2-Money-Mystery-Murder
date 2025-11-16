using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int playerCount = 1; // current number of players
    [SerializeField] private List<PlayerRole> rolePoolDebugView = new(); // debug: shows generated pool

    public int PlayerCount => playerCount;

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
        // At game start generate and assign roles to all players present in the scene.
        AssignRolesToPlayers();
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
