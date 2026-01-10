# Quick Setup Guide - Database Statistics Tracking

## ? What's Been Implemented

Your game now **automatically tracks and saves** these statistics to the database:

| Statistic | When Tracked | How |
|-----------|--------------|-----|
| **Kills** | Player dies from damage | Attacker's kill count increases |
| **Money** | Player earns money | Money earned is accumulated |
| **Wins** | Game ends | Winners get +1 win |
| **Total Games** | Game ends | All players get +1 game |
| **Win Rate** | Game ends | Calculated as (Wins/TotalGames) × 100 |

## ?? Setup Steps

### 1. Add PlayerStatsManager to Your Scene

**In Unity Editor:**
1. Right-click in Hierarchy
2. Create Empty GameObject
3. Rename it to "PlayerStatsManager"
4. Add Component ? Search "PlayerStatsManager"
5. Configure:
   - ? Enable Stat Tracking
   - ? Debug Mode (to see logs)
   - API Base URL: `http://localhost:5100`

### 2. Update Backend (REQUIRED)

Your backend at `UnityBackend/Program.cs` needs 2 new endpoints:

**Copy this entire updated Program.cs:**

```csharp
using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string connStr = "server=localhost;user=root;password=;database=mmm";

// GET all players
app.MapGet("/players", () =>
{
    var players = new List<Dictionary<string, object>>();
    using (var conn = new MySqlConnection(connStr))
    {
        conn.Open();
        string sql = "SELECT * FROM players";
        using (var cmd = new MySqlCommand(sql, conn))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var player = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                    player[reader.GetName(i)] = reader[i];
                players.Add(player);
            }
        }
    }
    return Results.Json(players);
});

// GET player by name (NEW)
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

// POST create new player (UPDATED with WinRate)
app.MapPost("/players", (PlayerStats player) =>
{
    using (var conn = new MySqlConnection(connStr))
    {
        conn.Open();
        
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

// PUT update existing player (NEW)
app.MapPut("/players/{name}", (string name, PlayerStats player) =>
{
    using (var conn = new MySqlConnection(connStr))
    {
        conn.Open();
        
        // Check if exists
        string checkSql = "SELECT COUNT(*) FROM players WHERE Name = @Name";
        using (var checkCmd = new MySqlCommand(checkSql, conn))
        {
            checkCmd.Parameters.AddWithValue("@Name", name);
            long count = (long)checkCmd.ExecuteScalar();
            if (count == 0)
                return Results.NotFound($"Player '{name}' not found");
        }
        
        // Get current stats to calculate new WinRate
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
        
        // Update stats (accumulative)
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

app.Run();

record PlayerStats(string Name, int Kills, long Money, int Wins, int TotalGames);
```

**Then restart backend:**
```bash
cd UnityBackend
dotnet run
```

## ?? Testing

### 1. Play a Complete Game
- Start your game
- Kill some players (kills tracked ?)
- Earn money (money tracked ?)
- Finish the game until winners announced

### 2. Check Console Logs
You should see:
```
[PlayerStatsManager] Initialized stats for player: Player
[PlayerStatsManager] Player kills: 1
[PlayerStatsManager] Player total money: 500
[PlayerStatsManager] Starting to save all player stats to database...
[PlayerStatsManager] Successfully saved stats for Player
```

### 3. Verify Database
```sql
SELECT * FROM players;
```

Expected result:
```
| Id | Name    | Kills | Money | Wins | TotalGames | WinRate |
|----|---------|-------|-------|------|------------|---------|
| 1  | Player  | 3     | 1500  | 1    | 1          | 100.0   |
| 2  | BOT_1   | 1     | 200   | 0    | 1          | 0.0     |
```

## ?? How It Works

### Kill Tracking
```csharp
// When player takes damage leading to death:
Player.TakeDamage(damage, attacker) 
    ? Player.Die(attacker) 
    ? PlayerStatsManager.RecordKill(attacker.name)
```

**Tracked in:**
- ? Melee weapons (knives, etc.)
- ? Ranged weapons (guns)
- ? Projectiles (bullets)
- ? Grenades (explosions)

### Money Tracking
```csharp
// When player earns money:
Player.AddBalance(amount) 
    ? PlayerStatsManager.RecordMoneyEarned(name, amount)
```

**Sources:**
- ? Minigame rewards
- ? Item pickups
- ? Any AddBalance() or AddMoney() call

### Game End Tracking
```csharp
// When game ends:
GameManager.EndGame(winningTeam) 
    ? PlayerStatsManager.RecordGameEnd(winningTeam, allPlayers)
    ? Save to database (POST or PUT)
```

**Records:**
- ? Winners get +1 win
- ? All players get +1 total game
- ? Final money balance added
- ? WinRate calculated

## ?? Database Updates

### For New Players
Uses `POST /players` - Creates new record with all stats

### For Existing Players
Uses `PUT /players/{name}` - **Adds** to existing stats (accumulative)

Example:
```
Player has: 5 kills, 2 wins, 3 games
Game ends: +2 kills, +1 win, +1 game
Result: 7 kills, 3 wins, 4 games, WinRate = 75%
```

## ?? Important Notes

### Player Names
- Player GameObject names are used as database keys
- Bot names: "BOT_1", "BOT_2", etc.
- Player name: "Player" or custom name
- **Name must be consistent across games for accumulation**

### Backend Must Be Running
- Start backend BEFORE playing: `dotnet run`
- Check `http://localhost:5100/players` works
- Stats won't save if backend is down

### Debug Mode
- Enable "Debug Mode" on PlayerStatsManager
- See detailed logs in Unity Console
- Helps diagnose issues

## ? What's Next?

All stats are now automatically tracked! The system:
- ? Tracks kills from all weapon types
- ? Tracks money from all sources
- ? Records wins/games on completion
- ? Calculates win rate
- ? Uses accumulative updates for existing players
- ? Creates new players automatically

No additional code needed - just play and stats will save! ??
