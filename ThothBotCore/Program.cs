using ThothBotCore.Discord;
using System;
using System.Threading.Tasks;
using ThothBotCore.Storage.Implementations;

namespace ThothBotCore
{
    internal class Program
    {
        private static async Task Main()
        {
            var connection = Unity.Resolve<Connection>();
            await connection.ConnectAsync();

            Console.ReadKey();
        }
    }
}
