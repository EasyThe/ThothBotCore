using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Models;

namespace ThothBotCore.Connections
{
    public class TrelloAPI
    {
        public async Task<List<TrelloModel>> GetTrelloCards()
        {
            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.trello.com/1/boards/d4fJtBlo/cards?key={Credentials.botConfig.trelloKey}&token={Credentials.botConfig.trelloToken}"))
                {
                    var response = await httpClient.SendAsync(request);
                    string json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<TrelloModel>>(json);
                }
            }
        }

        public async Task<string> GetTrelloCardsJSON()
        {
            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.trello.com/1/boards/d4fJtBlo/cards?key={Credentials.botConfig.trelloKey}&token={Credentials.botConfig.trelloToken}"))
                {
                    var response = await httpClient.SendAsync(request);
                    string json = await response.Content.ReadAsStringAsync();
                    return json;
                }
            }
        }
    }
}
