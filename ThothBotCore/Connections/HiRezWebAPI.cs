using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ThothBotCore.Models;

namespace ThothBotCore.Connections
{
    public static class HiRezWebAPI
    {
        private static readonly HttpClientHandler handler = new HttpClientHandler();

        public static async Task<List<WebAPIPostsModel>> FetchPostsAsync()
        {
            using var httpClient = new HttpClient(handler, false);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://cms.smitegame.com/wp-json/smite-api/get-posts/1");
            var response = await httpClient.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<WebAPIPostsModel>>(json);
        }
        public static async Task<WebAPIPostModel> GetPostBySlugAsync(string slug)
        {
            using var httpClient = new HttpClient(handler, false);
            using var request = new HttpRequestMessage(HttpMethod.Get, $"https://cms.smitegame.com/wp-json/smite-api/get-post/1?slug={slug}");
            var response = await httpClient.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<WebAPIPostModel>(json);
        }
    }
}
