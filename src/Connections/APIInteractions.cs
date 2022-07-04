using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Models;
using ThothBotCore.Utilities;

namespace ThothBotCore.Connections
{
    public static class APIInteractions
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
        public static async Task<List<SmiteWebGodAbilityInfoModel>> GetGodAbilityVideoIDsByGodNameAsync(string godName)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"https://cms.smitegame.com/wp-json/wp/v2/gods?search={godName.Replace(" ", "%20")}");
            var response = await httpClient.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<SmiteWebGodAbilityInfoModel>>(json);
        }
        public static async Task<List<SmiteWebGodAbilityInfoModel>> GetGodAbilityVideoIDsByPageAsync(int page)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"https://cms.smitegame.com/wp-json/wp/v2/gods?page={page}");
            var response = await httpClient.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return null;
            }
            return JsonConvert.DeserializeObject<List<SmiteWebGodAbilityInfoModel>>(json);
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
            using var request = new HttpRequestMessage(HttpMethod.Get, $"https://esports.hirezstudios.com/esportsAPI/smite/schedule/{Constants.BotSettings.s[4]}");
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
            using var request = new HttpRequestMessage(HttpMethod.Get, $"https://esports.hirezstudios.com/esportsAPI/smite/standings/{Constants.BotSettings.s[4]}");
            var response = await httpClient.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<List<SPLStandings>>(json);
            }
            return new List<SPLStandings>();
        }

        public static async Task<string> GetEsportsAppFile()
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://www.smiteproleague.com");
            var response = await httpClient.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return json;
            }
            return json;
        }

        public static async Task<string> GetEsportsEventID(string appFile)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"https://www.smiteproleague.com/{appFile}.js");
            var response = await httpClient.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return json;
            }
            return json;
        }

        // Trello
        public static async Task<List<TrelloModel>> GetTrelloCards()
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.trello.com/1/boards/d4fJtBlo/cards?key={Credentials.botConfig.trelloKey}&token={Credentials.botConfig.trelloToken}");
            var response = await httpClient.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<List<TrelloModel>>(json);
            }
            return new List<TrelloModel>();
        }

        // Server status
        public static async Task<string> GetStatusSummary()
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"http://stk4xr7r1y0r.statuspage.io/api/v2/summary.json");
                var response = await httpClient.SendAsync(request);
                string json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return json;
                }
                return "";
            }
            catch (System.Exception ex)
            {
                await Reporter.SendError($"===\nGetStatusSummary Call Error: [APIInteractions.cs]\n" + ex.Message + ex.InnerException + "\n===");
                return "";
            }
        }

        // Google Calendar
        public static async Task<GoogleCalendarModel> GetSCCCalendarEvents()
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, 
                    $"https://www.googleapis.com/calendar/v3/calendars/du6lvi0d4jksj6bvehf0478bes@group.calendar.google.com/events" +
                    $"?key={Credentials.botConfig.GoogleAPIKey}&" +
                    $"timeMin={XmlConvert.ToString(DateTime.UtcNow.AddDays(-5), XmlDateTimeSerializationMode.Utc)}");
                var response = await httpClient.SendAsync(request);
                string json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<GoogleCalendarModel>(json);
                }
                await Reporter.SendError($"GetSCCCalendarEvents error, status code: {response.StatusCode}");
                return new GoogleCalendarModel();
            }
            catch (Exception ex)
            {
                await Reporter.SendError($"===\nGetSCCCalendarEvents Call Error: [APIInteractions.cs]\n" + ex.Message + ex.InnerException + "\n===");
                return new GoogleCalendarModel();
            }
        }


        public static async Task<GoogleVisionAPIResponseModel> GetDominantColorFromCloudVisionAsync(string url)
        {
            try
            {
                using var content = new StringContent("{\"requests\":[{\"features\":[{\"maxResults\":10,\"type\":\"IMAGE_PROPERTIES\"}],\"image\":{\"source\":{\"imageUri\":" +
                    $"\"{url}\"" +
                    "}}}]}");
                var response = await httpClient.PostAsync($"https://vision.googleapis.com/v1/images:annotate?key={Credentials.botConfig.GoogleAPIKey}", content);
                string json = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<GoogleVisionAPIResponseModel>(json);
                }

                await Reporter.SendError($"GetDominantColorFromCloudVisionAsync error, status code: {response.StatusCode}\n```\n{json}```");
                return new GoogleVisionAPIResponseModel();
            }
            catch (Exception ex)
            {
                await Reporter.SendError($"===\nGetDominantColorFromCloudVisionAsync Call Error: [APIInteractions.cs]\n" + ex.Message + ex.InnerException + "\n===");
                return new GoogleVisionAPIResponseModel();
            }
        }
        
    }
}
