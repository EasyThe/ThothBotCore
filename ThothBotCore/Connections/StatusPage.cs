using System.Net.Http;
using System.Threading.Tasks;
using ThothBotCore.Utilities;

namespace ThothBotCore.Connections
{
    public static class StatusPage
    {
        public static async Task<string> GetStatusSummary()
        {
            try
            {
                var handler = new HttpClientHandler();
                using (var httpClient = new HttpClient(handler, false))
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, "http://stk4xr7r1y0r.statuspage.io/api/v2/summary.json"))
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
    }
}
