﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Connections.Models;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Models;

namespace ThothBotCore.Connections
{
    public class HiRezAPI
    {
        private readonly string devID = Credentials.botConfig.devId;
        private readonly string authKey = Credentials.botConfig.authKey;
        private const int language = 1;
        readonly string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        private SessionResult sessionResult = new SessionResult();
        private readonly string PCAPIurl = "http://api.smitegame.com/smiteapi.svc/";
        private readonly string PaladinsAPIurl = "http://api.paladins.com/paladinsapi.svc/";

        private static async Task<string> GetMD5Hash(string input)
        {
            var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            bytes = md5.ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2").ToLower());
            }
            return sb.ToString();
        }

        internal string pingAPI;
        internal string dataUsed;
        internal string testing;

        private async Task CreateSession()
        {
            string signature = await GetMD5Hash(Credentials.botConfig.devId + "createsession" + Credentials.botConfig.authKey + timestamp);
            string result;

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PCAPIurl}createsessionjson/{devID}/{signature}/{timestamp}"))
                {
                    var response = await httpClient.SendAsync(request);
                    result = await response.Content.ReadAsStringAsync();
                }
            }
            HiRezSession session = JsonConvert.DeserializeObject<HiRezSession>(result);

            await SaveSession(session.session_id, session.timestamp);
        }

        private async Task SaveSession(string sessionID, string timestamp)
        {
            sessionResult.sessionID = sessionID;
            sessionResult.sessionTime = timestamp;

            string json = JsonConvert.SerializeObject(sessionResult, Formatting.Indented);
            await File.WriteAllTextAsync("Config/hirezapi.json", json);
        }

        private async Task CheckSession()
        {
            if (!File.Exists("Config/hirezapi.json"))
            {
                await CreateSession();
            }
            string json = await File.ReadAllTextAsync("Config/hirezapi.json");
            sessionResult = JsonConvert.DeserializeObject<SessionResult>(json);

            DateTime parsedSessionTime = DateTime.Parse(sessionResult.sessionTime, CultureInfo.InvariantCulture);

            if ((DateTime.UtcNow - parsedSessionTime).TotalMinutes >= 15)
            {
                await CreateSession();
            }
        }

        public async Task<string> GetPlayer(string username)
        {
            await CheckSession();

            string signature = await GetMD5Hash(devID + "getplayer" + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PCAPIurl}getplayerjson/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}/{username}"))
                {
                    var response = await httpClient.SendAsync(request);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task<string> GetPlayerAchievements(int id)
        {
            await CheckSession();

            string signature = await GetMD5Hash(devID + "getplayerachievements" + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PCAPIurl}getplayerachievementsjson/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}/{id}"))
                {
                    var response = await httpClient.SendAsync(request);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task<string> APITestMethod(string _endpoint, string value) // Testing Method
        {
            await CheckSession();

            string signature = await GetMD5Hash(devID + _endpoint.ToLowerInvariant() + authKey + timestamp);
            
            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PCAPIurl}{_endpoint}json/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}/{value}"))
                {
                    var response = await httpClient.SendAsync(request);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task<string> GetPlayerIdByName(string username)
        {
            await CheckSession();

            string signature = await GetMD5Hash(devID + "getplayeridbyname" + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PCAPIurl}getplayeridbynamejson/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}/{username}"))
                {
                    var response = await httpClient.SendAsync(request);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task<string> GetGodRanks(int id)
        {
            await CheckSession();

            string signature = await GetMD5Hash(devID + "getgodranks" + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PCAPIurl}getgodranksjson/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}/{id}"))
                {
                    var response = await httpClient.SendAsync(request);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task<string> GetQueueStats(int id, int queue)
        {
            await CheckSession();

            string signature = await GetMD5Hash(devID + "getqueuestats" + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PCAPIurl}getqueuestatsjson/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}/{id}/{queue}"))
                {
                    var response = await httpClient.SendAsync(request);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task<List<SearchPlayers>> SearchPlayer(string username)
        {
            await CheckSession();

            string signature = await GetMD5Hash(devID + "searchplayers" + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PCAPIurl}searchplayersjson/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}/{username}"))
                {
                    var response = await httpClient.SendAsync(request);
                    string json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<SearchPlayers>>(json);
                }
            }
        }

        public async Task<string> GetTeamDetails(int id)
        {
            await CheckSession();

            string signature = await GetMD5Hash(devID + "getteamdetails" + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PCAPIurl}getteamdetailsjson/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}/{id}"))
                {
                    var response = await httpClient.SendAsync(request);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task<string> GetPlayerStatus(int playerID)
        {
            await CheckSession();

            string signature = await GetMD5Hash(devID + "getplayerstatus" + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PCAPIurl}getplayerstatusjson/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}/{playerID}"))
                {
                    var response = await httpClient.SendAsync(request);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task<string> GetItems()
        {
            await CheckSession();

            string signature = await GetMD5Hash(devID + "getitems" + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PCAPIurl}getitemsjson/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}/{language}"))
                {
                    var response = await httpClient.SendAsync(request);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task<string> GetMOTD()
        {
            await CheckSession();

            string signature = await GetMD5Hash(devID + "getmotd" + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PCAPIurl}getmotdjson/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}"))
                {
                    var response = await httpClient.SendAsync(request);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }
        public async Task<List<MatchHistoryModel>> GetMatchHistory(int playerID)
        {
            await CheckSession();

            string signature = await GetMD5Hash(devID + "getmatchhistory" + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PCAPIurl}getmatchhistoryjson/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}/{playerID}"))
                {
                    var response = await httpClient.SendAsync(request);
                    string json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<MatchHistoryModel>>(json);
                }
            }
        }
        public async Task<string> GetMatchDetails(int matchID)
        {
            await CheckSession();

            string signature = await GetMD5Hash(devID + "getmatchdetails" + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PCAPIurl}getmatchdetailsjson/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}/{matchID}"))
                {
                    var response = await httpClient.SendAsync(request);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task<string> GetMatchPlayerDetails(int matchID)
        {
            await CheckSession();

            string signature = await GetMD5Hash(devID + "getmatchplayerdetails" + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PCAPIurl}getmatchplayerdetailsjson/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}/{matchID}"))
                {
                    var response = await httpClient.SendAsync(request);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task<string> GetGods()
        {
            await CheckSession();

            string signature = await GetMD5Hash(devID + "getgods" + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PCAPIurl}getgodsjson/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}/1"))
                {
                    var response = await httpClient.SendAsync(request);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task EsportsProLeagueDetails()
        {
            await CheckSession();

            string signature = await GetMD5Hash(devID + "getesportsproleaguedetails" + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PCAPIurl}getesportsproleaguedetailsjson/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}"))
                {
                    var response = await httpClient.SendAsync(request);
                    testing = await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task PingAPI()
        {
            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, PCAPIurl + "pingjson"))
                {
                    var response = await httpClient.SendAsync(request);
                    pingAPI = await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task DataUsed()
        {
            await CheckSession();

            string signature = await GetMD5Hash(devID + "getdataused" + authKey + timestamp);

            var dataUsedHandler = new HttpClientHandler();
            using (var httpClient = new HttpClient(dataUsedHandler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, PCAPIurl + "getdatausedjson/" + devID + "/" + signature + "/" + sessionResult.sessionID + "/" + timestamp))
                {
                    var response = await httpClient.SendAsync(request);
                    dataUsed = await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task<string> GetPatchInfo()
        {
            await CheckSession();

            string signature = await GetMD5Hash(devID + "getpatchinfo" + authKey + timestamp);

            var dataUsedHandler = new HttpClientHandler();
            using (var httpClient = new HttpClient(dataUsedHandler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, PCAPIurl + "getpatchinfojson/" + devID + "/" + signature + "/" + sessionResult.sessionID + "/" + timestamp))
                {
                    var response = await httpClient.SendAsync(request);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        // Paladins

        public async Task<string> PaladinsAPITestMethod(string _endpoint, string value) // Testing Method
        {
            await CheckSession();

            string signature = await GetMD5Hash(devID + _endpoint.ToLowerInvariant() + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PaladinsAPIurl}{_endpoint}json/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}/{value}"))
                {
                    var response = await httpClient.SendAsync(request);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task<List<SearchPlayers>> SearchPlayersPaladins(string username)
        {
            await CheckSession();

            string signature = await GetMD5Hash(devID + "searchplayers" + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PaladinsAPIurl}searchplayersjson/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}/{username}"))
                {
                    var response = await httpClient.SendAsync(request);
                    string json = await response.Content.ReadAsStringAsync();
                    if (json.ToLowerInvariant().Contains("not found"))
                    {
                        await File.WriteAllTextAsync($"error4ence{DateTime.Now.ToString("dd-MM-yyyy")}.html", json);
                    }
                    return JsonConvert.DeserializeObject<List<SearchPlayers>>(json);
                }
            }
        }

        public async Task<string> GetPlayerPaladins(string username)
        {
            await CheckSession();

            string signature = await GetMD5Hash(devID + "getplayer" + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PaladinsAPIurl}getplayerjson/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}/{username}"))
                {
                    var response = await httpClient.SendAsync(request);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task<string> GetGodRanksPaladins(int id)
        {
            await CheckSession();

            string signature = await GetMD5Hash(devID + "getgodranks" + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PaladinsAPIurl}getgodranksjson/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}/{id}"))
                {
                    var response = await httpClient.SendAsync(request);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task<string> GetPlayerStatusPaladins(int playerID)
        {
            await CheckSession();

            string signature = await GetMD5Hash(devID + "getplayerstatus" + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PaladinsAPIurl}getplayerstatusjson/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}/{playerID}"))
                {
                    var response = await httpClient.SendAsync(request);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task<string> GetMatchPlayerDetailsPaladins(int matchID)
        {
            await CheckSession();

            string signature = await GetMD5Hash(devID + "getmatchplayerdetails" + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PaladinsAPIurl}getmatchplayerdetailsjson/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}/{matchID}"))
                {
                    var response = await httpClient.SendAsync(request);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        public class PatchInfo
        {
            public object ret_msg { get; set; }
            public string version_string { get; set; }
        }
    }
    public class SessionResult
    {
        public string sessionTime { get; set; }
        public string sessionID { get; set; }
    }
}
