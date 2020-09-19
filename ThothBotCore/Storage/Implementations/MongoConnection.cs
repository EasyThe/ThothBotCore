using System.Threading.Tasks;
using MongoDB.Driver;
using static ThothBotCore.Connections.Models.Player;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Models;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ThothBotCore.Storage.Implementations
{
    public class MongoConnection
    {
        private static MongoClient client;
        public static IMongoDatabase database;

        private static ReplaceOptions replaceOptions = new ReplaceOptions { IsUpsert = true };
        public static IMongoDatabase GetDatabase()
        {
            if (client == null)
            {
                client = new MongoClient(Credentials.botConfig.MongoDbURL);
            }
            if (database == null)
            {
                database = client.GetDatabase("thothbot");
            }
            return database;
        }
        public static async Task SavePlayerAsync(PlayerStats playerStats)
        {
            await GetDatabase().GetCollection<PlayerStats>("players").ReplaceOneAsync(
                filter: x => x.ActivePlayerId == playerStats.ActivePlayerId,
                replacement: playerStats,
                options: replaceOptions);
        }
        public static async Task<long> PlayersCount()
        {//not sure if works
            return await GetDatabase().GetCollection<PlayerStats>("players").CountDocumentsAsync(null);
        }
        // Player Specials
        public static async Task<PlayerSpecial> GetPlayerSpecialsByPlayerIdAsync(int playerID)
        {
            var result = await GetDatabase().GetCollection<PlayerSpecial>("player_specials").FindAsync(x=> x._id == playerID);
            return await result.FirstOrDefaultAsync();
        }
        public static async Task<PlayerSpecial> GetPlayerSpecialsByDiscordIdAsync(ulong discordID)
        {
            var result = await GetDatabase().GetCollection<PlayerSpecial>("player_specials").FindAsync(x => x.discordID == discordID);
            return await result.FirstOrDefaultAsync();
        }
        public static async Task SavePlayerSpecialsAsync(PlayerSpecial player)
        {
            await GetDatabase().GetCollection<PlayerSpecial>("player_specials").ReplaceOneAsync(
                filter: x => x._id == player._id,
                replacement: player,
                options: replaceOptions);
        }
        public static async Task UnlinkPlayerAsync(ulong discordID)
        {
            ulong id = 0;
            await GetDatabase().GetCollection<PlayerSpecial>("player_specials").UpdateOneAsync(
                filter: x => x.discordID == discordID,
                update: Builders<PlayerSpecial>.Update.Set(x => x.discordID, id));
        }
        // Badges
        public static async Task SaveOrCreateBadgeAsync(BadgeModel badge)
        {
            await GetDatabase().GetCollection<BadgeModel>("badges").ReplaceOneAsync(
                filter: x=> x.Key == badge.Key,
                replacement: badge,
                options: replaceOptions);
        }
        public static async Task<BadgeModel> GetBadgeAsync(string key)
        {
            var result = await GetDatabase().GetCollection<BadgeModel>("badges").FindAsync(x=> x.Key == key);
            return await result.FirstOrDefaultAsync();
        }
        public static List<BadgeModel> GetAllBadges()
        {
            return GetDatabase().GetCollection<BadgeModel>("badges").AsQueryable().ToList();
        }

        // Gods
        public static async Task SaveGodAsync(Gods.God god)
        {
            await GetDatabase().GetCollection<Gods.God>("gods").ReplaceOneAsync(
                filter: x=> x.id == god.id,
                replacement: god,
                options: replaceOptions);
        }

        public static List<Gods.God> GetAllGods()
        {
            return GetDatabase().GetCollection<Gods.God>("gods").AsQueryable().ToList();
        }

        // Tips
        public static async Task SaveTipAsync(TipsModel tip)
        {
            await GetDatabase().GetCollection<TipsModel>("tips").ReplaceOneAsync(
                filter: x=> x._id == tip._id,
                replacement: tip,
                options: replaceOptions);
        }

        public static List<TipsModel> GetAllTips()
        {
            return GetDatabase().GetCollection<TipsModel>("tips").AsQueryable().ToList();
        }
    }
}
