using System.Collections.Generic;
using System.Threading.Tasks;
using ThothBotCore.Connections;
using ThothBotCore.Connections.Models;

namespace ThothBotCore.Utilities.Smite
{
    public class SmiteStatsUtils
    {
        public static async Task<List<SearchPlayers>> SearchPlayersUtil(HiRezAPI hiRezAPI, string username)
        {
            var searchPlayer = await hiRezAPI.SearchPlayer(username);
            var realSearchPlayers = new List<SearchPlayers>();
            if (searchPlayer.Count != 0)
            {
                foreach (var player in searchPlayer)
                {
                    if (player.Name.ToLowerInvariant() == username.ToLowerInvariant())
                    {
                        realSearchPlayers.Add(player);
                    }
                }
            }
            return realSearchPlayers;
        }
    }
}
