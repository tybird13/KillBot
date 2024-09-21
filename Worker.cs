using Discord.WebSocket;
using Discord;
using KillBot.services;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace KillBot
{
    internal class Worker : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly CommandHandler _commandHandler;
        private readonly DiscordSocketClient _client;

        public Worker(IConfiguration config, CommandHandler commandHandler, DiscordSocketClient client)
        {
            _config = config;
            _commandHandler = commandHandler;
            _client = client;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);
            _client.Log += Program.LogMethod;

            Log.Debug("Starting Discord Bot {0}.", _config.GetValue<string>("AppName"));

            string key = _config.GetValue<string>("DiscordTokenKey");
            string? token = Environment.GetEnvironmentVariable(key);

            if (token == null)
            {
                string msg = string.Format("The token {0} was not set properly.", _config.GetValue<string>("DiscordTokenKey"));
                Log.Fatal(msg);
                throw new ApplicationException(msg);
            }

            Log.Verbose("LOGGING IN");
            await _client.LoginAsync(TokenType.Bot, token);
            Log.Verbose("LOGIN SUCCESSFUL");

            Log.Debug("Starting client");
            await _client.StartAsync();

            await _commandHandler.InstallCommandsAsync();
            Log.Information("Client started successfully. Status: {0}", _client.Status);

        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            Log.Information("Shutting down KillBot...");
            _client.Dispose();

        }
    }
}
