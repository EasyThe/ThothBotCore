using ThothBotCore.Discord;
using System;
using System.Threading.Tasks;
using Sentry;
using ThothBotCore.Discord.Entities;

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
            var connection = Unity.Resolve<Connection>();
            await connection.ConnectAsync();

            Console.ReadKey();
        }
    }
}
