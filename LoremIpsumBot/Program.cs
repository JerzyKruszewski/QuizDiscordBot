using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace QuizDiscordBot
{
    /// <summary>
    /// Algorytm ułatwiający naukę z dowolnej dziedziny, zaimplementowany w bota discordowego.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Obiekt odpowiedzialny za obsługę konta bota na discordzie
        /// </summary>
        private DiscordSocketClient _client;

        /// <summary>
        /// Instancja klasy odpowiedzialnej za reakcje na komendy
        /// </summary>
        private CommandHandler _handler;

        /// <summary>
        /// Main
        /// </summary>
        private static void Main()
        => new Program().RunBotAsync().GetAwaiter().GetResult(); //Run program asynchronously

        /// <summary>
        /// Asynchroniczne zadanie uruchamiające bota
        /// </summary>
        /// <returns></returns>
        public async Task RunBotAsync()
        {
            try
            {
                //Check if bot has OAUTH2.0 token
                if (Config.bot.token == "" || Config.bot.token == null)
                {
                    throw new ArgumentException("Invalid token");
                }

                _client?.Dispose();
                _client = new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Verbose
                });

                await InitializationClient();
                await InitializationLogs();

                //Prevent closing the program.
                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Przygotuj bota do pracy
        /// </summary>
        /// <returns></returns>
        private async Task InitializationClient()
        {
            await LoginAsync();
            await HandlerInitialize();
        }

        /// <summary>
        /// Zaloguj bota na serwer
        /// </summary>
        /// <returns></returns>
        private async Task LoginAsync()
        {
            await _client.LoginAsync(TokenType.Bot, Config.bot.token);
            await _client.StartAsync();
        }

        /// <summary>
        /// Przygotuj obsługę komend
        /// </summary>
        /// <returns></returns>
        private async Task HandlerInitialize()
        {
            _handler = new CommandHandler();
            await _handler.InitializeAsync(_client);
        }

        /// <summary>
        /// Przygotuj logi
        /// </summary>
        /// <returns>
        /// Task object
        /// </returns>
        private Task InitializationLogs()
        {
            _client.Log += BotLog; //subscribe to Log event

            return Task.CompletedTask;
        }

        /// <summary>
        /// Wypisz wiadomość logu w konsoli
        /// </summary>
        /// <param name="msg">
        /// LogMessage object used for logging purposes
        /// </param>
        /// <returns>
        /// Task object
        /// </returns>
        private Task BotLog(LogMessage msg)
        {
            Console.WriteLine(msg.Message);
            return Task.CompletedTask;
        }
    }
}
