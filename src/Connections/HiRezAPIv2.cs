using Newtonsoft.Json;
using Sentry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Discord;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Models;
using static ThothBotCore.Models.Gods;

namespace ThothBotCore.Connections
{
    public class HiRezAPIv2
    {
        private string DevId = Credentials.botConfig.devId;
        private string APIKey = Credentials.botConfig.authKey;
        private HttpClient Client;

        private SessionResult sessionResult = new();
        private readonly string BaseURL = "https://api.smitegame.com/smiteapi.svc";
        private MD5 Md5 = MD5.Create();

        public HiRezAPIv2()
        {
            HttpClientHandler httpClientHandler = new();
            Client = new HttpClient(httpClientHandler)
            {
                BaseAddress = new Uri(BaseURL),
                Timeout = TimeSpan.FromSeconds(10)
            };

            string json = File.ReadAllText("Config/hirezapi.json");
            sessionResult = JsonConvert.DeserializeObject<SessionResult>(json);
            var nz = TestSession(sessionResult).Result;
            if (nz != null && nz.Contains("This was a successful"))
            {
                Connection.Logger.Log("HiRezAPI", "Connected to the Hi-Rez API");
            }
        }

        private string GetMD5Hash(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            bytes = Md5.ComputeHash(bytes);
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2").ToLower());
            }
            return sb.ToString();
        }
        private string GetSignature(string endpoint) => GetMD5Hash(DevId + endpoint + APIKey + DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        private async Task CreateSessionAsync()
        {
            string json = await SendRequestAsync("createsession");
            if (json == null)
            {
                return;
            }

            HiRezSession session = JsonConvert.DeserializeObject<HiRezSession>(json);

            SaveSession(session.session_id, session.timestamp);
            Console.WriteLine($"New HiRezAPI Session has been created! [{session.session_id}][{session.timestamp}]");
        }
        private void SaveSession(string sessionID, string timestamp)
        {
            sessionResult.sessionID = sessionID;
            sessionResult.sessionTime = timestamp;

            string json = JsonConvert.SerializeObject(sessionResult, Formatting.Indented);
            File.WriteAllText("Config/hirezapi.json", json);
        }
        private async Task<string> TestSession(SessionResult session)
        {
            //                      /testsessionjson/{DevId}/{Signature}/{Session}/{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}
            string response = await SendRequestAsync("testsession");
            if (response != null && !response.Contains("This was a successful"))
            {
                await CreateSessionAsync();
            }
            return response;
        }
        public async Task<string> PingAsync()
        {
            string json = await SendRequestAsync("ping");
            return json ?? "";
        }
        public async Task<List<Gods.God>> GetGodsAsync()
        {
            string json = await SendRequestAsync("getgods", "1");
            return json == null ? new List<Gods.God>() : JsonConvert.DeserializeObject<List<Gods.God>>(json);
        }
        public async Task<object> GetGodAltAbilitiesAsync()
        {
            string json = await SendRequestAsync("getgodaltabilities");
            return json == null ? "" : JsonConvert.DeserializeObject<dynamic>(json);
        }
        public async Task<List<GodSkinModel>> GetGodSkinsAsync(int godId = -1)
        {
            string json = await SendRequestAsync("getgodskins", $"{godId}/1");
            return json == null ? new List<GodSkinModel>() : JsonConvert.DeserializeObject<List<GodSkinModel>>(json);
        }
        public async Task<List<RecommendedItem>> GetGodRecommendedItemsAsync(int godId = -1)
        {
            string json = await SendRequestAsync("getgodrecommendeditems", $"{godId}/1");
            return json == null ? new List<RecommendedItem>() : JsonConvert.DeserializeObject<List<RecommendedItem>>(json);
        }
        public async Task<List<GetItems.Item>> GetItemsAsync()
        {
            string json = await SendRequestAsync("getitems", "1");
            return json == null ? new List<GetItems.Item>() : JsonConvert.DeserializeObject<List<GetItems.Item>>(json);
        }
        public async Task<PatchInfoModel> GetPatchInfoAsync()
        {
            string json = await SendRequestAsync("getpatchinfo");
            return json == null ? new PatchInfoModel() : JsonConvert.DeserializeObject<PatchInfoModel>(json);
        }
        public async Task<List<DataUsed>> GetDataUsedAsync()
        {
            string json = await SendRequestAsync("getdataused");
            return json == null ? new List<DataUsed>() : JsonConvert.DeserializeObject<List<DataUsed>>(json);
        }
        public async Task<List<Player.PlayerStats>> GetPlayerAsync(string player)
        {
            string json = await SendRequestAsync("getplayer", player);
            return json == null ? new List<Player.PlayerStats>() : JsonConvert.DeserializeObject<List<Player.PlayerStats>>(json);
        }
        /// <summary>
        /// Not available for SMITE API
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public async Task<List<Player.PlayerStats>> GetPlayerBatchAsync(string[] player)
        {
            //                      /getplayerbatchjson/{DevId}/{Signature}/{Session}/{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}/{player}/{PortalId}
            string url = $"{BaseURL}/getplayerbatchjson/{DevId}/{GetSignature("getplayerbatch")}/{sessionResult.sessionID}/{DateTime.UtcNow:yyyyMMddHHmmss}/{String.Join(",", player)}";

            string json = await SendRequestAsync(url);
            if (json == null)
            {
                return new List<Player.PlayerStats>();
            }
            return JsonConvert.DeserializeObject<List<Player.PlayerStats>>(json);
        }
        public async Task<PlayerAchievements> GetPlayerAchievementsAsync(string player)
        {
            string json = await SendRequestAsync("getplayerachievements", player);
            return json == null ? new PlayerAchievements() : JsonConvert.DeserializeObject<PlayerAchievements>(json);
        }
        public async Task<List<Player.PlayerStatus>> GetPlayerStatusAsync(string player)
        {
            string json = await SendRequestAsync("getplayerstatus", player);
            return json == null ? new List<Player.PlayerStatus>() : JsonConvert.DeserializeObject<List<Player.PlayerStatus>>(json);
        }
        public async Task<List<GodRanks>> GetGodRanksAsync(string player)
        {
            string json = await SendRequestAsync("getgodranks", player);
            return json == null ? new List<GodRanks>() : JsonConvert.DeserializeObject<List<GodRanks>>(json);
        }
        public async Task<List<MatchDetails.MatchDetailsPlayer>> GetMatchDetailsAsync(string matchId)
        {
            string json = await SendRequestAsync("getmatchdetails", matchId);
            return json == null ? new List<MatchDetails.MatchDetailsPlayer>() : JsonConvert.DeserializeObject<List<MatchDetails.MatchDetailsPlayer>>(json);
        }
        public async Task<List<MatchHistoryModel>> GetMatchHistoryAsync(string player)
        {
            string json = await SendRequestAsync("getmatchhistory", player);
            return json == null ? new List<MatchHistoryModel>() : JsonConvert.DeserializeObject<List<MatchHistoryModel>>(json);
        }
        public async Task<List<MatchPlayerDetails.PlayerMatchDetails>> GetMatchPlayerDetailsAsync(string player)
        {
            string json = await SendRequestAsync("getmatchplayerdetails", player);
            return json == null ? new List<MatchPlayerDetails.PlayerMatchDetails>() : JsonConvert.DeserializeObject<List<MatchPlayerDetails.PlayerMatchDetails>>(json);
        }
        public async Task<List<Motd>> GetMOTDAsync()
        {
            string json = await SendRequestAsync("getmotd");
            return json == null ? new List<Motd>() : JsonConvert.DeserializeObject<List<Motd>>(json);
        }
        public async Task<List<HiRezServerStatus>> GetHiRezServerStatusAsync()
        {
            string json = await SendRequestAsync("gethirezserverstatus");
            return json == null ? new List<HiRezServerStatus>() : JsonConvert.DeserializeObject<List<HiRezServerStatus>>(json);
        }
        public async Task<List<SearchPlayers>> SearchPlayersAsync(string player)
        {
            string json = await SendRequestAsync("searchplayers", player);
            if (json == null)
            {
                var ret = new List<SearchPlayers>
                {
                    new() { ret_msg = "apidown" }
                };
                return ret;
            }
            return JsonConvert.DeserializeObject<List<SearchPlayers>>(json);
        }
        public async Task<List<QueueStats>> GetQueueStatsAsync(string playerid, int queueid)
        {
            string json = await SendRequestAsync("getqueuestats", $"{playerid}/{queueid}");
            if (json == null)
            {
                var ret = new List<QueueStats>
                {
                    new() { ret_msg = "apidown" }
                };
                return ret;
            }
            return JsonConvert.DeserializeObject<List<QueueStats>>(json);
        }
        public async Task<List<QueueStats>> GetQueueStatsBatchAsync(string playerid, string queueids)
        {
            string json = await SendRequestAsync("getqueuestatsbatch", $"{playerid}/{queueids}");
            if (json == null)
            {
                var ret = new List<QueueStats>
                {
                    new() { ret_msg = "apidown" }
                };
                return ret;
            }
            return JsonConvert.DeserializeObject<List<QueueStats>>(json);
        }
        public async Task<List<TeamDetailsModel>> GetTeamDetailsAsync(string clanId)
        {
            string json = await SendRequestAsync("getteamdetails", clanId);
            if (json == null)
            {
                var ret = new List<TeamDetailsModel>
                {
                    new() { ret_msg = "apidown" }
                };
                return ret;
            }
            return JsonConvert.DeserializeObject<List<TeamDetailsModel>>(json);
        }
        public async Task<List<TeamPlayersModel.TeamPlayer>> GetTeamPlayersAsync(string clanId)
        {
            string json = await SendRequestAsync("getteamplayers", clanId);
            if (json == null)
            {
                var ret = new List<TeamPlayersModel.TeamPlayer>
                {
                    new() { ret_msg = "apidown" }
                };
                return ret;
            }
            return JsonConvert.DeserializeObject<List<TeamPlayersModel.TeamPlayer>>(json);
        }

        private async Task<string> SendRequestAsync(string endpoint, string value = "")
        {
            try
            {
                var url = GenerateURL(endpoint, value);
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                var response = await Client.SendAsync(request);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    SentrySdk.CaptureMessage($"[HiRezAPI] {response.StatusCode} on URL: {url}\n{await response.Content.ReadAsStringAsync()}");
                    // Consider logging those non-ok status codes.
                    return null;
                }
                //Console.WriteLine(url);
                var json = await response.Content.ReadAsStringAsync();
                if (json.ToLowerInvariant().Contains("<html>"))
                {
                    await Connection.Logger.Log("X", $"[HiRezAPI] Status Code: {response.StatusCode} - URL: {url}");
                    return null;
                }
                else if (json.Contains("Invalid session id"))
                {
                    await CreateSessionAsync();
                    
                    return await SendRequestAsync(endpoint, value);
                }
                else if (json.Contains("Exception - Timestamp"))
                {
                    Console.WriteLine(url);
                    Console.WriteLine(DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
                    Console.WriteLine(json);
                    return null;
                }
                else
                {
                    // Everything went well
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendRequestAsync - {ex.Message}");
                return null;
            }
        }
        private string GenerateURL(string endpoint, string value)
        {
            if (value != "" && endpoint != "ping")
            {
                return $"{BaseURL}/{endpoint}json/{DevId}/{GetSignature(endpoint)}/{sessionResult.sessionID}/{DateTime.UtcNow:yyyyMMddHHmmss}/{value}";
            }
            else if (endpoint == "ping")
            {
                return $"{BaseURL}/pingjson";
            }
            else if (endpoint == "createsession")
            {
                return $"{BaseURL}/{endpoint}json/{DevId}/{GetSignature(endpoint)}/{DateTime.UtcNow:yyyyMMddHHmmss}";
            }
            return $"{BaseURL}/{endpoint}json/{DevId}/{GetSignature(endpoint)}/{sessionResult.sessionID}/{DateTime.UtcNow:yyyyMMddHHmmss}";
        }
    }
}
