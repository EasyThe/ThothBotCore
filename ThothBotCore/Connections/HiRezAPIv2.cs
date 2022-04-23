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
                Timeout = TimeSpan.FromSeconds(2)
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
            string url = $"{BaseURL}/createsessionjson/{DevId}/{GetSignature("createsession")}/{DateTime.UtcNow:yyyyMMddHHmmss}";

            string json = await SendRequestAsync(url);
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
            string url = $"{BaseURL}/testsessionjson/{DevId}/{GetSignature("testsession")}/{sessionResult.sessionID}/{DateTime.UtcNow:yyyyMMddHHmmss}";

            string response = await SendRequestAsync(url);
            if (response != null && !response.Contains("successful"))
            {
                await CreateSessionAsync();
            }
            return response;
        }
        public async Task<string> PingAsync()
        {
            //                      /pingjson
            string url = $"{BaseURL}/pingjson";

            string json = await SendRequestAsync(url);
            if (json == null)
            {
                return "";
            }
            return json;
        }
        public async Task<List<Gods.God>> GetGodsAsync()
        {
            //                      /getgodsjson/{DevId}/{Signature}/{Session}/{timestamp}/{languageId}
            string url = $"{BaseURL}/getgodsjson/{DevId}/{GetSignature("getgods")}/{sessionResult.sessionID}/{DateTime.UtcNow:yyyyMMddHHmmss}/1";

            string json = await SendRequestAsync(url);
            if (json == null)
            {
                return new List<Gods.God>();
            }
            return JsonConvert.DeserializeObject<List<Gods.God>>(json);
        }
        public async Task<object> GetGodAltAbilitiesAsync()
        {
            //                      /getGodAltAbilitiesjson/{DevId}/{Signature}/{Session}/{timestamp}/
            string url = $"{BaseURL}/getgodaltabilitiesjson/{DevId}/{GetSignature("getgodaltabilities")}/{sessionResult.sessionID}/{DateTime.UtcNow:yyyyMMddHHmmss}";

            string json = await SendRequestAsync(url);
            if (json == null)
            {
                return "";
            }
            return JsonConvert.DeserializeObject<dynamic>(json);
        }
        public async Task<List<GodSkinModel>> GetGodSkinsAsync(int godId = -1)
        {
            //                      /getgodskinsjson/{DevId}/{Signature}/{Session}/{timestamp}/{godId}/{languageCode}
            string url = $"{BaseURL}/getgodskinsjson/{DevId}/{GetSignature("getgodskins")}/{sessionResult.sessionID}/{DateTime.UtcNow:yyyyMMddHHmmss}/{godId}/1";

            string json = await SendRequestAsync(url);
            if (json == null)
            {
                return new List<GodSkinModel>();
            }
            return JsonConvert.DeserializeObject<List<GodSkinModel>>(json);
        }
        public async Task<List<GetItems.Item>> GetItemsAsync()
        {
            //                      /getitemsjson/{DevId}/{Signature}/{Session}/{timestamp}/{languageId}
            string url = $"{BaseURL}/getitemsjson/{DevId}/{GetSignature("getitems")}/{sessionResult.sessionID}/{DateTime.UtcNow:yyyyMMddHHmmss}/1";

            string json = await SendRequestAsync(url);
            if (json == null)
            {
                return new List<GetItems.Item>();
            }
            return JsonConvert.DeserializeObject<List<GetItems.Item>>(json);
        }
        public async Task<PatchInfoModel> GetPatchInfoAsync()
        {
            //                      /getpatchinfojson/{DevId}/{Signature}/{Session}/{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}
            string url = $"{BaseURL}/getpatchinfojson/{DevId}/{GetSignature("getpatchinfo")}/{sessionResult.sessionID}/{DateTime.UtcNow:yyyyMMddHHmmss}";

            string json = await SendRequestAsync(url);
            if (json == null)
            {
                return new PatchInfoModel();
            }
            return JsonConvert.DeserializeObject<PatchInfoModel>(json);
        }
        public async Task<List<DataUsed>> GetDataUsedAsync()
        {
            //                      /getdatausedjson/{DevId}/{Signature}/{Session}/{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}
            string url = $"{BaseURL}/getdatausedjson/{DevId}/{GetSignature("getdataused")}/{sessionResult.sessionID}/{DateTime.UtcNow:yyyyMMddHHmmss}";

            string json = await SendRequestAsync(url);
            if (json == null)
            {
                return new List<DataUsed>();
            }
            return JsonConvert.DeserializeObject<List<DataUsed>>(json);
        }
        public async Task<List<Player.PlayerStats>> GetPlayerAsync(string player)
        {
            //                      /getplayerjson/{DevId}/{Signature}/{Session}/{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}/{player}/{PortalId}
            string url = $"{BaseURL}/getplayerjson/{DevId}/{GetSignature("getplayer")}/{sessionResult.sessionID}/{DateTime.UtcNow:yyyyMMddHHmmss}/{player}";

            string json = await SendRequestAsync(url);
            if (json == null)
            {
                return new List<Player.PlayerStats>();
            }
            return JsonConvert.DeserializeObject<List<Player.PlayerStats>>(json);
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
            //                      /getplayerachievementsjson/{DevId}/{Signature}/{Session}/{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}/{player}
            string url = $"{BaseURL}/getplayerachievementsjson/{DevId}/{GetSignature("getplayerachievements")}/{sessionResult.sessionID}/{DateTime.UtcNow:yyyyMMddHHmmss}/{player}";

            string json = await SendRequestAsync(url);
            if (json == null)
            {
                return new PlayerAchievements();
            }
            return JsonConvert.DeserializeObject<PlayerAchievements>(json);
        }
        public async Task<List<Player.PlayerStatus>> GetPlayerStatusAsync(string player)
        {
            //                      /getplayerstatusjson/{DevId}/{Signature}/{Session}/{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}/{player}
            string url = $"{BaseURL}/getplayerstatusjson/{DevId}/{GetSignature("getplayerstatus")}/{sessionResult.sessionID}/{DateTime.UtcNow:yyyyMMddHHmmss}/{player}";

            string json = await SendRequestAsync(url);
            if (json == null)
            {
                return new List<Player.PlayerStatus>();
            }
            return JsonConvert.DeserializeObject<List<Player.PlayerStatus>>(json);
        }
        public async Task<List<GodRanks>> GetGodRanksAsync(string player)
        {
            //                      /getgodranksjson/{DevId}/{Signature}/{Session}/{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}/{player}
            string url = $"{BaseURL}/getgodranksjson/{DevId}/{GetSignature("getgodranks")}/{sessionResult.sessionID}/{DateTime.UtcNow:yyyyMMddHHmmss}/{player}";

            string json = await SendRequestAsync(url);
            if (json == null)
            {
                return new List<GodRanks>();
            }
            return JsonConvert.DeserializeObject<List<GodRanks>>(json);
        }
        public async Task<List<MatchDetails.MatchDetailsPlayer>> GetMatchDetailsAsync(string matchId)
        {
            //                      /getmatchdetailsjson/{DevId}/{Signature}/{Session}/{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}/{player}
            string url = $"{BaseURL}/getmatchdetailsjson/{DevId}/{GetSignature("getmatchdetails")}/{sessionResult.sessionID}/{DateTime.UtcNow:yyyyMMddHHmmss}/{matchId}";

            string json = await SendRequestAsync(url);
            if (json == null)
            {
                return new List<MatchDetails.MatchDetailsPlayer>();
            }
            return JsonConvert.DeserializeObject<List<MatchDetails.MatchDetailsPlayer>>(json);
        }
        public async Task<List<MatchHistoryModel>> GetMatchHistoryAsync(string player)
        {
            //                      /getmatchhistoryjson/{DevId}/{Signature}/{Session}/{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}/{player}
            string url = $"{BaseURL}/getmatchhistoryjson/{DevId}/{GetSignature("getmatchhistory")}/{sessionResult.sessionID}/{DateTime.UtcNow:yyyyMMddHHmmss}/{player}";

            string json = await SendRequestAsync(url);
            if (json == null)
            {
                return new List<MatchHistoryModel>();
            }
            return JsonConvert.DeserializeObject<List<MatchHistoryModel>>(json);
        }
        public async Task<List<MatchPlayerDetails.PlayerMatchDetails>> GetMatchPlayerDetailsAsync(string player)
        {
            //                      /getmatchplayerdetailsjson/{DevId}/{Signature}/{Session}/{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}/{player}
            string url = $"{BaseURL}/getmatchplayerdetailsjson/{DevId}/{GetSignature("getmatchplayerdetails")}/{sessionResult.sessionID}/{DateTime.UtcNow:yyyyMMddHHmmss}/{player}";

            string json = await SendRequestAsync(url);
            if (json == null)
            {
                return new List<MatchPlayerDetails.PlayerMatchDetails>();
            }
            return JsonConvert.DeserializeObject<List<MatchPlayerDetails.PlayerMatchDetails>>(json);
        }
        public async Task<List<Motd>> GetMOTD()
        {
            //                      /getmotdjson/{DevId}/{Signature}/{Session}/{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}
            string url = $"{BaseURL}/getmotdjson/{DevId}/{GetSignature("getmotd")}/{sessionResult.sessionID}/{DateTime.UtcNow:yyyyMMddHHmmss}";

            string json = await SendRequestAsync(url);
            if (json == null)
            {
                return new List<Motd>();
            }
            return JsonConvert.DeserializeObject<List<Motd>>(json);
        }
        public async Task<List<HiRezServerStatus>> GetHiRezServerStatusAsync()
        {
            //                      /gethirezserverstatusjson/{DevId}/{Signature}/{Session}/{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}
            string url = $"{BaseURL}/gethirezserverstatusjson/{DevId}/{GetSignature("gethirezserverstatus")}/{sessionResult.sessionID}/{DateTime.UtcNow:yyyyMMddHHmmss}";
            
            string json = await SendRequestAsync(url);
            if (json == null)
            {
                return new List<HiRezServerStatus>();
            }
            return JsonConvert.DeserializeObject<List<HiRezServerStatus>>(json);
        }
        public async Task<List<SearchPlayers>> SearchPlayersAsync(string player)
        {
            //                      /searchplayersjson/{DevId}/{Signature}/{Session}/{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}/{player}
            string url = $"{BaseURL}/searchplayersjson/{DevId}/{GetSignature("searchplayers")}/{sessionResult.sessionID}/{DateTime.UtcNow:yyyyMMddHHmmss}/{player}";

            string json = await SendRequestAsync(url);
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
        public async Task<List<QueueStats>> GetQueueStats(string playerid, int queueid)
        {
            //                      /getqueuestatsjson/{DevId}/{Signature}/{Session}/{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}/{playerid}/queueid
            string url = $"{BaseURL}/getqueuestatsjson/{DevId}/{GetSignature("getqueuestats")}/{sessionResult.sessionID}/{DateTime.UtcNow:yyyyMMddHHmmss}/{playerid}/{queueid}";

            string json = await SendRequestAsync(url);
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

        private async Task<string> SendRequestAsync(string url)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                var response = await Client.SendAsync(request);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    SentrySdk.CaptureMessage($"[HiRezAPI] {response.StatusCode} on URL: {url}\n{await response.Content.ReadAsStringAsync()}");
                    // Consider logging those non-ok status codes.
                    return null;
                }
                var json = await response.Content.ReadAsStringAsync();
                if (json.ToLowerInvariant().Contains("<html>"))
                {
                    await Connection.Logger.Log("X", $"[HiRezAPI] Status Code: {response.StatusCode} - URL: {url}");
                    return null;
                }
                else if (json.Contains("Invalid session id"))
                {
                    await CreateSessionAsync();
                    return null;
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
    }
}
