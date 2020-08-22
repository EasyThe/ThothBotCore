using MongoDB.Driver;
using System.Threading.Tasks;
using static ThothBotCore.Connections.Models.Player;
using ThothBotCore.Discord.Entities;

namespace ThothBotCore.Storage.Implementations
{
    public static class MongoConnection
    {
        private static MongoClient client;
        public static IMongoDatabase database;
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

        public static async Task SavePlayer(PlayerStats playerStats)
        {
            var result = await GetDatabase().GetCollection<PlayerStats>("players").ReplaceOneAsync(
                filter: x => x.ActivePlayerId == playerStats.ActivePlayerId,
                replacement: playerStats,
                options: new ReplaceOptions { IsUpsert = true });
        }
    }
}
