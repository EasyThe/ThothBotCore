using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Models;
using ThothBotCore.Utilities;

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
                        $"VALUES(\'{id}\', \'{inciID}\', \'{type}\', \'{status}\', \'{name}\', \'{body}\', \'{createdAt}\')");
                }
            }
            catch (Exception ex)
            {
                await Reporter.SendErrorAsync($"**Error in InsertServerStatusUpdates\n{ex.Message}");
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
                    $"WHERE _id = {serverID}");
            }
        }
        public static async Task SetPrefix(ulong serverID, string prefix) // Working as intended
        {
            using IDbConnection cnn = new SQLiteConnection(LoadConnectionString());
            await cnn.ExecuteAsync($"INSERT INTO serverConfig(_id, prefix) " +
                $"VALUES({serverID}, '{prefix}') " +
                $"ON CONFLICT(_id) " +
                $"DO UPDATE SET prefix = '{prefix}'");
        }
        public static async Task SetGuild(ulong serverID) // Working as intended
        {
            using IDbConnection cnn = new SQLiteConnection(LoadConnectionString());
            await cnn.ExecuteAsync($"INSERT OR IGNORE INTO serverConfig(_id, prefix) " +
                $"VALUES({serverID}, '{Credentials.botConfig.prefix}')");
        }
        public static async Task<List<ServerConfig>> GetServerConfig(ulong id) // Get prefix for guild. Working as intended
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = await cnn.QueryAsync<ServerConfig>($"SELECT * FROM serverConfig WHERE _id LIKE '%{id}%'", new DynamicParameters());
                return output.ToList();
            }
        }
        public static async Task DeleteServerConfig(ulong id) // Deleting serverConfig if exists on server leaving.
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                await cnn.ExecuteAsync($"DELETE FROM serverConfig WHERE _id = {id}");
            }
        }
        // remove those here after migration
        public static List<string> CountOfStatusUpdatesActivatedInDB() // Working as intended
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<string>($"SELECT count(*) FROM serverConfig WHERE statusBool = 1", new DynamicParameters());
                return output.ToList();
            }
        }
        private static string LoadConnectionString(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
    }
}
