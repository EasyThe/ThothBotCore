using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Connections.Models;
using ThothBotCore.Discord.Entities;

namespace ThothBotCore.Connections
{
    public class HiRezAPI
    {
        private readonly string devID = Credentials.botConfig.devId;
        private readonly string authKey = Credentials.botConfig.authKey;
        readonly string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        private SessionResult sessionResult;
        private readonly string PCAPIurl = "http://api.smitegame.com/smiteapi.svc/";
        private static string GetMD5Hash(string input)
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

        internal string playerResult;
        internal string playerStatus;
        internal string matchDetails;
        internal string matchPlayerDetails;
        internal string pingAPI;
        internal string dataUsed;
        internal string testing;

        private async Task CreateSession()
        {
            string signature = GetMD5Hash(Credentials.botConfig.devId + "createsession" + Credentials.botConfig.authKey + timestamp);
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

            SaveSession(session.session_id, session.timestamp);
        }

        private void SaveSession(string sessionID, string timestamp)
        {
            sessionResult.sessionID = sessionID;
            sessionResult.sessionTime = timestamp;

            string json = JsonConvert.SerializeObject(sessionResult, Formatting.Indented);
            File.WriteAllText("Config/hirezapi.json", json);
        }

        private async Task CheckSession()
        {
            if (!File.Exists("Config/hirezapi.json"))
            {
                await CreateSession();
            }
            string json = File.ReadAllText("Config/hirezapi.json");
            sessionResult = JsonConvert.DeserializeObject<SessionResult>(json);

            DateTime parsedSessionTime = DateTime.Parse(sessionResult.sessionTime, CultureInfo.InvariantCulture);

            if ((DateTime.UtcNow - parsedSessionTime).TotalMinutes >= 15)
            {
                await CreateSession();
            }
        }

        public async Task GetPlayer(string username)
        {
            await CheckSession();

            string signature = GetMD5Hash(devID + "getplayer" + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PCAPIurl}getplayerjson/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}/{username}"))
                {
                    var response = await httpClient.SendAsync(request);
                    playerResult = await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task<string> GetPlayerIdByName(string username)
        {
            await CheckSession();

            string signature = GetMD5Hash(devID + "getplayeridbyname" + authKey + timestamp);

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

            string signature = GetMD5Hash(devID + "getgodranks" + authKey + timestamp);

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

            string signature = GetMD5Hash(devID + "getqueuestats" + authKey + timestamp);

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

        public async Task<string> GetTeamDetails(int id)
        {
            await CheckSession();

            string signature = GetMD5Hash(devID + "getteamdetails" + authKey + timestamp);

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

        public async Task GetPlayerStatus(int playerID)
        {
            await CheckSession();

            string signature = GetMD5Hash(devID + "getplayerstatus" + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PCAPIurl}getplayerstatusjson/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}/{playerID}"))
                {
                    var response = await httpClient.SendAsync(request);
                    playerStatus = await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task GetMatchDetails(int matchID)
        {
            await CheckSession();

            string signature = GetMD5Hash(devID + "getmatchdetails" + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PCAPIurl}getmatchdetailsjson/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}/{matchID}"))
                {
                    var response = await httpClient.SendAsync(request);
                    matchDetails = await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task GetMatchPlayerDetails(int matchID)
        {
            await CheckSession();

            string signature = GetMD5Hash(devID + "getmatchplayerdetails" + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PCAPIurl}getmatchplayerdetailsjson/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}/{matchID}"))
                {
                    var response = await httpClient.SendAsync(request);
                    matchPlayerDetails = await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task<string> GetGods()
        {
            await CheckSession();

            string signature = GetMD5Hash(devID + "getgods" + authKey + timestamp);

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

        public async Task GetPatchInfo()
        {
            await CheckSession();

            string signature = GetMD5Hash(devID + "getpatchinfo" + authKey + timestamp);

            var handler = new HttpClientHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, $"{PCAPIurl}getpatchinfojson/{devID}/{signature}/{sessionResult.sessionID}/{timestamp}"))
                {
                    var response = await httpClient.SendAsync(request);
                    testing = await response.Content.ReadAsStringAsync();
                }
            }
        }

        public async Task EsportsProLeagueDetails()
        {
            await CheckSession();

            string signature = GetMD5Hash(devID + "getesportsproleaguedetails" + authKey + timestamp);

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

            string signature = GetMD5Hash(devID + "getdataused" + authKey + timestamp);

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

        public class SessionResult
        {
            public string sessionTime;
            public string sessionID;
        }
    }
}
