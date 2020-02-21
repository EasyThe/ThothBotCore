using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ThothBotCore.Utilities;

namespace ThothBotCore.Connections
{
    public static class StatusPage
    {
        internal static string statusSummary = "";

        public static async Task GetStatusSummary()
        {
            try
            {
                var handler = new HttpClientHandler();
                using (var httpClient = new HttpClient(handler, false))
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, "http://stk4xr7r1y0r.statuspage.io/api/v2/summary.json"))
                    {
                        var response = await httpClient.SendAsync(request);
                        statusSummary = await response.Content.ReadAsStringAsync();
                    }
                }
                return;
            }
            catch (System.Exception ex)
            {
                await ErrorTracker.SendError("**GetStatusSummary** Error\n" + ex.Message);
            }
        }
        public static async Task<string> GetDiscordStatusSummary()
        {
            try
            {
                var handler = new HttpClientHandler();
                using (var httpClient = new HttpClient(handler, false))
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, "https://srhpyqt94yxb.statuspage.io/api/v2/summary.json"))
                    {
                        var response = await httpClient.SendAsync(request);
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (System.Exception ex)
            {
                await ErrorTracker.SendError("**GetStatusSummary** Error\n" + ex.Message);
                return "";
            }
        }

        public static async Task<string> CreateStatusWebhook()
        {
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "http://stk4xr7r1y0r.statuspage.io/api/v2/subscribers.json"))
                {
                    var contentList = new List<string>();
                    contentList.Add("subscriber[email]=f3n1xx.org@gmail.com");
                    contentList.Add("subscriber[endpoint]=https://discordapp.com/api/webhooks/561220546755297310/qYZ2JoFo5jB5unCXzDIHklnaWZbgVG4nEtpyCLPWIx-toVoRZ6GM5dYdJfZR8Kaddr-x");
                    request.Content = new StringContent(string.Join("&", contentList), Encoding.UTF8, "application/x-www-form-urlencoded");

                    var response = await httpClient.SendAsync(request);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }
    }
}
