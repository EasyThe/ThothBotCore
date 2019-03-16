using Dapper;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using ThothBotCore.Storage.Models;
using static ThothBotCore.Connections.Models.Player;

namespace ThothBotCore.Storage
{
    public class Database
    {
        public static void AddPlayerToDb(List<PlayerStats> playerStats)
        {
            string sql = "INSERT INTO players(active_player_id, id, hz_player_name, name, team_id, team__name, " +
                "level, mastery_level, hours_played, wins, losses, leaves, region, personal__status__message)" +
                    "VALUES(@activeID, @ID, @hz_player_name, @name, @teamID, @teamName, @level, @mastery, @hoursPlayed, " +
                    "@wins, @losses, @leaves, @region, @statusMess)" +
                    "ON CONFLICT(active_player_id) " +
                    "DO UPDATE SET " +
                    "id = @ID, hz_player_name = @hz_player_name, team_id = @teamID, " +
                    "team__name = @teamName, level = @level, mastery_level = @mastery, " +
                    "hours_played = @hoursPlayed, wins = @wins, losses = @losses, leaves = @leaves, " +
                    "region = @region, personal__status__message = @statusMess";
            SQLiteConnection connection = new SQLiteConnection(LoadConnectionString());
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            connection.Open();

            command.Parameters.AddWithValue("activeID", playerStats[0].ActivePlayerId);
            command.Parameters.AddWithValue("ID", playerStats[0].Id);
            command.Parameters.AddWithValue("hz_player_name", playerStats[0].hz_player_name);
            command.Parameters.AddWithValue("name", playerStats[0].Name);
            command.Parameters.AddWithValue("teamID", playerStats[0].TeamId);
            command.Parameters.AddWithValue("teamName", playerStats[0].Team_Name);
            command.Parameters.AddWithValue("level", playerStats[0].Level);
            command.Parameters.AddWithValue("mastery", playerStats[0].MasteryLevel);
            command.Parameters.AddWithValue("hoursPlayed", playerStats[0].HoursPlayed);
            command.Parameters.AddWithValue("wins", playerStats[0].Wins);
            command.Parameters.AddWithValue("losses", playerStats[0].Losses);
            command.Parameters.AddWithValue("leaves", playerStats[0].Leaves);
            command.Parameters.AddWithValue("region", playerStats[0].Region);
            command.Parameters.AddWithValue("statusMess", playerStats[0].Personal_Status_Message);

            command.ExecuteNonQuery();
            connection.Close();

        }

        public static List<Gods.God> LoadGod(string godname) // Get god by godname
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<Gods.God>($"SELECT * FROM Gods WHERE Name LIKE '%{godname}%'", new DynamicParameters());
                return output.ToList();
            }
        }

        public static List<Gods.God> GetRandomGod() // Get random god for rgod command
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<Gods.God>($"SELECT Name, godIcon_URL, Pantheon, Roles, Title, Type, DomColor FROM Gods", new DynamicParameters());
                return output.ToList();
            }
        }

        public static List<Gods.God> LoadGodsDomColor()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<Gods.God>($"SELECT id, godIcon_URL, DomColor FROM Gods", new DynamicParameters());
                return output.ToList();
            }
        }

        public static void SaveGods(Gods.God gods)
        {
            // TO DO....

            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("insert into Gods ", gods);
            }
        }

        public static void SaveDomColor(int id, int color)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute($"UPDATE Gods SET DomColor = {color} WHERE id = '{id}'");
            }
        }

        private static string LoadConnectionString(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
    }
}
