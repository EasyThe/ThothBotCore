using Newtonsoft.Json;
using System.IO;

namespace ThothBotCore.Discord.Entities
{
    class Credentials
    {
        private const string configFolder = "Config";
        private const string configFile = "Config.json";

        public static BotConfig botConfig;
        static Credentials()
        {
            if (!Directory.Exists(configFolder))
            {
                Directory.CreateDirectory(configFolder);
            }
            
            if (!File.Exists(configFolder + "/" + configFile))
            {
                botConfig = new BotConfig();
                string json = JsonConvert.SerializeObject(botConfig, Formatting.Indented);
                File.WriteAllText(configFolder + "/" + configFile, json);
            }
            else
            {
                string json = File.ReadAllText(configFolder + "/" + configFile);
                botConfig = JsonConvert.DeserializeObject<BotConfig>(json);
                
                SaveConfig();
            }
        }

        public static void SaveConfig()
        {
            string json = JsonConvert.SerializeObject(botConfig, Formatting.Indented);
            File.WriteAllText(configFolder + "/" + configFile, json);
        }
    }

    public class BotConfig
    {
        public string Token { get; set; } = "TOKEN-HERE";
        public string devId { get; set; } = "HiRezDevID";
        public string authKey { get; set; } = "HiRezAuthKey";
        public string challongeKey { get; set; } = "ChallongeKey";
        public string trelloKey { get; set; } = "TrelloKey";
        public string trelloToken { get; set; } = "TrelloToken";
        public string prefix { get; set; } = "!!";
        public string setGame { get; set; } = "!!help";
        public string botsAPI { get; set; } = "DiscordBotsAPIkey";
        public string bfdAPI { get; set; } = "BotsForDiscordAPIkey";
        public string dblAPI { get; set; } = "DiscordBotListAPIkey";
        public string dbggAPI { get; set; } = "DiscordBotsGGAPIkey";
        public string BotsOnDiscordAPI { get; set; } = "BotsOnDiscordAPIkey";
        public string DiscordServicesAPI { get; set; } = "DiscordServicesAPI";
    }
}
