using System.Net.Http;
using System.Threading.Tasks;

namespace ThothBotCore.Connections
{
    public class StatusPage
    {
        internal string statusSummary = "";

        public async Task GetStatusSummary()
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
    }
}
