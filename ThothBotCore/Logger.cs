using System;
using ThothBotCore.Utilities;

namespace ThothBotCore
{
    public class Logger : ILogger
    {
        public void Log(string severity, string message)
        {
            Text.WriteLine($"{DateTime.UtcNow:[HH:mm:ss]}[{severity}] {message}");
        }
    }
}