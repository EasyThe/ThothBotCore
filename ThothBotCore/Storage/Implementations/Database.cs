using Dapper;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Storage.Models;
using static ThothBotCore.Connections.Models.Player;

namespace ThothBotCore.Storage
{
    public class Database
    {
        public static async Task RegisterNewGuild(SocketGuild guild)
        {
            string sql = $"INSERT INTO serverConfig(serverID, prefix) VALUES({guild.Id}, \"{Credentials.botConfig.prefix}\")";

            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                await cnn.ExecuteAsync(sql);
            }
        }

        public static async void SetPrefix(ulong serverID, string prefix)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                await cnn.ExecuteAsync($"INSERT INTO serverConfig(serverID, prefix) " +
                    $"VALUES({serverID}, \"{prefix}\") " +
                    $"ON CONFLICT(serverID) " +
                    $"DO UPDATE SET prefix = \"{prefix}\"");
            }
        }

        public static List<ServerConfig> GetPrefix(SocketGuild guild) // Get god by godname
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<ServerConfig>($"SELECT * FROM serverConfig WHERE serverID LIKE '%{guild.Id}%'", new DynamicParameters());
                return output.ToList();
            }
        }

        public static async Task AddPlayerToDb(List<PlayerStats> playerStats)
        {
            // This thing looks so bad... I hope I will find something else, possibly looking way better, smaller and performance-friendly!
            string sql = "INSERT INTO players(active_player_id, id, hz_player_name, name, team_id, team__name, " +
                "level, mastery_level, hours_played, wins, losses, leaves, region, personal__status__message, " +
                "last__login__datetime, created__datetime, total__worshippers, total__achievements, tier__conquest, " +
                "tier__joust, tier__duel, avatar_url, ranked_joust_controller__season, rank__stat__duel, ranked_conquest_controller__prev_rank, " +
                "ranked_joust_controller__losses, ranked_duel_controller__trend, ranked_joust_controller__prev_rank, " +
                "ranked_duel_controller__wins, rank__stat__conquest, ranked_duel__tier, ranked_conquest_controller__losses, " +
                "ranked_conquest__leaves, ranked_joust__name, ranked_joust__leaves, ranked_joust__tier, ranked_joust__season, " +
                "ranked_joust__wins, ranked_conquest_controller__wins, ranked_duel_controller__prev_rank, " +
                "ranked_conquest_controller__trend, ranked_joust__prev_rank, ranked_joust_controller__wins, ranked_conquest__trend, " +
                "ranked_duel_controller__losses, ranked_joust__losses, ranked_duel__losses, ranked_duel_controller__rank, " +
                "ranked_joust_controller__points, ranked_duel__wins, ranked_joust_controller__tier, ranked_duel__points, " +
                "ranked_duel__prev_rank, ranked_conquest_controller__leaves, ranked_joust_controller__rank, " +
                "ranked_conquest_controller__rank, ranked_conquest__tier, ranked_joust_controller__trend, " +
                "ranked_conquest_controller__season, ranked_duel__name, ranked_conquest__name, rank__stat__joust, " +
                "ranked_duel_controller__season, ranked_duel__season, ranked_duel__leaves, ranked_duel_controller__tier, " +
                "ranked_duel_controller__points, ranked_conquest__season, ranked_duel_controller__leaves, " +
                "ranked_duel__rank, ranked_conquest_controller__name, ranked_joust__rank, ranked_conquest_controller__tier, " +
                "ranked_joust_controller__leaves, ranked_conquest__losses, ranked_conquest__points, ranked_conquest_controller__points, " +
                "ranked_conquest__rank, ranked_joust__points, ranked_joust_controller__name, ranked_conquest__prev_rank, " +
                "ranked_duel__trend, ranked_duel_controller__name, ranked_joust__trend, ranked_conquest__wins, updated_at)" +
                    "VALUES(@activeID, @ID, @hz_player_name, @name, @teamID, @teamName, @level, @mastery, @hoursPlayed, " +
                    "@wins, @losses, @leaves, @region, @statusMess, @lastLogin, @accCreated, @totWorship, @totAchiev, @tierConq, " +
                    "@tierJoust, @tierDuel, @avatarURL, @rankJoustContrSeason, @rankStatDuel, @rankConqContrPrevRank, @rankJoustContrLoss, " +
                    "@rankDuelContrTrend, @rankJoustContrPrevRank, @rankDuelContrWins, @rankStatConq, @rankDuelTier, @rankConqContrLoss, " +
                    "@rankConqLeaves, @rankJoustName, @rankJoustLeaves, @rankJoustTier, @rankJoustSeason, @rankJoustWins, @rankConqContrWins, " +
                    "@rankDuelContrPrevRank, @rankConqContrTrend, @rankJoustPrevRank, @rankJoustContrWins, @rankConqTrend, @rankDuelContrLoss, " +
                    "@rankJoustLoss, @rankDuelLoss, @rankDuelContrRank, @rankJoustContrPoints, @rankDuelWins, @rankJoustContrTier, @rankDuelPoints, " +
                    "@rankDuelPrevRank, @rankConqContrLeaves, @rankJoustContrRank, @rankConqContrRank, @rankConqTier, @rankJoustContrTrend, " +
                    "@rankConqContrSeason, @rankDuelName, @rankConqName, @rankStatJoust, @rankDuelContrSeason, @rankDuelSeason, @rankDuelLeaves, " +
                    "@rankDuelContrTier, @rankDuelContrPoints, @rankConqSeason, @rankDuelContrLeaves, @rankDuelRank, @rankConqContrName, @rankJoustRank, " +
                    "@rankConqContrTier, @rankJoustContrLeaves, @rankConqLoss, @rankConqPoints, @rankConqContrPoints, @rankConqRank, @rankJoustPoints, @rankJoustContrName, " +
                    "@rankConqPrevRank, @rankDuelTrend, @rankDuelContrName, @rankJoustTrend, @rankConqWins, @updatedAt)" + 
                    "ON CONFLICT(active_player_id) " +
                    "DO UPDATE SET " +
                    "created__datetime = @accCreated, hz_player_name = @hz_player_name, team_id = @teamID, " +
                    "team__name = @teamName, level = @level, mastery_level = @mastery, " +
                    "hours_played = @hoursPlayed, wins = @wins, losses = @losses, leaves = @leaves, " +
                    "region = @region, personal__status__message = @statusMess, last__login__datetime = @lastLogin, total__worshippers = @totWorship, " +
                    "total__achievements = @totAchiev, tier__conquest = @tierConq, tier__joust = @tierJoust, tier__duel = @tierDuel, " +
                    "avatar_url = @avatarURL, ranked_joust_controller__season = @rankJoustContrSeason, rank__stat__duel = @rankStatDuel, " +
                    "ranked_conquest_controller__prev_rank = @rankConqContrPrevRank, ranked_joust_controller__losses = @rankJoustContrLoss, " +
                    "ranked_duel_controller__trend = @rankDuelContrTrend, ranked_joust_controller__prev_rank = @rankJoustContrPrevRank, " +
                    "ranked_duel_controller__wins = @rankDuelContrWins, rank__stat__conquest = @rankStatConq, ranked_duel__tier = @rankDuelTier, " +
                    "ranked_conquest_controller__losses = @rankConqContrLoss, ranked_conquest__leaves = @rankConqLeaves, ranked_joust__name = @rankJoustName, " +
                    "ranked_joust__leaves = @rankJoustLeaves, ranked_joust__tier = @rankJoustTier, ranked_joust__season = @rankJoustSeason, " +
                    "ranked_joust__wins = @rankJoustWins, ranked_conquest_controller__wins = @rankConqContrWins, ranked_duel_controller__prev_rank = @rankDuelContrPrevRank, " +
                    "ranked_conquest_controller__trend = @rankConqContrTrend, ranked_joust__prev_rank = @rankJoustPrevRank, ranked_joust_controller__wins = @rankJoustContrWins, " +
                    "ranked_conquest__trend = @rankConqTrend, ranked_duel_controller__losses = @rankDuelContrLoss, ranked_joust__losses = @rankJoustLoss, " +
                    "ranked_duel__losses = @rankDuelLoss, ranked_duel_controller__rank = @rankDuelContrRank, ranked_joust_controller__points = @rankJoustContrPoints, " +
                    "ranked_duel__wins = @rankDuelWins, ranked_joust_controller__tier = @rankJoustContrTier, ranked_duel__points = @rankDuelPoints, " +
                    "ranked_duel__prev_rank = @rankDuelPrevRank, ranked_conquest_controller__leaves = @rankConqContrLeaves, ranked_joust_controller__rank = @rankJoustContrRank, " +
                    "ranked_conquest_controller__rank = @rankConqContrRank, ranked_conquest__tier = @rankConqTier, ranked_joust_controller__trend = @rankJoustContrTrend, " +
                    "ranked_conquest_controller__season = @rankConqContrSeason, ranked_duel__name = @rankDuelName, ranked_conquest__name = @rankConqName, " +
                    "rank__stat__joust = @rankStatJoust, ranked_duel_controller__season = @rankDuelContrSeason, ranked_duel__season = @rankDuelSeason, " +
                    "ranked_duel__leaves = @rankDuelLeaves, ranked_duel_controller__tier = @rankDuelContrTier, ranked_duel_controller__points = @rankDuelContrPoints, " +
                    "ranked_conquest__season = @rankConqSeason, ranked_duel_controller__leaves = @rankDuelContrLeaves, ranked_duel__rank = @rankDuelRank, " +
                    "ranked_conquest_controller__name = @rankConqContrName, ranked_joust__rank = @rankJoustRank, ranked_conquest_controller__tier = @rankConqContrTier, " +
                    "ranked_joust_controller__leaves = @rankJoustContrLeaves, ranked_conquest__losses = @rankConqLoss, ranked_conquest__points = @rankConqPoints, " +
                    "ranked_conquest_controller__points = @rankConqContrPoints, ranked_conquest__rank = @rankConqRank, ranked_joust__points = @rankJoustPoints, " +
                    "ranked_joust_controller__name = @rankJoustContrName, ranked_conquest__prev_rank = @rankConqPrevRank, ranked_duel__trend = @rankDuelTrend, " +
                    "ranked_duel_controller__name = @rankDuelContrName, ranked_joust__trend = @rankJoustTrend, ranked_conquest__wins = @rankConqWins, updated_at = @updatedAt";
            SQLiteConnection connection = new SQLiteConnection(LoadConnectionString());
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            await connection.OpenAsync();

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
            command.Parameters.AddWithValue("lastLogin", playerStats[0].Last_Login_Datetime);
            command.Parameters.AddWithValue("accCreated", playerStats[0].Created_Datetime);
            command.Parameters.AddWithValue("totWorship", playerStats[0].Total_Worshippers);
            command.Parameters.AddWithValue("totAchiev", playerStats[0].Total_Achievements);
            command.Parameters.AddWithValue("tierConq", playerStats[0].Tier_Conquest);
            command.Parameters.AddWithValue("tierJoust", playerStats[0].Tier_Joust);
            command.Parameters.AddWithValue("tierDuel", playerStats[0].Tier_Duel);
            command.Parameters.AddWithValue("avatarURL", playerStats[0].Avatar_URL);
            command.Parameters.AddWithValue("rankStatConq", playerStats[0].Rank_Stat_Conquest);
            command.Parameters.AddWithValue("rankStatJoust", playerStats[0].Rank_Stat_Joust);
            command.Parameters.AddWithValue("rankStatDuel", playerStats[0].Rank_Stat_Duel);
            command.Parameters.AddWithValue("updatedAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            // Ranked Conquest
            command.Parameters.AddWithValue("rankConqName", playerStats[0].RankedConquest.Name);
            command.Parameters.AddWithValue("rankConqTier", playerStats[0].RankedConquest.Tier);
            command.Parameters.AddWithValue("rankConqWins", playerStats[0].RankedConquest.Wins);
            command.Parameters.AddWithValue("rankConqLoss", playerStats[0].RankedConquest.Losses);
            command.Parameters.AddWithValue("rankConqLeaves", playerStats[0].RankedConquest.Leaves);
            command.Parameters.AddWithValue("rankConqSeason", playerStats[0].RankedConquest.Season);
            command.Parameters.AddWithValue("rankConqPoints", playerStats[0].RankedConquest.Points);
            command.Parameters.AddWithValue("rankConqTrend", playerStats[0].RankedConquest.Trend);
            command.Parameters.AddWithValue("rankConqRank", playerStats[0].RankedConquest.Rank);
            command.Parameters.AddWithValue("rankConqPrevRank", playerStats[0].RankedConquest.PrevRank);
            // Ranked Conquest Controller
            command.Parameters.AddWithValue("rankConqContrName", playerStats[0].RankedConquestController.Name);
            command.Parameters.AddWithValue("rankConqContrTier", playerStats[0].RankedConquestController.Tier);
            command.Parameters.AddWithValue("rankConqContrWins", playerStats[0].RankedConquestController.Wins);
            command.Parameters.AddWithValue("rankConqContrLoss", playerStats[0].RankedConquestController.Losses);
            command.Parameters.AddWithValue("rankConqContrLeaves", playerStats[0].RankedConquestController.Leaves);
            command.Parameters.AddWithValue("rankConqContrSeason", playerStats[0].RankedConquestController.Season);
            command.Parameters.AddWithValue("rankConqContrTrend", playerStats[0].RankedConquestController.Trend);
            command.Parameters.AddWithValue("rankConqContrPoints", playerStats[0].RankedConquestController.Points);
            command.Parameters.AddWithValue("rankConqContrRank", playerStats[0].RankedConquestController.Rank);
            command.Parameters.AddWithValue("rankConqContrPrevRank", playerStats[0].RankedConquestController.PrevRank);
            // Ranked Joust
            command.Parameters.AddWithValue("rankJoustName", playerStats[0].RankedJoust.Name);
            command.Parameters.AddWithValue("rankJoustTier", playerStats[0].RankedJoust.Tier);
            command.Parameters.AddWithValue("rankJoustWins", playerStats[0].RankedJoust.Wins);
            command.Parameters.AddWithValue("rankJoustLoss", playerStats[0].RankedJoust.Losses);
            command.Parameters.AddWithValue("rankJoustLeaves", playerStats[0].RankedJoust.Leaves);
            command.Parameters.AddWithValue("rankJoustSeason", playerStats[0].RankedJoust.Season);
            command.Parameters.AddWithValue("rankJoustPoints", playerStats[0].RankedJoust.Points);
            command.Parameters.AddWithValue("rankJoustTrend", playerStats[0].RankedJoust.Trend);
            command.Parameters.AddWithValue("rankJoustRank", playerStats[0].RankedJoust.Rank);
            command.Parameters.AddWithValue("rankJoustPrevRank", playerStats[0].RankedJoust.PrevRank);
            // Ranked Joust Controller
            command.Parameters.AddWithValue("rankJoustContrName", playerStats[0].RankedJoustController.Name);
            command.Parameters.AddWithValue("rankJoustContrTier", playerStats[0].RankedJoustController.Tier);
            command.Parameters.AddWithValue("rankJoustContrWins", playerStats[0].RankedJoustController.Wins);
            command.Parameters.AddWithValue("rankJoustContrLoss", playerStats[0].RankedJoustController.Losses);
            command.Parameters.AddWithValue("rankJoustContrLeaves", playerStats[0].RankedJoustController.Leaves);
            command.Parameters.AddWithValue("rankJoustContrSeason", playerStats[0].RankedJoustController.Season);
            command.Parameters.AddWithValue("rankJoustContrPoints", playerStats[0].RankedJoustController.Points);
            command.Parameters.AddWithValue("rankJoustContrTrend", playerStats[0].RankedJoustController.Trend);
            command.Parameters.AddWithValue("rankJoustContrRank", playerStats[0].RankedJoustController.Rank);
            command.Parameters.AddWithValue("rankJoustContrPrevRank", playerStats[0].RankedJoustController.PrevRank);
            // Ranked Duel
            command.Parameters.AddWithValue("rankDuelName", playerStats[0].RankedDuel.Name);
            command.Parameters.AddWithValue("rankDuelTier", playerStats[0].RankedDuel.Tier);
            command.Parameters.AddWithValue("rankDuelWins", playerStats[0].RankedDuel.Wins);
            command.Parameters.AddWithValue("rankDuelLoss", playerStats[0].RankedDuel.Losses);
            command.Parameters.AddWithValue("rankDuelLeaves", playerStats[0].RankedDuel.Leaves);
            command.Parameters.AddWithValue("rankDuelSeason", playerStats[0].RankedDuel.Season);
            command.Parameters.AddWithValue("rankDuelPoints", playerStats[0].RankedDuel.Points);
            command.Parameters.AddWithValue("rankDuelTrend", playerStats[0].RankedDuel.Trend);
            command.Parameters.AddWithValue("rankDuelRank", playerStats[0].RankedDuel.Rank);
            command.Parameters.AddWithValue("rankDuelPrevRank", playerStats[0].RankedDuel.PrevRank);
            // Ranked Duel Controller
            command.Parameters.AddWithValue("rankDuelContrName", playerStats[0].RankedDuelController.Name);
            command.Parameters.AddWithValue("rankDuelContrWins", playerStats[0].RankedDuelController.Wins);
            command.Parameters.AddWithValue("rankDuelContrLoss", playerStats[0].RankedDuelController.Losses);
            command.Parameters.AddWithValue("rankDuelContrLeaves", playerStats[0].RankedDuelController.Leaves);
            command.Parameters.AddWithValue("rankDuelContrSeason", playerStats[0].RankedDuelController.Season);
            command.Parameters.AddWithValue("rankDuelContrTrend", playerStats[0].RankedDuelController.Trend);
            command.Parameters.AddWithValue("rankDuelContrTier", playerStats[0].RankedDuelController.Tier);
            command.Parameters.AddWithValue("rankDuelContrPoints", playerStats[0].RankedDuelController.Points);
            command.Parameters.AddWithValue("rankDuelContrRank", playerStats[0].RankedDuelController.Rank);
            command.Parameters.AddWithValue("rankDuelContrPrevRank", playerStats[0].RankedDuelController.PrevRank);
            
            await command.ExecuteNonQueryAsync();
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

        public class ServerConfig
        {
            public ulong serverID { get; set; }
            public string prefix { get; set; }
        }
    }
}
