using Heroes.DataAccessLayer.Data;
using Heroes.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace HelperCore;

public static class PlayerExtensions
{
    public static List<int> GetPlayerIdAlts(HeroesdataContext heroesEntity, int playerId)
    {
        return heroesEntity.Players.Single(i => i.PlayerId == playerId).GetPlayerIdAlts(heroesEntity.Database.GetConnectionString());
    }

    public static List<int> GetPlayerIdAlts(this Player player, string connectionString)
    {
        var playerIdAlts = new List<int>();

        using var mySqlConnection =
            new MySqlConnection(connectionString);
        mySqlConnection.Open();

        using var mySqlCommand =
            new MySqlCommand(
                @"select PlayerIDAlt from PlayerAlt where PlayerIDMain = " + player.PlayerId,
                mySqlConnection)
            { CommandTimeout = 360 };
        using var mySqlDataReader = mySqlCommand.ExecuteReader();
        while (mySqlDataReader.Read())
        {
            playerIdAlts.Add(mySqlDataReader.GetInt32("PlayerIDAlt"));
        }

        return playerIdAlts;
    }
}
