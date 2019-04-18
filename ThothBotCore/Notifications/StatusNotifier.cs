using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThothBotCore.Connections.Models;
using ThothBotCore.Discord;
using static ThothBotCore.Storage.Database;

namespace ThothBotCore.Notifications
{
    public static class StatusNotifier
    {
        private static SocketTextChannel channel;

        public static Task SendNotifs(ServerStatus serverStatus)
        {
            //deprecated NOT USING THIS...

            List<ServerConfig> notifChannels = GetNotifChannels();

            for (int i = 0; i < notifChannels.Count; i++)
            {
                channel = Connection.Client.GetGuild(notifChannels[i].serverID).GetTextChannel(notifChannels[i].statusChannel);
                if (serverStatus.scheduled_maintenances.Count >= 1 && serverStatus.scheduled_maintenances[0].name.Contains("Smite"))
                {
                    channel.SendMessageAsync("", false, EmbedHandler.StatusMaintenanceEmbed(serverStatus).Build());
                }
                else if (serverStatus.incidents.Count >= 1 && serverStatus.incidents[0].name.Contains("Smite"))
                {
                    channel.SendMessageAsync("", false, EmbedHandler.StatusIncidentEmbed(serverStatus).Build());
                }
            }

            return Task.CompletedTask;
        }

        public static Task SendServerStatus(EmbedBuilder embed)
        {
            List<ServerConfig> notifChannels = GetNotifChannels();

            for (int i = 0; i < notifChannels.Count; i++)
            {
                channel = Connection.Client.GetGuild(notifChannels[i].serverID).GetTextChannel(notifChannels[i].statusChannel);
                channel.SendMessageAsync("", false, embed.Build());

                System.Threading.Thread.Sleep(200);
            }

            return Task.CompletedTask;
        }
    }
}
