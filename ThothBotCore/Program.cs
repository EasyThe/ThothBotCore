using ThothBotCore.Discord;
using ThothBotCore.Storage;
using System;
using System.Threading.Tasks;

namespace ThothBotCore
{
    internal class Program
    {
        private static async Task Main()
        {
            var storage = Unity.Resolve<IDataStorage>();

            var connection = Unity.Resolve<Connection>();
            await connection.ConnectAsync();

            Console.ReadKey();
        }
    }
}
