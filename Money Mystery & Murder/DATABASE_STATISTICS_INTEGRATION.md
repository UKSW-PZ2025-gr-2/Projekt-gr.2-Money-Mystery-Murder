# Database Statistics Integration - Setup Guide

## Overview
The game now tracks player statistics during gameplay and saves them to the MySQL database when the game ends.

## What's Tracked

### During Gameplay:
- **Kills** - When a player kills another player (via melee, ranged, projectiles, or grenades)
- **Money Earned** - When players earn money (from minigames, rewards, etc.)

### At Game End:
- **Wins** - Players on the winning team get a win recorded
- **Total Games** - All players get their game count incremented
- **Win Rate** - Automatically calculated as (Wins / TotalGames) * 100

## Components Added

### 1. PlayerStatsManager.cs
**Location**: `Assets/Scripts/Data/PlayerStatsManager.cs`

This singleton manager:
- Tracks all player statistics during gameplay
- Saves stats to database when game ends
- Uses UPDATE endpoint if player exists, CREATE if new
- Handles API communication with the backend

**Setup**:
1. Create an empty GameObject in your game scene
2. Name it "PlayerStatsManager"
3. Add the `PlayerStatsManager` component
4. Configure settings:
   - **API Base URL**: `http://localhost:5100` (default)
   - **Enable Stat Tracking**: Check this box
   - **Debug Mode**: Enable for detailed logs

### 2. Code Changes

#### Player.cs
- `TakeDamage()` now accepts optional `attacker` parameter
- `Die()` records kills to PlayerStatsManager
- `AddBalance()` records money earned

#### WeaponSystem.cs
- Passes `owner` as attacker in melee and ranged attacks

#### Projectile.cs & GrenadeProjectile.cs
- Added `ownerPlayer` field to track who fired/threw
- Passes attacker to `TakeDamage()`

#### GameManager.cs
- Calls `PlayerStatsManager.RecordGameEnd()` when game ends
- Passes winning team and all players for stat recording

#### RoleManager.cs
- Added `GetAllPlayers()` method for stats collection

## Backend Updates Required

**?? IMPORTANT**: Update your `UnityBackend/Program.cs` file with these endpoints:

### 1. Add GET by Name Endpoint
```csharp
app.MapGet("/players/{name}", (string name) =>
{
    using (var conn = new MySqlConnection(connStr))
    {
        conn.Open();
        string sql = "SELECT * FROM players WHERE Name = @Name LIMIT 1";
        using (var cmd = new MySqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@Name", name);
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    var player = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                        player[reader.GetName(i)] = reader[i];
                    return Results.Json(player);
                }
            }
        }
    }
    return Results.NotFound($"Player '{name}' not found");
});
```

### 2. Update POST Endpoint (Add WinRate)
```csharp
app.MapPost("/players", (PlayerStats player) =>
{
    using (var conn = new MySqlConnection(connStr))
    {
        conn.Open();
        
        // Calculate win rate
        float winRate = player.TotalGames > 0 ? ((float)player.Wins / player.TotalGames) * 100f : 0f;
        
        string sql = "INSERT INTO players (Name, Kills, Money, Wins, TotalGames, WinRate) VALUES (@Name, @Kills, @Money, @Wins, @TotalGames, @WinRate)";
        using (var cmd = new MySqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@Name", player.Name);
            cmd.Parameters.AddWithValue("@Kills", player.Kills);
            cmd.Parameters.AddWithValue("@Money", player.Money);
            cmd.Parameters.AddWithValue("@Wins", player.Wins);
            cmd.Parameters.AddWithValue("@TotalGames", player.TotalGames);
            cmd.Parameters.AddWithValue("@WinRate", winRate);
            cmd.ExecuteNonQuery();
        }
    }
    return Results.Ok("Dodano gracza!");
});
```

### 3. Add PUT Endpoint (Update Existing Player)
```csharp
app.MapPut("/players/{name}", (string name, PlayerStats player) =>
{
    using (var conn = new MySqlConnection(connStr))
    {
        conn.Open();
        
        // Check if player exists
        string checkSql = "SELECT COUNT(*) FROM players WHERE Name = @Name";
        using (var checkCmd = new MySqlCommand(checkSql, conn))
        {
            checkCmd.Parameters.AddWithValue("@Name", name);
            long count = (long)checkCmd.ExecuteScalar();
            if (count == 0)
            {
                return Results.NotFound($"Player '{name}' not found");
            }
        }
        
        // Calculate new win rate
        // First get current stats
        int currentWins = 0, currentGames = 0;
        string getSql = "SELECT Wins, TotalGames FROM players WHERE Name = @Name";
        using (var getCmd = new MySqlCommand(getSql, conn))
        {
            getCmd.Parameters.AddWithValue("@Name", name);
            using (var reader = getCmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    currentWins = reader.GetInt32(0);
                    currentGames = reader.GetInt32(1);
                }
            }
        }
        
        int newWins = currentWins + player.Wins;
        int newGames = currentGames + player.TotalGames;
        float winRate = newGames > 0 ? ((float)newWins / newGames) * 100f : 0f;
        
        // Update player stats (accumulative)
        string sql = "UPDATE players SET Kills = Kills + @Kills, Money = Money + @Money, Wins = Wins + @Wins, TotalGames = TotalGames + @TotalGames, WinRate = @WinRate WHERE Name = @Name";
        using (var cmd = new MySqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Kills", player.Kills);
            cmd.Parameters.AddWithValue("@Money", player.Money);
            cmd.Parameters.AddWithValue("@Wins", player.Wins);
            cmd.Parameters.AddWithValue("@TotalGames", player.TotalGames);
            cmd.Parameters.AddWithValue("@WinRate", winRate);
            cmd.ExecuteNonQuery();
        }
    }
    return Results.Ok($"Zaktualizowano gracza '{name}'!");
});
```

## Testing the Integration

### 1. Start Backend
```bash
cd UnityBackend
dotnet run
```

Verify it's running at `http://localhost:5100`

### 2. In Unity
1. Ensure PlayerStatsManager GameObject exists in your game scene
2. Enable "Debug Mode" on PlayerStatsManager to see detailed logs
3. Play a game until it ends
4. Check Unity Console for logs like:
   ```
   [PlayerStatsManager] Initialized stats for player: Player
   [PlayerStatsManager] Player kills: 1
   [PlayerStatsManager] Player total money: 100
   [PlayerStatsManager] Successfully saved stats for Player
   ```

### 3. Verify Database
Open phpMyAdmin or MySQL client:
```sql
SELECT * FROM players;
```

You should see:
- Player names
- Kill counts
- Money earned
- Wins/TotalGames
- WinRate calculated correctly

## Troubleshooting

### Stats Not Saving
1. Check PlayerStatsManager exists in scene
2. Check "Enable Stat Tracking" is checked
3. Verify backend is running (`http://localhost:5100/players`)
4. Enable Debug Mode to see detailed error messages

### Backend Errors
- **404 on PUT**: Backend doesn't have PUT endpoint yet - add it from above
- **500 Server Error**: Check MySQL connection string in `Program.cs`
- **Connection Refused**: Backend not running - start with `dotnet run`

### Player Names Incorrect
- Bot names should be like "BOT_1", "BOT_2"
- Real player should be named "Player" or similar
- Check GameObject names in your scene

## Data Flow

```
Gameplay Event (Kill/Money/Game End)
        ?
PlayerStatsManager.RecordXXX()
        ?
Store in Dictionary<string, PlayerStatistics>
        ?
Game Ends ? RecordGameEnd()
        ?
For each player:
  - Check if exists (GET /players/{name})
  - If exists: UPDATE (PUT /players/{name})
  - If not: CREATE (POST /players)
        ?
Database Updated with WinRate calculated
```

## Future Enhancements

### Possible Additions:
- **Death tracking** - Add Deaths field to track K/D ratio
- **Average money per game** - Calculate avg money earned
- **Favorite weapon** - Track most used weapon
- **Best game stats** - Track highest kills/money in single game
- **Leaderboards** - Display top players by wins, kills, money
- **Match history** - Save individual match details with timestamps

### API Extensions Needed:
- `GET /players/leaderboard?sortBy=kills&limit=10`
- `POST /matches` - Record individual match data
- `GET /players/{name}/stats` - Detailed player profile
- `DELETE /players/{name}` - Remove player (admin only)
