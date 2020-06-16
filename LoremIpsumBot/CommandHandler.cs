using System;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Interactive;

namespace QuizDiscordBot
{
    /// <summary>
    /// Klasa odpowiedzialna za obsługę komend
    /// </summary>
    internal class CommandHandler
    {
        /// <summary>
        /// Obiekt odpowiedzialny za obsługę konta bota na discordzie
        /// </summary>
        private DiscordSocketClient _client;

        /// <summary>
        /// Framework do obsługi komend
        /// </summary>
        private CommandService _commands;

        /// <summary>
        /// Nie mam pojęcia co to jest, ale bez niego bot nie będzie rozpoznawał komend
        /// </summary>
        private IServiceProvider _service;

        /// <summary>
        /// Zainicjuj obsługę komend dla bota
        /// </summary>
        /// <param name="client">
        /// Bot's discord account
        /// </param>
        /// <returns>
        /// Task object
        /// </returns>
        public async Task InitializeAsync(DiscordSocketClient client)
        {
            _client = client;

            (_service as IDisposable)?.Dispose();
            _service = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(new InteractiveService(_client))
                .BuildServiceProvider();

            CommandServiceConfig cmdConfig = new CommandServiceConfig
            {
                DefaultRunMode = RunMode.Async
            };

            (_commands as IDisposable)?.Dispose();
            _commands = new CommandService(cmdConfig);
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _service);

            _client.MessageReceived += HandleCommandAsync;
        }

        /// <summary>
        /// Reakcja bota na wiadoość
        /// </summary>
        /// <param name="s">
        /// Message on discord server/guild
        /// </param>
        /// <returns>
        /// Task object
        /// </returns>
        private async Task HandleCommandAsync(SocketMessage s)
        {
            try
            {
                // Check is received message is convertable to SocketUserMessage
                if (!(s is SocketUserMessage msg)) return;

                // Create message context
                SocketCommandContext context = new SocketCommandContext(_client, msg);

                // Check if message starts with a prefix specified in Config file or has mention bot
                int argPos = 0;
                if (msg.HasStringPrefix(Config.bot.cmdPrefix, ref argPos)
                    || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
                {
                    // Try to execute command
                    IResult result = await _commands.ExecuteAsync(context, argPos, _service);

                    // If it was not success log the reason to console.
                    if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                    {
                        Console.WriteLine(result.ErrorReason);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }

}
