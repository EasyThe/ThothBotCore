using Discord;
using ThothBotCore.Discord.Entities;

namespace ThothBotCore.Utilities
{
    public class Constants
    {
        public static string JoinedMessage { get; set; } = ":wave:**Hi. Thanks for adding me!**\n" +
                $":small_orange_diamond:My prefix is `{Credentials.botConfig.prefix}`\n" +
                $":small_orange_diamond:You can set a custom prefix for your server with {Credentials.botConfig.prefix}prefix `your-prefix-here`\n" +
                $":small_orange_diamond:You can check my commands by using `{Credentials.botConfig.prefix}help`\n" +
                $":small_orange_diamond:Please make sure I have **Send Messages**, **Read Messages**, **Embed Links** and **Use External Emojis** in the channels you would like me to react to your commands.";
        public static string FailedToSendJoinedMessage { get; set; } = "Couldn't send JoinedMessage to the Guild.";


        public static Color DefaultBlueColor { get; set; } = new Color(85, 172, 238);
    }
}
