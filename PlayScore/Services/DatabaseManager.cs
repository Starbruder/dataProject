﻿using MoonScore.Models;
using System.Data.SQLite;

namespace MoonScore.Services;

public sealed class DatabaseManager(SQLiteConnection connection)
{
    private const string databaseName = "MoonScore";

    public static string GetDatabaseName() => ( databaseName + ".db" );

    public void ConnectToDatabase()
    {
        if (connection.State == System.Data.ConnectionState.Open)
        {
            return;
        }

        try
        {
            connection.Open();
            Console.WriteLine("Database connected.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public async Task AddGameToSpieleTableAsync(GameModel game)
    {
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var sql = @"
        INSERT INTO Spiele (ID, Name, ReleaseDate, Rating, MondphaseID)
        VALUES (@ID, @Name, @ReleaseDate, @Rating, @MondphaseID);";

        using var command = new SQLiteCommand(sql, connection);
        command.Parameters.AddWithValue("@ID", game.Id);
        command.Parameters.AddWithValue("@Name", game.Name);
        command.Parameters.AddWithValue("@Rating", game.Rating);
        command.Parameters.AddWithValue("@ReleaseDate", game.Released);
        command.Parameters.AddWithValue("@MondphaseID", game.MondphaseID);

        await command.ExecuteNonQueryAsync();
    }

    public async Task AddGamesToSpieleTableAsync(ICollection<GameModel> games)
    {
        foreach (var game in games)
        {
            if (game.Rating > 0)
            {
                await AddGameToSpieleTableAsync(game);
            }
        }
    }

    public Dictionary<string, double> GetAverageRatingPerMondphase()
    {
        var averages = new Dictionary<string, double>();

        if (connection.State != System.Data.ConnectionState.Open)
        {
            connection.Open();
        }

        var sql = @"
        SELECT m.Name, AVG(s.Rating) 
        FROM Spiele s
        JOIN Mondphasen m ON s.MondphaseID = m.Id
        GROUP BY m.Name;";

        using var command = new SQLiteCommand(sql, connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var mondphaseName = reader.GetString(0);
            var averageRating = reader.GetDouble(1);

            averages[mondphaseName] = averageRating;
        }

        return averages;
    }
}
