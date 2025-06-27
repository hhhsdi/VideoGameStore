using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using VideoGameStore.Models;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;

namespace VideoGameStore.Services
{
    public class DatabaseService
    {
        private readonly string _databasePath;

        public DatabaseService()
        {
            _databasePath = Path.Combine(Directory.GetCurrentDirectory(), "games.db");
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection($"Data Source={_databasePath}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Games (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        Genre TEXT,
                        Price REAL NOT NULL,
                        Platform TEXT,
                        Key TEXT UNIQUE NOT NULL
                    );";
                command.ExecuteNonQuery();
            }
        }

        public ObservableCollection<Game> GetGamesForShop()
        {
            var gamesFromDb = new List<Game>();
            using (var connection = new SqliteConnection($"Data Source={_databasePath}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Title, Genre, Price, Platform, Key FROM Games";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        gamesFromDb.Add(new Game
                        {
                            Id = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Genre = reader.GetString(2),
                            Price = reader.GetDecimal(3),
                            Platform = reader.GetString(4),
                            Key = reader.GetString(5) 
                        });
                    }
                }
            }

            var shopGames = new ObservableCollection<Game>();
            var groupedGames = gamesFromDb
                .GroupBy(g => new { g.Title, g.Genre, g.Price, g.Platform })
                .Select(group => new Game
                {
                    Id = 0,
                    Title = group.Key.Title,
                    Genre = group.Key.Genre,
                    Price = group.Key.Price,
                    Platform = group.Key.Platform,
                    Key = "N/A",
                    AvailableKeysCount = group.Count()
                })
                .OrderBy(g => g.Title);

            foreach (var game in groupedGames)
            {
                shopGames.Add(game);
            }

            return shopGames;
        }

        public ObservableCollection<Game> GetGames()
        {
            var games = new ObservableCollection<Game>();
            using (var connection = new SqliteConnection($"Data Source={_databasePath}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Title, Genre, Price, Platform, Key FROM Games";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        games.Add(new Game
                        {
                            Id = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Genre = reader.GetString(2),
                            Price = reader.GetDecimal(3),
                            Platform = reader.GetString(4),
                            Key = reader.GetString(5)
                        });
                    }
                }
            }
            return games;
        }

        public void AddGame(Game game)
        {
            using (var connection = new SqliteConnection($"Data Source={_databasePath}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Games (Title, Genre, Price, Platform, Key)
                    VALUES (@Title, @Genre, @Price, @Platform, @Key)";
                command.Parameters.AddWithValue("@Title", game.Title);
                command.Parameters.AddWithValue("@Genre", game.Genre);
                command.Parameters.AddWithValue("@Price", game.Price);
                command.Parameters.AddWithValue("@Platform", game.Platform);
                command.Parameters.AddWithValue("@Key", game.Key);
                command.ExecuteNonQuery();
            }
        }

        public void UpdateGame(Game game)
        {
            using (var connection = new SqliteConnection($"Data Source={_databasePath}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Games
                    SET Title = @Title, Genre = @Genre, Price = @Price, Platform = @Platform, Key = @Key
                    WHERE Id = @Id";
                command.Parameters.AddWithValue("@Title", game.Title);
                command.Parameters.AddWithValue("@Genre", game.Genre);
                command.Parameters.AddWithValue("@Price", game.Price);
                command.Parameters.AddWithValue("@Platform", game.Platform);
                command.Parameters.AddWithValue("@Key", game.Key);
                command.Parameters.AddWithValue("@Id", game.Id);
                command.ExecuteNonQuery();
            }
        }

        public void DeleteGame(int id)
        {
            using (var connection = new SqliteConnection($"Data Source={_databasePath}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Games WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();
            }
        }
    }
}