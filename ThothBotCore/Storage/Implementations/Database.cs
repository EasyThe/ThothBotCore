using Dapper;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ThothBotCore.Models;
using ThothBotCore.Storage.Models;
using ThothBotCore.Utilities;
using static ThothBotCore.Connections.Models.Player;

namespace ThothBotCore.Storage
{
    public class Database
    {
        public static async Task InsertServerStatusUpdates(string id, string inciID, string type, string status, string name, string body, string createdAt)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    await cnn.ExecuteAsync($"INSERT OR IGNORE INTO ServerStatusUpdates(id, inciID, type, status, name, body, createdAt) " +
                        $"VALUES(\"{id}\", \"{inciID}\", \"{type}\", \"{status}\", \"{name}\", \"{body}\", \"{createdAt}\")");
                }
            }
            catch (Exception ex)
            {
                await ErrorTracker.SendError($"**Error in InsertServerStatusUpdates\n{ex.Message}");
            }
        }

        public static List<string> GetServerStatusUpdates(string id)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<string>($"SELECT EXISTS(SELECT 1 FROM ServerStatusUpdates WHERE id LIKE '%{id}%')", new DynamicParameters());
                return output.ToList();// is it tho? THIS IS NOT WORKIIING
            }
        }

        public static async Task<List<ServerConfig>> GetNotifChannels() // Working as intended
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = await cnn.QueryAsync<ServerConfig>($"SELECT * FROM serverConfig WHERE statusBool LIKE '%1%'", new DynamicParameters());
                return output.ToList();
            }
        }

        public static async Task StopNotifs(ulong serverID) // Working as intended
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                await cnn.ExecuteAsync($"UPDATE serverConfig " +
                    $"SET statusBool = 0 " +
                    $"WHERE serverID = {serverID}");
            }
        }

        public static async Task SetNotifChannel(ulong serverID, string serverName, ulong statusChannel) // Working as intended
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                await cnn.ExecuteAsync($"INSERT INTO serverConfig(serverID, serverName, statusBool, statusChannel) " +
                    $"VALUES({serverID}, \"{serverName}\", 1, {statusChannel}) " +
                    $"ON CONFLICT(serverID) " +
                    $"DO UPDATE SET statusChannel = \"{statusChannel}\", statusBool = 1");
            }
        }

        public static async Task SetPrefix(ulong serverID, string serverName, string prefix) // Working as intended
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                await cnn.ExecuteAsync($"INSERT INTO serverConfig(serverID, serverName, prefix) " +
                    $"VALUES({serverID}, \"{serverName}\", \"{prefix}\") " +
                    $"ON CONFLICT(serverID) " +
                    $"DO UPDATE SET prefix = \"{prefix}\", serverName = \"{serverName}\"");
            }
        }

        public static async Task SetGuild(ulong serverID, string serverName) // Working as intended
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                await cnn.ExecuteAsync($"INSERT OR IGNORE INTO serverConfig(serverID, serverName) " +
                    $"VALUES({serverID}, \"{serverName}\")");
            }
        }

        public static async Task<List<ServerConfig>> GetServerConfig(SocketGuild guild) // Get prefix for guild. Working as intended
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = await cnn.QueryAsync<ServerConfig>($"SELECT * FROM serverConfig WHERE serverID LIKE '%{guild.Id}%'", new DynamicParameters());
                return output.ToList();
            }
        }

        public static async Task DeleteServerConfig(ulong id) // Deleting serverConfig if exists on server leaving.
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                await cnn.ExecuteAsync($"DELETE FROM serverConfig WHERE serverID = {id}");
            }
        }

        public static async Task SetTimeZone(SocketGuild guild, string timezone)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                await cnn.ExecuteAsync($"INSERT INTO serverConfig(serverID, serverName, timezone) " +
                    $"VALUES({guild.Id}, \"{guild.Name}\", \"{timezone}\") " +
                    $"ON CONFLICT(serverID) " +
                    $"DO UPDATE SET timezone = \"{timezone}\"");
            }
        }

        public static async Task<List<string>> GetTimeZone(ulong guildID)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = await cnn.QueryAsync<string>($"SELECT timezone FROM serverConfig WHERE serverID LIKE '%{guildID}%'", new DynamicParameters());
                return output.ToList();
            }
        } // >> Database.GetTimeZone(Context.Guild.Id).Result[0]

        public static async Task AddPlayerToDb(List<PlayerStats> playerStats, int portal)
        {
            // This thing looks so bad... I hope I will find something else, possibly looking way better, smaller and performance-friendly!
            string sql = "INSERT INTO players(ActivePlayerId, portal_id, Id, hz_player_name, hz_gamer_tag, Name, TeamId, Team_Name, " +
                "Level, MasteryLevel, HoursPlayed, Wins, Losses, Leaves, Region, Personal_Status_Message, " +
                "Last_Login_Datetime, Created_Datetime, Total_Worshippers, Total_Achievements, Tier_Conquest, " +
                "Tier_Joust, Tier_Duel, Avatar_URL, RankedJoustControllerSeason, Rank_Stat_Duel, RankedConquestControllerPrevRank, " +
                "RankedJoustControllerLosses, RankedDuelControllerTrend, RankedJoustControllerPrevRank, " +
                "RankedDuelControllerWins, Rank_Stat_Conquest, RankedDuelTier, RankedConquestControllerLosses, " +
                "RankedConquestLeaves, RankedJoustName, RankedJoustLeaves, RankedJoustTier, RankedJoustSeason, " +
                "RankedJoustWins, RankedConquestControllerWins, RankedDuelControllerPrevRank, " +
                "RankedConquestControllerTrend, RankedJoustPrevRank, RankedJoustControllerWins, RankedConquestTrend, " +
                "RankedDuelControllerLosses, RankedJoustLosses, RankedDuelLosses, RankedDuelControllerRank, " +
                "RankedJoustControllerPoints, RankedDuelWins, RankedJoustControllerTier, RankedDuelPoints, " +
                "RankedDuelPrevRank, RankedConquestControllerLeaves, RankedJoustControllerRank, " +
                "RankedConquestControllerRank, RankedConquestTier, RankedJoustControllerTrend, " +
                "RankedConquestControllerSeason, RankedDuelName, RankedConquestName, Rank_Stat_Joust, " +
                "RankedDuelControllerSeason, RankedDuelSeason, RankedDuelLeaves, RankedDuelControllerTier, " +
                "RankedDuelControllerPoints, RankedConquestSeason, RankedDuelControllerLeaves, " +
                "RankedDuelRank, RankedConquestControllerName, RankedJoustRank, RankedConquestControllerTier, " +
                "RankedJoustControllerLeaves, RankedConquestLosses, RankedConquestPoints, RankedConquestControllerPoints, " +
                "RankedConquestRank, RankedJoustPoints, RankedJoustControllerName, RankedConquestPrevRank, " +
                "RankedDuelTrend, RankedDuelControllerName, RankedJoustTrend, RankedConquestWins, Updated_At, Rank_Stat_Conquest_Controller, " +
                "Rank_Stat_Duel_Controller, Rank_Stat_Joust_Controller, RankedConquestRank_Stat, RankedConquestControllerRank_Stat, " +
                "RankedJoustRank_Stat, RankedJoustControllerRank_Stat, RankedDuelRank_Stat, RankedDuelControllerRank_Stat)" +
                    "VALUES(@activeID, @portal, @ID, @hz_player_name, @hz_gamer_tag, @name, @teamID, @teamName, @level, @mastery, @hoursPlayed, " +
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
                    "@rankConqPrevRank, @rankDuelTrend, @rankDuelContrName, @rankJoustTrend, @rankConqWins, @updatedAt, @rankStatCqContr, @rankStatDuContr, " +
                    "@rankStatJoContr, @rankConqRankStat, @rankConqContrRankStat, @rankJoustRankStat, @rankJoustContrRankStat, @rankDuelRankStat, @rankDuelContrRankStat) " +
                    "ON CONFLICT(ActivePlayerId) " +
                    "DO UPDATE SET " +
                    "Created_Datetime = @accCreated, hz_player_name = @hz_player_name, TeamId = @teamID, " +
                    "Team_Name = @teamName, Level = @level, MasteryLevel = @mastery, " +
                    "HoursPlayed = @hoursPlayed, Wins = @wins, Losses = @losses, Leaves = @leaves, " +
                    "Region = @region, Personal_Status_Message = @statusMess, Last_Login_Datetime = @lastLogin, Total_Worshippers = @totWorship, " +
                    "Total_Achievements = @totAchiev, Tier_Conquest = @tierConq, Tier_Joust = @tierJoust, Tier_Duel = @tierDuel, " +
                    "Avatar_URL = @avatarURL, RankedJoustControllerSeason = @rankJoustContrSeason, Rank_Stat_Duel = @rankStatDuel, " +
                    "RankedConquestControllerPrevRank = @rankConqContrPrevRank, RankedJoustControllerLosses = @rankJoustContrLoss, " +
                    "RankedDuelControllerTrend = @rankDuelContrTrend, RankedJoustControllerPrevRank = @rankJoustContrPrevRank, " +
                    "RankedDuelControllerWins = @rankDuelContrWins, Rank_Stat_Conquest = @rankStatConq, RankedDuelTier = @rankDuelTier, " +
                    "RankedConquestControllerLosses = @rankConqContrLoss, RankedConquestLeaves = @rankConqLeaves, RankedJoustName = @rankJoustName, " +
                    "RankedJoustLeaves = @rankJoustLeaves, RankedJoustTier = @rankJoustTier, RankedJoustSeason = @rankJoustSeason, " +
                    "RankedJoustWins = @rankJoustWins, RankedConquestControllerWins = @rankConqContrWins, RankedDuelControllerPrevRank = @rankDuelContrPrevRank, " +
                    "RankedConquestControllerTrend = @rankConqContrTrend, RankedJoustPrevRank = @rankJoustPrevRank, RankedJoustControllerWins = @rankJoustContrWins, " +
                    "RankedConquestTrend = @rankConqTrend, RankedDuelControllerLosses = @rankDuelContrLoss, RankedJoustLosses = @rankJoustLoss, " +
                    "RankedDuelLosses = @rankDuelLoss, RankedDuelControllerRank = @rankDuelContrRank, RankedJoustControllerPoints = @rankJoustContrPoints, " +
                    "RankedDuelWins = @rankDuelWins, RankedJoustControllerTier = @rankJoustContrTier, RankedDuelPoints = @rankDuelPoints, " +
                    "RankedDuelPrevRank = @rankDuelPrevRank, RankedConquestControllerLeaves = @rankConqContrLeaves, RankedJoustControllerRank = @rankJoustContrRank, " +
                    "RankedConquestControllerRank = @rankConqContrRank, RankedConquestTier = @rankConqTier, RankedJoustControllerTrend = @rankJoustContrTrend, " +
                    "RankedConquestControllerSeason = @rankConqContrSeason, RankedDuelName = @rankDuelName, RankedConquestName = @rankConqName, " +
                    "Rank_Stat_Joust = @rankStatJoust, RankedDuelControllerSeason = @rankDuelContrSeason, RankedDuelSeason = @rankDuelSeason, " +
                    "RankedDuelLeaves = @rankDuelLeaves, RankedDuelControllerTier = @rankDuelContrTier, RankedDuelControllerPoints = @rankDuelContrPoints, " +
                    "RankedConquestSeason = @rankConqSeason, RankedDuelControllerLeaves = @rankDuelContrLeaves, RankedDuelRank = @rankDuelRank, " +
                    "RankedConquestControllerName = @rankConqContrName, RankedJoustRank = @rankJoustRank, RankedConquestControllerTier = @rankConqContrTier, " +
                    "RankedJoustControllerLeaves = @rankJoustContrLeaves, RankedConquestLosses = @rankConqLoss, RankedConquestPoints = @rankConqPoints, " +
                    "RankedConquestControllerPoints = @rankConqContrPoints, RankedConquestRank = @rankConqRank, RankedJoustPoints = @rankJoustPoints, " +
                    "RankedJoustControllerName = @rankJoustContrName, RankedConquestPrevRank = @rankConqPrevRank, RankedDuelTrend = @rankDuelTrend, " +
                    "RankedDuelControllerName = @rankDuelContrName, RankedJoustTrend = @rankJoustTrend, RankedConquestWins = @rankConqWins, Updated_At = @updatedAt, " +
                    "Rank_Stat_Conquest_Controller = @rankStatCqContr, Rank_Stat_Duel_Controller = @rankStatDuContr, Rank_Stat_Joust_Controller = @rankStatJoContr, " +
                    "RankedConquestRank_Stat = @rankConqRankStat, RankedConquestControllerRank_Stat = @rankConqContrRankStat, " +
                    "RankedJoustRank_Stat = @rankJoustRankStat, RankedJoustControllerRank_Stat = @rankJoustContrRankStat, " +
                    "RankedDuelRank_Stat = @rankDuelRankStat, RankedDuelControllerRank_Stat = @rankDuelContrRankStat, portal_id = @portal";
            SQLiteConnection connection = new SQLiteConnection(LoadConnectionString());
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            await connection.OpenAsync();

            command.Parameters.AddWithValue("activeID", playerStats[0].ActivePlayerId);
            command.Parameters.AddWithValue("portal", portal);
            command.Parameters.AddWithValue("ID", playerStats[0].Id);
            command.Parameters.AddWithValue("hz_player_name", playerStats[0].hz_player_name);
            command.Parameters.AddWithValue("hz_gamer_tag", playerStats[0].hz_gamer_tag);
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
            command.Parameters.AddWithValue("rankStatCqContr", playerStats[0].Rank_Stat_Conquest_Controller);
            command.Parameters.AddWithValue("rankStatJoust", playerStats[0].Rank_Stat_Joust);
            command.Parameters.AddWithValue("rankStatJoContr", playerStats[0].Rank_Stat_Joust_Controller);
            command.Parameters.AddWithValue("rankStatDuel", playerStats[0].Rank_Stat_Duel);
            command.Parameters.AddWithValue("rankStatDuContr", playerStats[0].Rank_Stat_Duel_Controller);
            command.Parameters.AddWithValue("updatedAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            // Ranked Conquest
            command.Parameters.AddWithValue("rankConqName", playerStats[0].RankedConquest.Name);
            command.Parameters.AddWithValue("rankConqTier", playerStats[0].RankedConquest.Tier);
            command.Parameters.AddWithValue("rankConqWins", playerStats[0].RankedConquest.Wins);
            command.Parameters.AddWithValue("rankConqLoss", playerStats[0].RankedConquest.Losses);
            command.Parameters.AddWithValue("rankConqLeaves", playerStats[0].RankedConquest.Leaves);
            command.Parameters.AddWithValue("rankConqRankStat", playerStats[0].RankedConquest.Rank_Stat);
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
            command.Parameters.AddWithValue("rankConqContrRankStat", playerStats[0].RankedConquestController.Rank_Stat);
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
            command.Parameters.AddWithValue("rankJoustRankStat", playerStats[0].RankedJoust.Rank_Stat);
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
            command.Parameters.AddWithValue("rankJoustContrRankStat", playerStats[0].RankedJoustController.Rank_Stat);
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
            command.Parameters.AddWithValue("rankDuelRankStat", playerStats[0].RankedDuel.Rank_Stat);
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
            command.Parameters.AddWithValue("rankDuelContrRankStat", playerStats[0].RankedDuelController.Rank_Stat);
            command.Parameters.AddWithValue("rankDuelContrSeason", playerStats[0].RankedDuelController.Season);
            command.Parameters.AddWithValue("rankDuelContrTrend", playerStats[0].RankedDuelController.Trend);
            command.Parameters.AddWithValue("rankDuelContrTier", playerStats[0].RankedDuelController.Tier);
            command.Parameters.AddWithValue("rankDuelContrPoints", playerStats[0].RankedDuelController.Points);
            command.Parameters.AddWithValue("rankDuelContrRank", playerStats[0].RankedDuelController.Rank);
            command.Parameters.AddWithValue("rankDuelContrPrevRank", playerStats[0].RankedDuelController.PrevRank);
            
            await command.ExecuteNonQueryAsync();
            connection.Close();
        }

        public static async Task<List<PlayerSpecial>> GetPlayerSpecials(string id) // Working as intended
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = await cnn.QueryAsync<PlayerSpecial>($"SELECT * FROM playersSpecial WHERE active_player_id LIKE '%{id}%'", new DynamicParameters());
                return output.ToList();
            }
        }

        public static async Task SetPlayerSpecials(int id, string Name, ulong? discordID = null, int? strValue = null, int? proValue = null, string specValue = "")
        {
            try
            {// to fix
                if (discordID.HasValue)
                {
                    using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                    {
                        await cnn.ExecuteAsync($"INSERT INTO playersSpecial(active_player_id, Name, discordID) " +
                            $"VALUES({id}, \"{Name}\", {discordID}) " +
                            $"ON CONFLICT(active_player_id) " +
                            $"DO UPDATE SET discordID = {discordID}");
                    }
                }
                if (strValue.HasValue)
                {
                    using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                    {
                        await cnn.ExecuteAsync($"INSERT INTO playersSpecial(active_player_id, Name, streamer_bool) " +
                            $"VALUES({id}, \"{Name}\", {strValue}) " +
                            $"ON CONFLICT(active_player_id) " +
                            $"DO UPDATE SET streamer_bool = {strValue}");
                    }
                }
                if (proValue.HasValue)
                {
                    using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                    {
                        await cnn.ExecuteAsync($"INSERT INTO playersSpecial(active_player_id, Name, pro_bool) " +
                            $"VALUES({id}, \"{Name}\", {proValue}) " +
                            $"ON CONFLICT(active_player_id) " +
                            $"DO UPDATE SET pro_bool = {proValue}");
                    }
                }
                if (specValue != "" || specValue != null)
                {
                    using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                    {
                        await cnn.ExecuteAsync($"INSERT INTO playersSpecial(active_player_id, Name, special) " +
                            $"VALUES({id}, \"{Name}\", \"{specValue}\") " +
                            $"ON CONFLICT(active_player_id) " +
                            $"DO UPDATE SET special = \"{specValue}\"");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SetPlayerSpecials()\n" + ex.Message);
            }
        }

        public static async Task<List<PlayerStats>> GetAllPlayers()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = await cnn.QueryAsync<PlayerStats>($"SELECT * FROM players", new DynamicParameters());
                return output.ToList();
            }
        }

        public static List<string> PlayersInDbCount() // Working as intended
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<string>($"SELECT count(*) FROM players", new DynamicParameters());
                return output.ToList();
            }
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
        
        public static async Task InsertItems(List<GetItems.Item> items)
        {
            string sql = $"INSERT INTO Items(ActiveFlag, ChildItemId, DeviceName, IconId, ItemDescription, SecondaryDescription, ItemId, " +
                $"ItemTier, Price, RestrictedRoles, RootItemId, ShortDesc, StartingItem, Type, itemIcon_URL) " +
                            $"VALUES(@activeflag, @childitemid, @devicename, @iconid, @itemdesc, @secdesc, @itemid, @itemtier, " +
                            $"@price, @restrictedroles, @rootitemid, @shortdesc, @startingitem, @type, @itemicon_URL)  " +
                            $"ON CONFLICT(ItemId) " +
                            $"DO UPDATE SET ActiveFlag = @activeflag, ChildItemId = @childitemid, DeviceName = @devicename, IconId = @iconid, " +
                            $"ItemDescription = @itemdesc, SecondaryDescription = @secdesc, ItemTier = @itemtier, Price = @price, " +
                            $"RestrictedRoles = @restrictedroles, RootItemId = @rootitemid, ShortDesc = @shortdesc, " +
                            $"StartingItem = @startingitem, Type = @type, itemIcon_URL = @itemicon_URL";
            try
            {
                SQLiteConnection connection = new SQLiteConnection(LoadConnectionString());
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                int counter = 0;
                for (int i = 0; i < items.Count; i++)
                {
                    await connection.OpenAsync();

                    command.Parameters.AddWithValue("activeflag", items[i].ActiveFlag);
                    command.Parameters.AddWithValue("childitemid", items[i].ChildItemId);
                    command.Parameters.AddWithValue("devicename", items[i].DeviceName);
                    command.Parameters.AddWithValue("iconid", items[i].IconId);
                    command.Parameters.AddWithValue("itemdesc", items[i].ItemDescription.Description);
                    command.Parameters.AddWithValue("secdesc", items[i].ItemDescription.SecondaryDescription);
                    command.Parameters.AddWithValue("itemid", items[i].ItemId);
                    command.Parameters.AddWithValue("itemtier", items[i].ItemTier);
                    command.Parameters.AddWithValue("price", items[i].Price);
                    command.Parameters.AddWithValue("restrictedroles", items[i].RestrictedRoles);
                    command.Parameters.AddWithValue("rootitemid", items[i].RootItemId);
                    command.Parameters.AddWithValue("shortdesc", items[i].ShortDesc);
                    command.Parameters.AddWithValue("startingitem", items[i].StartingItem);
                    command.Parameters.AddWithValue("type", items[i].Type);
                    command.Parameters.AddWithValue("itemicon_URL", items[i].itemIcon_URL);
                    counter++;
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                }
                Console.WriteLine(counter);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in InsertItems()\n" + ex.Message);
            }
        }

        public static async Task<List<Item>> GetActiveItems(int tier)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = await cnn.QueryAsync<Item>($"SELECT * FROM Items WHERE ItemTier = {tier} AND ActiveFlag LIKE '%y%'", new DynamicParameters());
                return output.ToList();
            }
        }

        public static async Task SaveGods(List<Gods.God> gods)
        {
            string sql = $"INSERT INTO Gods(id, Name, Pantheon, Pros, Roles, Title, Type, godIcon_URL) " +
                    $"VALUES(@id, @Name, @Pantheon, @Pros, @Roles, @Title, @Type, @godIcon_URL) " +
                    $"ON CONFLICT(id) " +
                    $"DO UPDATE SET godIcon_URL = @godIcon_URL";
            try
            {
                SQLiteConnection connection = new SQLiteConnection(LoadConnectionString());
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                for (int i = 0; i < gods.Count; i++)
                {
                    await connection.OpenAsync();

                    command.Parameters.AddWithValue("id", gods[i].id);
                    command.Parameters.AddWithValue("Name", gods[i].Name);
                    command.Parameters.AddWithValue("Pantheon", gods[i].Pantheon);
                    command.Parameters.AddWithValue("Pros", gods[i].Pros);
                    command.Parameters.AddWithValue("Roles", gods[i].Roles);
                    command.Parameters.AddWithValue("Title", gods[i].Title);
                    command.Parameters.AddWithValue("Type", gods[i].Type);
                    command.Parameters.AddWithValue("godIcon_URL", gods[i].godIcon_URL);
                    Console.WriteLine($"{i} {gods[i].Name}");
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SaveGods()\n" + ex.Message);
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
            public string serverName { get; set; }
            public bool statusBool { get; set; }
            public ulong statusChannel { get; set; } 
            public string timezone { get; set; }
        }
    }
}
