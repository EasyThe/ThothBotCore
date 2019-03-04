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
                Directory.CreateDirectory(configFolder);

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
            }
        }
    }

    public struct BotConfig
    {
        public string Token { get; set; }
        public string devId { get; set; }
        public string authKey { get; set; }
        public string prefix { get; set; }
    }
}
