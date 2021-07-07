using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ThothBotCore.Models;

namespace ThothBotCore.Connections
{
    public static class HiRezWebAPI
    {
        private static readonly HttpClientHandler handler = new();
        private static readonly HttpClient httpClient = new(handler, false);

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
    }
}
