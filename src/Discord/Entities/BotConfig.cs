using Newtonsoft.Json;
using System;
using System.IO;

namespace ThothBotCore.Discord.Entities
{
    class Credentials
    {
        private const string configFolder = "Config";
        private const string configFile = "Config.json";

        public static BotConfig botConfig;

        public static BotConfig GetConfig()
        {
            return botConfig ?? throw new Exception("Config not initialized");
        }

        static Credentials()
        {
            LoadConfig();
        }

        private static void LoadConfig()
        {
            string path = Path.Combine(configFolder, configFile);

            try
            {
                // Ensure folder exists (safe in Docker bind mounts)
                if (!Directory.Exists(configFolder))
                {
                    Directory.CreateDirectory(configFolder);
                }

                // If config does NOT exist → create default
                if (!File.Exists(path))
                {
                    botConfig = new BotConfig();

                    string json = JsonConvert.SerializeObject(botConfig, Formatting.Indented);
                    File.WriteAllText(path, json);
                    return;
                }

                // If config exists → load it
                string existingJson = File.ReadAllText(path);
                botConfig = JsonConvert.DeserializeObject<BotConfig>(existingJson);

                if (botConfig == null)
                {
                    throw new Exception("Config failed to load and is null");
                }
            }
            catch (System.Exception ex)
            {
                // IMPORTANT: prevent container crash loop
                // fallback to defaults instead of crashing app
                botConfig = new BotConfig();

                System.Console.WriteLine("[Config] Failed to load config, using defaults.");
                System.Console.WriteLine(ex.Message);
            }
        }

        public static void SaveConfig()
        {
            try
            {
                string path = Path.Combine(configFolder, configFile);

                if (!Directory.Exists(configFolder))
                {
                    Directory.CreateDirectory(configFolder);
                }

                string json = JsonConvert.SerializeObject(botConfig, Formatting.Indented);
                File.WriteAllText(path, json);
            }
            catch (System.Exception ex)
            {
                // Prevent crash if container volume is read-only or misconfigured
                System.Console.WriteLine("[Config] Failed to save config.");
                System.Console.WriteLine(ex.Message);
            }
        }
    }

    public class BotConfig
    {
        public string Token { get; set; } = "TOKEN-HERE";
        public string devId { get; set; } = "HiRezDevID";
        public string MongoDbURL { get; set; } = "MongoURL";
        public string Sentry { get; set; } = "SentryURL";
        public string authKey { get; set; } = "HiRezAuthKey";
        public string challongeKey { get; set; } = "ChallongeKey";
        public string trelloKey { get; set; } = "TrelloKey";
        public string trelloToken { get; set; } = "TrelloToken";
        public string prefix { get; set; } = "!!";
        public string setGame { get; set; } = "!!help";
        public string botsAPI { get; set; } = "DiscordBotsAPIkey";
        public string dblAPI { get; set; } = "DiscordBotListAPIkey";
        public string dbggAPI { get; set; } = "DiscordBotsGGAPIkey";
        public string BotsOnDiscordAPI { get; set; } = "BotsOnDiscordAPIkey";
        public string DiscordServicesAPI { get; set; } = "DiscordServicesAPI";
        public string DiscordLabsAPI { get; set; } = "DiscordLabsAPI";
        public string StatCordAPI { get; set; } = "StatCordAPI";
        public string GoogleAPIKey { get; set; } = "GoogleAPIKey";
        public string MetricsPort { get; set; } = "9284";
        public bool Debug { get; set; } = false;
        public int IsDev { get; set; } = 0;
    }
}