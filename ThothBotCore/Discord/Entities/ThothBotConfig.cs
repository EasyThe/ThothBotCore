﻿using Discord.WebSocket;

namespace ThothBotCore.Discord.Entities
{
    public class ThothBotConfig
    {
        public string Token { get; set; }

        public DiscordSocketConfig SocketConfig { get; set; }
    }
}
