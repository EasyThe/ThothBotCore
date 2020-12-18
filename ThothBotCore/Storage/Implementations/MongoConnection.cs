using System.Threading.Tasks;
using MongoDB.Driver;
using static ThothBotCore.Connections.Models.Player;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Models;
using System.Linq;
using System.Collections.Generic;

namespace ThothBotCore.Storage.Implementations
{
    public class MongoConnection
    {
        private static MongoClient client;
        public static IMongoDatabase database;

        private static readonly ReplaceOptions replaceOptions = new ReplaceOptions { IsUpsert = true };
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

        // Players
        public static async Task SavePlayerAsync(PlayerStats playerStats)
        {
            await GetDatabase().GetCollection<PlayerStats>("players").ReplaceOneAsync(
                filter: x => x.ActivePlayerId == playerStats.ActivePlayerId,
                replacement: playerStats,
                options: replaceOptions);
        }
        public static async Task<long> PlayersCount()
        {
            return await GetDatabase().GetCollection<PlayerStats>("players").CountDocumentsAsync(_ => true);
        }

        // Player Specials
        public static async Task<PlayerSpecial> GetPlayerSpecialsByPlayerIdAsync(int playerID)
        {
            var result = await GetDatabase().GetCollection<PlayerSpecial>("player_specials").FindAsync(x => x._id == playerID);
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
        public static async Task<long> LinkedPlayersCount()
        {
            return await GetDatabase().GetCollection<PlayerSpecial>("player_specials").CountDocumentsAsync(filter: x => x.discordID != 0);
        }
        // Communities
        public static async Task SaveOrCreateCommunityAsync(CommunityModel community)
        {
            await GetDatabase().GetCollection<CommunityModel>("communities").ReplaceOneAsync(
                filter: x => x.Name == community.Name,
                replacement: community,
                options: replaceOptions);
        }
        public static async Task<CommunityModel> GetCommunityAsync(string name)
        {
            var result = await GetDatabase().GetCollection<CommunityModel>("communities").FindAsync(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant());
            return await result.FirstOrDefaultAsync();
        }
        public static List<CommunityModel> GetAllCommunities()
        {
            return GetDatabase().GetCollection<CommunityModel>("communities").AsQueryable().ToList();
        }
        // Badges
        public static async Task SaveOrCreateBadgeAsync(BadgeModel badge)
        {
            await GetDatabase().GetCollection<BadgeModel>("badges").ReplaceOneAsync(
                filter: x => x.Key == badge.Key,
                replacement: badge,
                options: replaceOptions);
        }
        public static async Task<BadgeModel> GetBadgeAsync(string key)
        {
            var result = await GetDatabase().GetCollection<BadgeModel>("badges").FindAsync(x => x.Key == key);
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
                filter: x => x.id == god.id,
                replacement: god,
                options: replaceOptions);
        }
        public static List<Gods.God> GetAllGods()
        {
            return GetDatabase().GetCollection<Gods.God>("gods").AsQueryable().ToList();
        }
        public static async Task<Gods.God> LoadGod(string godname)
        {
            var result = await GetDatabase().GetCollection<Gods.God>("gods").FindAsync(filter: x => x.Name.Contains(godname));
            return await result.FirstOrDefaultAsync();
        }

        // Items
        public static List<GetItems.Item> GetAllItems()
        {
            return GetDatabase().GetCollection<GetItems.Item>("items").AsQueryable().ToList();
        }
        public static async Task<List<GetItems.Item>> GetActiveActivesAsync()
        {
            var filter = Builders<GetItems.Item>.Filter.Where(x => x.ItemTier == 2 && x.ActiveFlag == "y" && x.Type == "Active" && !x.DeviceName.Contains("Relic"));
            var result = await GetDatabase().GetCollection<GetItems.Item>("items").FindAsync(
                filter: filter);
            return await result.ToListAsync();
        }
        public static async Task<List<GetItems.Item>> GetBootsOrShoesAsync(string godtype)
        {
            var filter = Builders<GetItems.Item>.Filter.Where(x => x.ItemTier == 3 && x.ActiveFlag == "y" && x.GodType.Contains($"{godtype}, boots"));
            var result = await GetDatabase().GetCollection<GetItems.Item>("items").FindAsync(
                filter: filter);
            return await result.ToListAsync();
        }
        public static async Task<List<GetItems.Item>> GetActiveItemsByGodTypeAsync(string type, string role)
        {
            var filter = Builders<GetItems.Item>.Filter.Where(
                x => x.ItemTier == 3 && 
                x.ActiveFlag == "y" && 
                x.Type == "Item" &&
                x.GodType.Contains(type) &&
                !x.GodType.Contains("boots") &&
                !x.RestrictedRoles.Contains(role));
            var result = await GetDatabase().GetCollection<GetItems.Item>("items").FindAsync(
                filter: filter);
            return await result.ToListAsync();
        }
        public static async Task<List<GetItems.Item>> GetSpecificItemAsync(string searchWord)
        {
            var filter = Builders<GetItems.Item>.Filter.Where(x=>
                x.ActiveFlag == "y" &&
                x.DeviceName.ToLowerInvariant().Contains(searchWord.ToLowerInvariant()));
            var result = await GetDatabase().GetCollection<GetItems.Item>("items").FindAsync(
                filter: filter);
            return await result.ToListAsync();
        }
        public static async Task<List<GetItems.Item>> GetSpecificItemByIDAsync(int id)
        {
            var filter = Builders<GetItems.Item>.Filter.Where(x =>
                x.ActiveFlag == "y" &&
                x.ItemId == id);
            var result = await GetDatabase().GetCollection<GetItems.Item>("items").FindAsync(
                filter: filter);
            return await result.ToListAsync();
        }
        public static async Task SaveItemAsync(GetItems.Item item)
        {
            await GetDatabase().GetCollection<GetItems.Item>("items").ReplaceOneAsync(
                filter: x => x.ItemId == item.ItemId,
                replacement: item,
                options: replaceOptions);
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

        // Misc
        public static BotSettingsModel GetSettings()
        {
            return GetDatabase().GetCollection<BotSettingsModel>("settings").AsQueryable().ToList()[0];
        }
    }
}
