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

    /// <summary>The active role pool that gets depleted as roles are assigned.</summary>
    private List<PlayerRole> activeRolePool = new();
    
    /// <summary>Whether the role pool has been initialized.</summary>
    private bool isPoolInitialized = false;

    /// <summary>Gets the current player count.</summary>
    public int PlayerCount => playerCount;
    
    /// <summary>Gets a read-only list of all managed players.</summary>
    public IReadOnlyList<Player> Players => players;
    
    /// <summary>Gets the number of roles remaining in the active pool.</summary>
    public int RemainingRolesInPool => activeRolePool.Count;

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
    /// Initializes the role pool based on the current number of players in the scene.
    /// This should be called once at the start of the game.
    /// </summary>
    public void InitializeRolePool()
    {
        var foundPlayers = Object.FindObjectsByType<Player>(FindObjectsSortMode.None);
        if (foundPlayers == null || foundPlayers.Length == 0)
        {
            Debug.LogWarning("[RoleManager] No players found to initialize role pool");
            return;
        }
        
        players.Clear();
        players.AddRange(foundPlayers);
        playerCount = players.Count;
        
        activeRolePool = BuildRolePool(playerCount);
        rolePoolDebugView = new List<PlayerRole>(activeRolePool);
        Shuffle(activeRolePool);
        
        isPoolInitialized = true;
        
        Debug.Log($"[RoleManager] Initialized role pool for {playerCount} players with {activeRolePool.Count} roles");
    }

    /// <summary>
    /// Initializes the role pool for a specific list of players.
    /// </summary>
    /// <param name="playerList">The list of players to initialize the pool for.</param>
    public void InitializeRolePool(List<Player> playerList)
    {
        if (playerList == null || playerList.Count == 0)
        {
            Debug.LogWarning("[RoleManager] No players provided to initialize role pool");
            return;
        }
        
        players.Clear();
        players.AddRange(playerList);
        playerCount = players.Count;
        
        activeRolePool = BuildRolePool(playerCount);
        rolePoolDebugView = new List<PlayerRole>(activeRolePool);
        Shuffle(activeRolePool);
        
        isPoolInitialized = true;
        
        Debug.Log($"[RoleManager] Initialized role pool for {playerCount} players with {activeRolePool.Count} roles");
    }

    /// <summary>
    /// Assigns unique roles from the active pool to all <see cref="Player"/> instances in the scene.
    /// The pool must be initialized first using <see cref="InitializeRolePool"/>.
    /// </summary>
    public void AssignRolesToPlayers()
    {
        if (!isPoolInitialized)
        {
            Debug.LogWarning("[RoleManager] Role pool not initialized. Call InitializeRolePool first.");
            InitializeRolePool();
        }
        
        if (players == null || players.Count == 0)
        {
            Debug.LogWarning("[RoleManager] No players to assign roles to");
            return;
        }
        
        foreach (var p in players)
        {
            if (p == null) continue;
            
            PlayerRole assignedRole = TakeRoleFromPool();
            p.SetRole(assignedRole);
        }
        
        Debug.Log($"[RoleManager] Assigned roles to {players.Count} players. {activeRolePool.Count} roles remaining in pool");
    }

    /// <summary>
    /// Assigns roles to a specific list of players from the active pool.
    /// The pool must be initialized first using <see cref="InitializeRolePool"/>.
    /// </summary>
    /// <param name="playerList">The list of players to assign roles to.</param>
    public void AssignRoles(List<Player> playerList)
    {
        if (!isPoolInitialized)
        {
            Debug.LogWarning("[RoleManager] Role pool not initialized. Initializing with provided player list.");
            InitializeRolePool(playerList);
        }
        
        if (playerList == null || playerList.Count == 0)
        {
            Debug.LogWarning("[RoleManager] No players to assign roles to");
            return;
        }
        
        foreach (var p in playerList)
        {
            if (p == null) continue;
            
            PlayerRole assignedRole = TakeRoleFromPool();
            p.SetRole(assignedRole);
        }
        
        Debug.Log($"[RoleManager] Assigned roles to {playerList.Count} players. {activeRolePool.Count} roles remaining in pool");
    }

    /// <summary>
    /// Takes and removes a role from the active pool.
    /// If the pool is empty, returns Civilian as a fallback.
    /// </summary>
    /// <returns>A <see cref="PlayerRole"/> from the pool.</returns>
    private PlayerRole TakeRoleFromPool()
    {
        if (activeRolePool.Count == 0)
        {
            Debug.LogWarning("[RoleManager] Role pool is empty, assigning Civilian as fallback");
            return PlayerRole.Civilian;
        }
        
        PlayerRole role = activeRolePool[0];
        activeRolePool.RemoveAt(0);
        return role;
    }

    /// <summary>
    /// Picks a random role from the role pool based on the current player count.
    /// This method takes a role from the active pool and does not return it.
    /// </summary>
    /// <returns>A random <see cref="PlayerRole"/>.</returns>
    public PlayerRole PickRandomRoleFromPool()
    {
        if (!isPoolInitialized)
        {
            Debug.LogWarning("[RoleManager] Role pool not initialized. Initializing based on scene players.");
            InitializeRolePool();
        }
        
        if (activeRolePool.Count == 0)
        {
            Debug.LogWarning("[RoleManager] Role pool is empty, returning Civilian as fallback");
            return PlayerRole.Civilian;
        }
        
        int idx = Random.Range(0, activeRolePool.Count);
        PlayerRole role = activeRolePool[idx];
        activeRolePool.RemoveAt(idx);
        
        Debug.Log($"[RoleManager] Picked {role} from pool. {activeRolePool.Count} roles remaining");
        return role;
    }

    /// <summary>
    /// Resets the role pool, allowing roles to be reassigned.
    /// This rebuilds the pool based on current player count.
    /// </summary>
    public void ResetRolePool()
    {
        if (players.Count > 0)
        {
            activeRolePool = BuildRolePool(players.Count);
        }
        else
        {
            activeRolePool = BuildRolePool(playerCount);
        }
        
        rolePoolDebugView = new List<PlayerRole>(activeRolePool);
        Shuffle(activeRolePool);
        isPoolInitialized = true;
        
        Debug.Log($"[RoleManager] Reset role pool with {activeRolePool.Count} roles");
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
