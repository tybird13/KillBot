using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using Serilog;

namespace KillBot.services
{
    public class CommandHandler
    {
        private readonly IServiceProvider services;
        private readonly DiscordSocketClient client;
        private readonly CommandService commands;

        public CommandHandler(IServiceProvider services, DiscordSocketClient client, CommandService commands)
        {
            Log.Verbose("Creating Command Handler");
            this.services = services;
            this.client = client;
            this.commands = commands;
        }

        public async Task InstallCommandsAsync()
        {
            Log.Verbose("Install commands async");
            client.MessageReceived += HandleCommandAsync;
            client.Ready += async () =>
            {
                Log.Verbose("Adding modules async command");
                await commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: services);
                Log.Verbose("Done adding modules");
            };
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            Log.Verbose("HANDLE COMMAND");
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
