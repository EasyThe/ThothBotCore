using System;
using ThothBotCore.Utilities;

namespace ThothBotCore
{
    public class Logger : ILogger
    {
        public void Log(string severity, string message)
        {
            // Remove this when eventually D.NET gets finally rewritten ffs
            if (message.Contains("Unknown Dispatch"))
            {
                return;
            }
            Text.WriteLine($"{DateTime.UtcNow:[HH:mm:ss]}[{severity}] {message}");
        }
    }
}