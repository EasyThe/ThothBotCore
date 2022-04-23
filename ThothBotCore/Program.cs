using ThothBotCore.Discord;
using System;
using System.Threading.Tasks;
using Sentry;
using ThothBotCore.Discord.Entities;
using OpenTelemetry.Metrics;
using OpenTelemetry;

namespace ThothBotCore
{
    class Program
    {
        private static void Main()
        {
            using (SentrySdk.Init(Credentials.botConfig.Sentry))
            {
                MainAsync().Wait();
            }
        }
        private static async Task MainAsync()
        {
            using MeterProvider meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddMeter("ThothBotMetrics")
                .AddPrometheusExporter(opt =>
                {
                    opt.HttpListenerPrefixes = new string[] { $"http://localhost:9284/" };
                    opt.ScrapeEndpointPath = "/metrics";
                    opt.StartHttpListener = true;
                })
                .Build();

            var connection = Unity.Resolve<Connection>();
            await connection.ConnectAsync();

            Console.ReadKey();
        }
    }
}
