using Discord;
using CustomLogging;
using KillBot;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using KillBot.services;

public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();
    public static ILogger Logger = new LogProvider("VERBOSE").Logger;

    public async Task MainAsync()
    {
        // Get configuration
        Configuration config = Configuration.GetConfiguration("config.json");        
        Logger.Debug("Starting Discord Bot {0}.", config.AppName);
        
        string? token = Environment.GetEnvironmentVariable(config.DiscordTokenKey);
        Logger.Verbose("Discord Token Key: {0} => {1}", config.DiscordTokenKey, token);

        DiscordSocketClient client = new DiscordSocketClient();
        client.Log += Log;

        await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable(config.DiscordTokenKey));

        Logger.Debug("Starting client");

        await client.StartAsync();

        // Configure services
        ServiceProvider services = Configuration.BuildServiceProvider(Logger, client);
        CommandHandler commandHandler = services.GetRequiredService<CommandHandler>();
        await commandHandler.InstallCommandsAsync();



        Logger.Information("Client started successfully.\nStatus: {0)", client.Status);

        await Task.Delay(-1);

        Logger.Information("Shutting down...");
    }

    public static async Task Log(LogMessage msg)
    {
        switch (msg.Severity){
            case LogSeverity.Debug:
                Logger.Debug(msg.ToString());
                break;
            case LogSeverity.Info:
                Logger.Information(msg.ToString());
                break;
            case LogSeverity.Warning:
                Logger.Warning(msg.ToString());
                break;
            case LogSeverity.Error:
                Logger.Error(msg.ToString());
                break;
            default:
                Logger.Verbose(msg.ToString());
                break;
        }
    }


}