using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Connections.Models;
using ThothBotCore.Discord;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Utilities;

namespace ThothBotCore.Connections
{
    public class HiRezAPIv2
    {
        readonly string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        private SessionResult sessionResult = new SessionResult();
        private readonly string PCAPIurl = "http://api.smitegame.com/smiteapi.svc/";

        private static async Task<string> GetMD5Hash(string input)
        {
            var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            var bytes = Encoding.UTF8.GetBytes(input);
            bytes = md5.ComputeHash(bytes);
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2").ToLower());
            }
            return sb.ToString();
        }

        private async Task CreateSessionAsync()
        {
            string signature = await GetMD5Hash(Credentials.botConfig.devId + "createsession" + Credentials.botConfig.authKey + timestamp);
            string result;

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PCAPIurl}createsessionjson/{Credentials.botConfig.devId}/{signature}/{timestamp}"))
                {
                    var response = await httpClient.SendAsync(request);
                    result = await response.Content.ReadAsStringAsync();
                }
            }
            HiRezSession session = JsonConvert.DeserializeObject<HiRezSession>(result);

            SaveSessionAsync(session.session_id, session.timestamp);
        }
        private async void SaveSessionAsync(string sessionID, string timestamp)
        {
            sessionResult.sessionID = sessionID;
            sessionResult.sessionTime = timestamp;

            string json = JsonConvert.SerializeObject(sessionResult, Formatting.Indented);
            await File.WriteAllTextAsync("Config/hirezapi.json", json);
        }
        private async Task CheckSessionAsync()
        {
            if (!File.Exists("Config/hirezapi.json"))
            {
                await CreateSessionAsync();
            }
            string json = await File.ReadAllTextAsync("Config/hirezapi.json");
            sessionResult = JsonConvert.DeserializeObject<SessionResult>(json);

            DateTime parsedSessionTime = DateTime.Parse(sessionResult.sessionTime, CultureInfo.InvariantCulture);

            if ((DateTime.UtcNow - parsedSessionTime).TotalMinutes >= 15)
            {
                await CreateSessionAsync();
            }
        }
        private async Task<string> TestAndCallAsync(string endpoint, string value)
        {
            await CheckSessionAsync();

            string signature = await GetMD5Hash(Credentials.botConfig.devId + endpoint + Credentials.botConfig.authKey + timestamp);

            // Remove / or \
            if (value.EndsWith('/'))
            {
                value = value.Replace('/', ' ');
            }
            else if (value.EndsWith('\\'))
            {
                value = value.Replace('\\', ' ');
            }

            var handler = new HttpClientHandler();
            using var httpClient = new HttpClient(handler, false);
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{PCAPIurl}{endpoint}json/" +
                $"{Credentials.botConfig.devId}/" +
                $"{signature}/" +
                $"{sessionResult.sessionID}/" +
                $"{timestamp}/" +
                $"{value}");
            var response = await httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            if (json.ToLowerInvariant().Contains("html"))
            {
                var embed = await EmbedHandler.BuildDescriptionEmbedAsync(
                    $"**== HiRezAPIv2 Reporting... ==**\nEndpoint: `{endpoint}` Value: `{value}`\n```html\n{json}```", 254);
                await Reporter.SendEmbedError(embed: embed.ToEmbedBuilder());
                return null;
            }
            else
            {
                return await response.Content.ReadAsStringAsync();
            }
        }
        public async Task<List<Player.PlayerStats>> GetPlayerAsync(string value)
        {
            string json = await TestAndCallAsync("getplayer", value);
            return JsonConvert.DeserializeObject<List<Player.PlayerStats>>(json);
        }
    }
}
