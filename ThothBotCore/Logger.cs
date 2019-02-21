using System;

namespace ThothBotCore
{
    public class Logger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine($"{DateTime.UtcNow.ToString("[HH:mm, d.MM.yyyy]")} {message}");
        }
    }
}