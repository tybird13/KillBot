using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using CustomLogging;

namespace KillBot.services
{
    public class CommandHandler
    {
        private readonly IServiceProvider services;
        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private readonly ILogger logger;

        public CommandHandler(IServiceProvider services, DiscordSocketClient client, CommandService commands, ILogger logger)
        {
            logger.Verbose("Creating Command Handler");
            this.services = services;
            this.client = client;
            this.commands = commands;
            this.logger = logger;
        }

        public async Task InstallCommandsAsync()
        {
            logger.Verbose("Install commands async");
            client.MessageReceived += HandleCommandAsync;
            await commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: services);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            logger.Verbose("Handle command async method");
            // don't process system messages
            var message = messageParam as SocketUserMessage;
            if (message == null)
            {
                return;
            }

            int argPos = 0;

            // don't process bot triggers
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos)) || message.Author.IsBot)
            {
                return;
            }

            var context = new SocketCommandContext(client, message);

            await commands.ExecuteAsync(context: context, argPos: argPos, services: services);
        }
    }
}          
