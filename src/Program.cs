using ThothBotCore.Discord;
using System;
using System.Threading.Tasks;
using Sentry;
using ThothBotCore.Discord.Entities;
using OpenTelemetry.Metrics;
using OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using ThothBotCore.Tasks;

namespace ThothBotCore
{
    class Program
    {
        private static void Main()
        {
            // FORCE config load FIRST
            var config = Credentials.GetConfig();

            // validate Sentry safely
            using (SentrySdk.Init(
                !string.IsNullOrWhiteSpace(config.Sentry) && config.Sentry.StartsWith("http")
                    ? config.Sentry
                    : null))
            {
                MainAsync().Wait();
            }
        }
        private static async Task MainAsync()
        {
            var serviceCollection = new ServiceCollection();
            ServiceCollectionExtensions.ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // resolve services
            var logger = serviceProvider.GetRequiredService<ILogger>();
            var connection = serviceProvider.GetRequiredService<Connection>();
            var smite2NewsTask = serviceProvider.GetRequiredService<Smite2NewsTask>();
            smite2NewsTask.Start();
            var smite2GodsTask = serviceProvider.GetRequiredService<Smite2GodsTask>();
            smite2GodsTask.Start();
            var advertiserTask = serviceProvider.GetRequiredService<AdvertiserTask>();
            advertiserTask.Start();
            var updateNotesTask = serviceProvider.GetRequiredService<Smite2UpdateNotes>();
            updateNotesTask.Start();

            using MeterProvider meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddMeter("ThothBotMetrics")
                .AddPrometheusHttpListener(opt =>
                {
                    opt.ScrapeEndpointPath = "/metrics";
                    opt.UriPrefixes = [$"http://localhost:{Credentials.botConfig.MetricsPort}/"];
                })
                .Build();

            await connection.ConnectAsync();

            Console.ReadKey();
        }

        public static class ServiceCollectionExtensions
        {
            public static void ConfigureServices(IServiceCollection services)
            {
                // Register dependencies
                services.AddSingleton<ILogger, Logger>();
                services.AddSingleton(provider => SocketConfig.GetDefault());
                services.AddSingleton(provider =>
                {
                    var config = provider.GetRequiredService<DiscordSocketConfig>();
                    return new DiscordShardedClient(config);
                });
                services.AddSingleton<DiscordLogger>();
                services.AddSingleton<Connection>();
                services.AddSingleton(provider => new Smite2NewsTask(TimeSpan.FromMinutes(6)));
                services.AddSingleton(provider => new Smite2GodsTask(TimeSpan.FromDays(1)));
                services.AddSingleton(provider => new AdvertiserTask(TimeSpan.FromMinutes(10)));
                services.AddSingleton(provider => new Smite2UpdateNotes(TimeSpan.FromMinutes(8)));
            }
        }
    }
}
