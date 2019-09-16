using System.Net.Http;
using System.Threading.Tasks;
using ThothBotCore.Discord.Entities;

namespace ThothBotCore.Connections
{
    public class ChallongeAPI
    {
        public async Task<string> CreateTournament()
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
    }
}
