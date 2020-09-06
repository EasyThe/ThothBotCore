using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Models;
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
                await Reporter.SendError($"**Error in InsertServerStatusUpdates\n{ex.Message}");
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

        public static async Task SetNotifChannel(ulong serverID, string serverName, ulong statusChannel) // Working as intended
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                await cnn.ExecuteAsync($"INSERT INTO serverConfig(_id, serverName, statusBool, statusChannel) " +
                    $"VALUES({serverID}, \"{serverName}\", 1, {statusChannel}) " +
                    $"ON CONFLICT(_id) " +
                    $"DO UPDATE SET statusChannel = \"{statusChannel}\", statusBool = 1");
            }
        }

        public static async Task SetPrefix(ulong serverID, string serverName, string prefix) // Working as intended
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                await cnn.ExecuteAsync($"INSERT INTO serverConfig(_id, serverName, prefix) " +
                    $"VALUES({serverID}, \"{serverName}\", \"{prefix}\") " +
                    $"ON CONFLICT(_id) " +
                    $"DO UPDATE SET prefix = \"{prefix}\", serverName = \"{serverName}\"");
            }
        }

        public static async Task SetGuild(ulong serverID, string serverName) // Working as intended
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                if (serverName.Contains("\""))
                {
                    serverName = serverName.Replace("\"", "\"\"");
                }
                await cnn.ExecuteAsync($"INSERT OR IGNORE INTO serverConfig(_id, prefix, serverName) " +
                    $"VALUES({serverID}, \"{Credentials.botConfig.prefix}\", \"{serverName}\")");
            }
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

        public static async Task<List<PlayerSpecial>> GetPlayerSpecialsByDiscordID(ulong id) // Working as intended
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = await cnn.QueryAsync<PlayerSpecial>($"SELECT * FROM playersSpecial WHERE discordID LIKE '%{id}%'", new DynamicParameters());
                return output.ToList();
            }
        }

        public static async Task SetPlayerSpecials(int id, string Name, ulong? discordID = null, int? strValue = null, string strLink = "", int? proValue = null)
        {
            try
            {// to fix
                if (discordID.HasValue)
                {
                    using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                    {
                        await cnn.ExecuteAsync($"INSERT INTO playersSpecial(_id, Name, discordID) " +
                            $"VALUES({id}, \"{Name}\", {discordID}) " +
                            $"ON CONFLICT(_id) " +
                            $"DO UPDATE SET discordID = {discordID}");
                    }
                }
                if (strValue.HasValue)
                {
                    using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                    {
                        await cnn.ExecuteAsync($"INSERT INTO playersSpecial(_id, Name, streamer_bool, streamer_link) " +
                            $"VALUES({id}, \"{Name}\", {strValue}, \"{strLink}\") " +
                            $"ON CONFLICT(_id) " +
                            $"DO UPDATE SET streamer_bool = {strValue}, streamer_link = \"{strLink}\"");
                    }
                }
                if (proValue.HasValue)
                {
                    using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                    {
                        await cnn.ExecuteAsync($"INSERT INTO playersSpecial(_id, Name, pro_bool) " +
                            $"VALUES({id}, \"{Name}\", {proValue}) " +
                            $"ON CONFLICT(_id) " +
                            $"DO UPDATE SET pro_bool = {proValue}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SetPlayerSpecials()\n" + ex.Message);
            }
        }

        public static async Task RemoveLinkedAccount(ulong id)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                await cnn.ExecuteAsync($"DELETE FROM playersSpecial WHERE discordID = {id}");
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

        public static async Task<List<PlayerSpecial>> GetAllPlayerSpecials()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = await cnn.QueryAsync<PlayerSpecial>($"SELECT * FROM playersSpecial", new DynamicParameters());
                return output.ToList();
            }
        }

        public static async Task<List<ServerConfig>> GetAllGuilds()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = await cnn.QueryAsync<ServerConfig>($"SELECT * FROM serverConfig", new DynamicParameters());
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

        public static List<string> LinkedPlayersInDBCount() // Working as intended
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<string>($"SELECT count(*) FROM playersSpecial WHERE discordID IS NOT NULL", new DynamicParameters());
                return output.ToList();
            }
        }

        public static List<string> CountOfStatusUpdatesActivatedInDB() // Working as intended
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<string>($"SELECT count(*) FROM serverConfig WHERE statusBool = 1", new DynamicParameters());
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

        public static List<Gods.God> LoadAllGods() // Get all gods
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<Gods.God>($"SELECT * FROM Gods", new DynamicParameters());
                return output.ToList();
            }
        }

        public static List<Gods.God> LoadAllGodsWithLessInfo() // Get random god for rgod command
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<Gods.God>($"SELECT Name, godIcon_URL, Pantheon, Roles, Title, Type, DomColor, Emoji, id FROM Gods", new DynamicParameters());
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

        public static async Task<string> GetGodEmoji(string godname)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                if (godname.Contains("'"))
                {
                    godname = godname.Replace("'", "''");
                }
                if (godname.ToLowerInvariant().Contains("change"))
                {
                    godname = "chang";
                }
                var output = await cnn.QueryAsync<Gods.God>($"SELECT Emoji,godIcon_URL FROM Gods WHERE Name LIKE '%{godname}%'", new DynamicParameters());
                var returnlist = output.ToList();
                if (returnlist[0].Emoji == null || returnlist[0].Emoji == "")
                {
                    await Reporter.SendError("Found missing emoji for " + godname);

                    // do tuk stignahme, ne uspqh da izmislq kak da vzema hirezapi za da pusna updatedb ot utils..
                    //Utils.UpdateDb
                    return "";
                }
                else
                {
                    return returnlist[0].Emoji;
                }
            }
        }

        public static async Task InsertItems(List<GetItems.Item> items)
        {
            string sql = $"INSERT INTO Items(ActiveFlag, ChildItemId, DeviceName, ItemBenefits, IconId, ItemDescription, SecondaryDescription, ItemId, " +
                $"ItemTier, Price, RestrictedRoles, RootItemId, ShortDesc, StartingItem, Type, itemIcon_URL) " +
                            $"VALUES(@activeflag, @childitemid, @devicename, @itembenefits, @iconid, @itemdesc, @secdesc, @itemid, @itemtier, " +
                            $"@price, @restrictedroles, @rootitemid, @shortdesc, @startingitem, @type, @itemicon_URL)  " +
                            $"ON CONFLICT(ItemId) " +
                            $"DO UPDATE SET ActiveFlag = @activeflag, ChildItemId = @childitemid, DeviceName = @devicename, ItemBenefits = @itembenefits, " +
                            $"IconId = @iconid, ItemDescription = @itemdesc, SecondaryDescription = @secdesc, ItemTier = @itemtier, Price = @price, " +
                            $"RestrictedRoles = @restrictedroles, RootItemId = @rootitemid, ShortDesc = @shortdesc, " +
                            $"StartingItem = @startingitem, Type = @type, itemIcon_URL = @itemicon_URL";
            try
            {
                SQLiteConnection connection = new SQLiteConnection(LoadConnectionString());
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                for (int i = 0; i < items.Count; i++)
                {
                    await connection.OpenAsync();

                    command.Parameters.AddWithValue("activeflag", items[i].ActiveFlag);
                    command.Parameters.AddWithValue("childitemid", items[i].ChildItemId);
                    command.Parameters.AddWithValue("devicename", items[i].DeviceName);
                    StringBuilder benefits = new StringBuilder();
                    for (int b = 0; b < items[i].ItemDescription.Menuitems.Count; b++)
                    {
                        benefits.Append($"{items[i].ItemDescription.Menuitems[b].Value} {items[i].ItemDescription.Menuitems[b].Description}");
                        benefits.Append("\n");
                    }
                    command.Parameters.AddWithValue("itembenefits", benefits.ToString());
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
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in InsertItems()\n" + ex.Message);
            }
        }

        public static async Task<List<Item>> GetActiveTierItems(int tier)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = await cnn.QueryAsync<Item>($"SELECT * FROM Items WHERE ItemTier = {tier} AND ActiveFlag LIKE '%y%' AND Type LIKE '%Item%'", new DynamicParameters());
                return output.ToList();
            }
        }
        
        public static async Task<List<Item>> GetActiveItemsByGodType(string type, string role)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = await cnn.QueryAsync<Item>($"SELECT * FROM Items " +
                    $"WHERE ItemTier = 3 " +
                    $"AND ActiveFlag LIKE '%y%' " +
                    $"AND Type LIKE '%Item%' " +
                    $"AND GodType LIKE '%{type}%' " +
                    $"AND GodType NOT LIKE '%boots%' " +
                    $"AND RestrictedRoles NOT LIKE '%{role}%'", new DynamicParameters());
                return output.ToList();
            }
        }

        public static async Task<List<Item>> GetActiveActives()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = await cnn.QueryAsync<Item>($"SELECT * FROM Items " +
                    $"WHERE ItemTier = 2 " +
                    $"AND ActiveFlag LIKE '%y%' " +
                    $"AND Type LIKE '%Active%' " +
                    $"AND DeviceName NOT LIKE '%Relic%'", new DynamicParameters());
                return output.ToList();
            }
        }

        public static async Task<List<Item>> GetAllItems()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = await cnn.QueryAsync<Item>($"SELECT * FROM Items", new DynamicParameters());
                return output.ToList();
            }
        }

        public static async Task<List<Item>> GetSpecificItem(string itemname)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = await cnn.QueryAsync<Item>($"SELECT * FROM Items WHERE DeviceName LIKE '%{itemname}%' AND ActiveFlag LIKE '%y%' AND ItemTier NOT LIKE '%4%'", new DynamicParameters());
                return output.ToList();
            }
        }

        public static async Task<List<Item>> GetSpecificItemByID(int id)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = await cnn.QueryAsync<Item>($"SELECT * FROM Items WHERE ItemId = {id} AND ActiveFlag LIKE '%y%' AND ItemTier NOT LIKE '%4%'", new DynamicParameters());
                return output.ToList();
            }
        }

        public static async Task<List<Item>> GetBootsOrShoes(string className)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = await cnn.QueryAsync<Item>($"SELECT * FROM Items WHERE GodType LIKE '%{className}, boots%' AND ActiveFlag LIKE '%y%'", new DynamicParameters());
                return output.ToList();
            }
        }

        public static async Task SaveGods(List<Gods.God> gods)
        {
            string sql = $"INSERT INTO Gods(id, Name, Pantheon, Pros, Roles, Title, Type, godIcon_URL, OnFreeRotation, latestGod) " +
                    $"VALUES(@id, @Name, @Pantheon, @Pros, @Roles, @Title, @Type, @godIcon_URL, @OnFreeRotation, @latestGod) " +
                    $"ON CONFLICT(id) " +
                    $"DO UPDATE SET Pros = @Pros, Roles = @Roles, Title = @Title, Type = @Type, godIcon_URL = @godIcon_URL, " +
                    $"OnFreeRotation = @OnFreeRotation, latestGod = @latestGod";
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
                    command.Parameters.AddWithValue("OnFreeRotation", gods[i].OnFreeRotation);
                    command.Parameters.AddWithValue("latestGod", gods[i].latestGod);
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

        public static void SaveGodDomColor(int id, int color)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.ExecuteAsync($"UPDATE Gods SET DomColor = {color} WHERE id = '{id}'");
            }
        }

        public static void SaveItemDomColor(int ItemId, int color)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.ExecuteAsync($"UPDATE Items SET DomColor = {color} WHERE ItemId = '{ItemId}'");
            }
        }

        public static async Task InsertEmojiForGod(string godName, string emoji)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                await cnn.ExecuteAsync($"UPDATE Gods SET Emoji = '{emoji}' WHERE Name LIKE '%{godName}%'");
            }
        }

        public static async Task InsertEmojiForItem(string itemName, string emoji)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                await cnn.ExecuteAsync($"UPDATE Items SET Emoji = '{emoji}' WHERE DeviceName LIKE '%{itemName}%' AND ItemTier = 3");
            }
        }

        private static string LoadConnectionString(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
    }
}
