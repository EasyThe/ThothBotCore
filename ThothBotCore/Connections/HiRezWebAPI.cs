using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ThothBotCore.Models;
using ThothBotCore.Storage.Implementations;

namespace ThothBotCore.Connections
{
    public static class HiRezWebAPI
    {
        private static readonly HttpClientHandler handler = new();
        private static readonly HttpClient httpClient = new(handler, false);
        private static readonly BotSettingsModel settings = MongoConnection.GetSettings();

        public static async Task<List<WebAPIPostsModel>> FetchPostsAsync()
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://cms.smitegame.com/wp-json/smite-api/get-posts/1");
            var response = await httpClient.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<WebAPIPostsModel>>(json);
        }
        public static async Task<WebAPIPostModel> GetPostBySlugAsync(string slug)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"https://cms.smitegame.com/wp-json/smite-api/get-post/1?slug={slug}");
            var response = await httpClient.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<WebAPIPostModel>(json);
        }
        public static async Task<SmiteNewsModel> GetLandingPanel()
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "http://cds.q6u4m8x5.hwcdn.net/LandingPanel/Smite/live//landingpanel.json");
            var response = await httpClient.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<SmiteNewsModel>(json);
        }
        public static async Task<SPLSchedule> GetEsportsSchedule()
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"https://esports.hirezstudios.com/esportsAPI/smite/schedule/{settings.s[4]}");
            var response = await httpClient.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<SPLSchedule>(json);
            }
            return new SPLSchedule();
        }
        public static async Task<List<SPLStandings>> GetEsportsStandings()
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"https://esports.hirezstudios.com/esportsAPI/smite/standings/{settings.s[4]}");
            var response = await httpClient.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<List<SPLStandings>>(json);
            }
            return new List<SPLStandings>();
        }
    }
}
