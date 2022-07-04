using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Models.Challonge;

namespace ThothBotCore.Connections
{
    public static class ChallongeAPI
    {
        private static readonly HttpClientHandler handler = new();
        private static readonly HttpClient httpClient = new(handler, false);
        private static readonly string CommunityId = "75d206613babbd4b3933dde3";

        public static async Task<string> CreateTournament()
        {
            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.challonge.com/v1/tournaments.json"))
                {
                    request.Content.Headers.Add("", "");
                    var response = await httpClient.SendAsync(request);
                    string json = await response.Content.ReadAsStringAsync();
                    return json;
                }
            }
        }
        public static async Task<List<ChallongeMatchModel>> GetMatches(string tournament, string state)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, 
                $"https://api.challonge.com/v1/tournaments/{CommunityId}-{tournament}/matches.json?api_key={Credentials.botConfig.challongeKey}&state={(state.Length == 0 ? "all" : state)}");
            var response = await httpClient.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ChallongeMatchModel>>(json);
        }
        public static async Task<List<ChallongeParticipantsModel>> GetParticipants(string tournament)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://api.challonge.com/v1/tournaments/{CommunityId}-{tournament}/participants.json?api_key={Credentials.botConfig.challongeKey}");
            var response = await httpClient.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ChallongeParticipantsModel>>(json);
        }
    }
}
