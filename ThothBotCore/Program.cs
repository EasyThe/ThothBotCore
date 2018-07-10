using System;
using ThothBotCore.Discord;
using ThothBotCore.Discord.Entities;

namespace ThothBotCore
{
    internal class Program
    {
        private static void Main()
        {
            Unity.RegisterTypes();
            Console.WriteLine("Hello, Discord!");

            var thothBotConfig = new ThothBotConfig
            {
                Token = "ABC",
                SocketConfig = SocketConfig.GetDefault()
            };

            var connection = Unity.Resolve<Connection>();

            Console.ReadKey();
        }
    }
}