using System;

namespace ThothBotCore
{
    public class Logger : ILogger
    {
        public void Log(string severity, string message)
        {
            Console.WriteLine($"{DateTime.Now:[HH:mm]}[{severity}] {message}");
        }
    }
}