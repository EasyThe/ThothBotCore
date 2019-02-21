using ThothBotCore.Discord;
using ThothBotCore.Discord.Entities;
using ThothBotCore.Storage;
using System;
using System.Threading.Tasks;

namespace ThothBotCore
{
    internal class Program
    {
        private static async Task Main()
        {
            Unity.RegisterTypes();
            Console.WriteLine("Hello, Discord!");

            var storage = Unity.Resolve<IDataStorage>();

            //var token = "YOUR-TOKEN-HERE";
            //storage.StoreObject(token, "Config/BotToken");

            //Console.WriteLine("Done");

            //Console.ReadKey();
            //return;

            var connection = Unity.Resolve<Connection>();
            await connection.ConnectAsync(new ThothBotConfig
            {
                Token = storage.RestoreObject<string>("Config/BotToken")
            });

            Console.ReadKey();
        }
    }
}
