﻿using Discord;
using System.Threading.Tasks;

namespace ThothBotCore.Discord
{
    public class DiscordLogger
    {
        readonly ILogger _logger;

        public DiscordLogger(ILogger logger)
        {
            _logger = logger;
        }

        public Task Log(LogMessage logMsg)
        {
            _logger.Log(logMsg.Severity.ToString(), logMsg.Message);
            return Task.CompletedTask;
        }
    }
}
