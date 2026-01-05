using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string connStr = "server=localhost;user=root;password=;database=mmm";

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

app.MapPost("/players", (PlayerStats player) =>
{
    using (var conn = new MySqlConnection(connStr))
    {
        conn.Open();
        string sql = "INSERT INTO players (Name, Kills, Money, Wins, TotalGames) VALUES (@Name, @Kills, @Money, @Wins, @TotalGames)";
        using (var cmd = new MySqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@Name", player.Name);
            cmd.Parameters.AddWithValue("@Kills", player.Kills);
            cmd.Parameters.AddWithValue("@Money", player.Money);
            cmd.Parameters.AddWithValue("@Wins", player.Wins);
            cmd.Parameters.AddWithValue("@TotalGames", player.TotalGames);
            cmd.ExecuteNonQuery();
        }
    }
    return Results.Ok("Dodano gracza!");
});

app.Run();

record PlayerStats(string Name, int Kills, long Money, int Wins, int TotalGames);
