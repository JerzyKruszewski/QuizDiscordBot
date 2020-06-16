using System.IO;
using Newtonsoft.Json;

namespace QuizDiscordBot
{
    /// <summary>
    /// Klasa odpowiedzialna za konfigurację bota
    /// </summary>
    internal class Config
    {
        /// <summary>
        /// Folder pliku config
        /// </summary>
        private const string ConfigFolder = "./Resources/Config";

        /// <summary>
        /// Nazwa pliku konfiguracyjnego
        /// </summary>
        private const string ConfigFile = "config.json";

        /// <summary>
        /// Instancja struktury konfiguracji bota
        /// </summary>
        public static BotConfig bot;

        static Config()
        {
            // check if CONFIGFOLDER exist
            if (!Directory.Exists(ConfigFolder))
                Directory.CreateDirectory(ConfigFolder);

            // check if CONFIGFILE exist and get bot configuration from it
            if (!File.Exists(ConfigFolder + "/" + ConfigFile))
            {
                bot = new BotConfig();
                string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
                File.WriteAllText(ConfigFolder + "/" + ConfigFile, json);
            }
            else
            {
                string json = File.ReadAllText(ConfigFolder + "/" + ConfigFile);
                bot = JsonConvert.DeserializeObject<BotConfig>(json);
            }
        }
    }
}
