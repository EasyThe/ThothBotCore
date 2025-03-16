using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Models;
using ThothBotCore.Models.SMITE2;
using ThothBotCore.Utilities;

namespace ThothBotCore.Connections
{
    public static class APIInteractions
    {
        private static readonly HttpClientHandler handler = new();
        private static readonly HttpClient httpClient = new(handler, false);

        // SMITE 2
        public static async Task<List<Smite2NewsModel>> FetchSmite2NewsAsync()
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://webcms.hirezstudios.com/smite2/api/posts/?lng=en-US&populate=*");
            var response = await httpClient.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Smite2NewsModel>>(json);
        }
        public static async Task<Smite2GodsModel> FetchSmite2GodsAsync(string slug = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"https://webcms.hirezstudios.com/smite2/api/gods/?lng=en-US&pagination[page]=1&pagination[pageSize]=100&populate[0]=Ability&populate[1]=Ability.YouTubeLink&populate[2]=Ability.Buffs&populate[3]=Ability.Icon&populate[4]=Ability.Buffs.Icon&populate[5]=difficulty&populate[6]=HeaderImage&populate[7]=pantheon&populate[8]=pantheon.Image&populate[9]=pantheon.localizations&populate[10]=Portrait&populate[11]=roles&populate[12]=roles.gods&populate[13]=roles.gods.pantheon&populate[14]=roles.gods.pantheon.Image&populate[15]=roles.gods.Portrait&populate[16]=roles.Image&populate[17]=roles.localizations&populate[18]=Skin&populate[19]=Skin.Image&populate[20]=type&populate[21]=CommunityGuide&populate[22]=CommunityGuide.previewThumbnail" +
                $"{(slug != null ? $"/{slug}" : "")}");
            var response = await httpClient.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Smite2GodsModel>(json);
        }

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
        public static async Task<SPLStats> GetEsportsStats()
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"https://esports.hirezstudios.com/esportsAPI/smite/eventoverview/{Constants.BotSettings.s[4]}");
            var response = await httpClient.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<SPLStats>(json);
            }
            return new SPLStats();
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
            using var request = new HttpRequestMessage(HttpMethod.Get, $"https://www.smiteproleague.com/{appFile}");
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
            return [];
        }
        // SMITE 2 Trello
        public static async Task<List<TrelloModel>> GetSMITE2TrelloCards()
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.trello.com/1/boards/mrW6CEFO/cards?key={Credentials.botConfig.trelloKey}&token={Credentials.botConfig.trelloToken}");
            var response = await httpClient.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<List<TrelloModel>>(json);
            }
            return [];
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
            catch (Exception ex)
            {
                await Reporter.SendErrorAsync($"===\nGetStatusSummary Call Error: [APIInteractions.cs]\n" + ex.Message + ex.InnerException + "\n===");
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
                await Reporter.SendErrorAsync($"GetSCCCalendarEvents error, status code: {response.StatusCode}");
                return new GoogleCalendarModel();
            }
            catch (Exception ex)
            {
                await Reporter.SendErrorAsync($"===\nGetSCCCalendarEvents Call Error: [APIInteractions.cs]\n" + ex.Message + ex.InnerException + "\n===");
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

                await Reporter.SendErrorAsync($"GetDominantColorFromCloudVisionAsync error, status code: {response.StatusCode}\n```\n{json}```");
                return new GoogleVisionAPIResponseModel();
            }
            catch (Exception ex)
            {
                await Reporter.SendErrorAsync($"===\nGetDominantColorFromCloudVisionAsync Call Error: [APIInteractions.cs]\n" + ex.Message + ex.InnerException + "\n===");
                return new GoogleVisionAPIResponseModel();
            }
        }
    }
}
