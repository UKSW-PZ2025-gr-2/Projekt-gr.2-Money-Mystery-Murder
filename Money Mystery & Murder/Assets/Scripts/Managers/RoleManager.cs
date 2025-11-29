using System.Collections.Generic;
using UnityEngine;

public class RoleManager : MonoBehaviour
{
    /// <summary>Current number of players in the game. Set this in the Unity Inspector.</summary>
    [SerializeField] private int playerCount = 1;
    
    /// <summary>Debug view of the generated role pool. Set this in the Unity Inspector.</summary>
    [SerializeField] private List<PlayerRole> rolePoolDebugView = new();
    
    /// <summary>List of all players being managed. Set this in the Unity Inspector.</summary>
    [SerializeField] private List<Player> players = new();

    /// <summary>Gets the current player count.</summary>
    public int PlayerCount => playerCount;
    
    /// <summary>Gets a read-only list of all managed players.</summary>
    public IReadOnlyList<Player> Players => players;

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
    public void DecrementPlayerCount() 
    { 
        if (playerCount > 0) playerCount--; 
    }

    /// <summary>
    /// Assigns unique roles from a generated pool to all <see cref="Player"/> instances in the scene.
    /// </summary>
    public void AssignRolesToPlayers()
    {
        var foundPlayers = Object.FindObjectsByType<Player>(FindObjectsSortMode.None);
        if (foundPlayers == null || foundPlayers.Length == 0) return;
        
        players.Clear();
        players.AddRange(foundPlayers);
        playerCount = players.Count;
        
        var pool = BuildRolePool(playerCount);
        rolePoolDebugView = new List<PlayerRole>(pool);
        Shuffle(pool);
        
        int index = 0;
        foreach (var p in players)
        {
            if (index >= pool.Count) 
            { 
                p.SetRole(PlayerRole.Civilian); 
                continue; 
            }
            p.SetRole(pool[index]);
            index++;
        }
        
        Debug.Log($"[RoleManager] Assigned roles to {players.Count} players");
    }

    /// <summary>
    /// Assigns roles to a specific list of players.
    /// </summary>
    /// <param name="playerList">The list of players to assign roles to.</param>
    public void AssignRoles(List<Player> playerList)
    {
        if (playerList == null || playerList.Count == 0) return;
        
        players.Clear();
        players.AddRange(playerList);
        playerCount = players.Count;
        
        var pool = BuildRolePool(playerCount);
        rolePoolDebugView = new List<PlayerRole>(pool);
        Shuffle(pool);
        
        int index = 0;
        foreach (var p in players)
        {
            if (index >= pool.Count) 
            { 
                p.SetRole(PlayerRole.Civilian); 
                continue; 
            }
            p.SetRole(pool[index]);
            index++;
        }
        
        Debug.Log($"[RoleManager] Assigned roles to {players.Count} players");
    }

    /// <summary>
    /// Picks a random role from the role pool based on the current player count.
    /// </summary>
    /// <returns>A random <see cref="PlayerRole"/>.</returns>
    public PlayerRole PickRandomRoleFromPool()
    {
        int count = playerCount;
        var foundPlayers = Object.FindObjectsByType<Player>(FindObjectsSortMode.None);
        if (foundPlayers != null && foundPlayers.Length > 0) 
        {
            count = foundPlayers.Length;
        }
        
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
    public List<PlayerRole> BuildRolePool(int totalPlayers)
    {
        var result = new List<PlayerRole>(totalPlayers);
        if (totalPlayers <= 0) return result;
        
        int murdererCount = Mathf.RoundToInt(totalPlayers * 0.25f);
        int detectiveCount = totalPlayers >= 6 ? 1 : 0;
        int civiliansCount = Mathf.Max(0, totalPlayers - murdererCount - detectiveCount);
        
        int totalAllocated = murdererCount + detectiveCount + civiliansCount;
        if (totalAllocated > totalPlayers)
        {
            int overflow = totalAllocated - totalPlayers;
            civiliansCount = Mathf.Max(0, civiliansCount - overflow);
        }
        else if (totalAllocated < totalPlayers)
        {
            civiliansCount += (totalPlayers - totalAllocated);
        }
        
        for (int i = 0; i < murdererCount; i++) result.Add(PlayerRole.Murderer);
        for (int i = 0; i < detectiveCount; i++) result.Add(PlayerRole.Detective);
        for (int i = 0; i < civiliansCount; i++) result.Add(PlayerRole.Civilian);
        
        return result;
    }

    /// <summary>
    /// Gets the team name for a given role.
    /// </summary>
    /// <param name="role">The <see cref="PlayerRole"/> to get the team for.</param>
    /// <returns>The team name as a string.</returns>
    public string GetRoleTeam(PlayerRole role)
    {
        return role switch
        {
            PlayerRole.Murderer => "Murderers",
            PlayerRole.Detective => "Innocents",
            PlayerRole.Civilian => "Innocents",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Gets all players with a specific role.
    /// </summary>
    /// <param name="role">The role to filter by.</param>
    /// <returns>A list of players with the specified role.</returns>
    public List<Player> GetPlayersByRole(PlayerRole role)
    {
        var result = new List<Player>();
        foreach (var player in players)
        {
            if (player != null && player.Role == role)
            {
                result.Add(player);
            }
        }
        return result;
    }

    /// <summary>
    /// Counts how many players have a specific role.
    /// </summary>
    /// <param name="role">The role to count.</param>
    /// <returns>The number of players with the specified role.</returns>
    public int CountPlayersByRole(PlayerRole role)
    {
        int count = 0;
        foreach (var player in players)
        {
            if (player != null && player.Role == role)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Counts how many players are still alive with a specific role.
    /// </summary>
    /// <param name="role">The role to count.</param>
    /// <returns>The number of alive players with the specified role.</returns>
    public int CountAlivePlayersByRole(PlayerRole role)
    {
        int count = 0;
        foreach (var player in players)
        {
            if (player != null && player.Role == role && player.IsAlive)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Checks if all players of a specific role are dead.
    /// </summary>
    /// <param name="role">The role to check.</param>
    /// <returns>True if all players with the role are dead.</returns>
    public bool AreAllRolePlayersDead(PlayerRole role)
    {
        return CountAlivePlayersByRole(role) == 0;
    }

    /// <summary>
    /// Refreshes the player list by finding all Player instances in the scene.
    /// </summary>
    public void RefreshPlayerList()
    {
        var foundPlayers = Object.FindObjectsByType<Player>(FindObjectsSortMode.None);
        players.Clear();
        if (foundPlayers != null && foundPlayers.Length > 0)
        {
            players.AddRange(foundPlayers);
        }
        playerCount = players.Count;
        Debug.Log($"[RoleManager] Refreshed player list: {playerCount} players found");
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
