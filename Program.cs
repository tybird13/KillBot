using Discord;
using CustomLogging;
using KillBot;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using KillBot.services;
using Microsoft.Extensions.Configuration;

public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();
    public static Configuration _config = Configuration.GetConfiguration("config.json");
    public static ILogger Logger = new LogProvider(_config.LogLevel, Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KillBot", "Log.log")).Logger;

    public async Task MainAsync()
    {
        Logger.Verbose("Getting configuration..");
        Logger.Verbose("Logger file path: {0}", Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KillBot", "Log.log"));
        // Get configuration
        Logger.Debug("Starting Discord Bot {0}.", _config.AppName);

        string? token = Environment.GetEnvironmentVariable(_config.DiscordTokenKey);
        Logger.Verbose("Discord Token Key: {0} => {1}", _config.DiscordTokenKey, token);

        DiscordSocketClient client = new DiscordSocketClient();
        client.Log += Log;

        await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable(_config.DiscordTokenKey));

        Logger.Debug("Starting client");

        await client.StartAsync();

        // Configure services
        ServiceProvider services = Configuration.BuildServiceProvider(Logger, client);
        CommandHandler commandHandler = services.GetRequiredService<CommandHandler>();
        await commandHandler.InstallCommandsAsync();

        Logger.Information("Client started successfully. Status: {0}", client.Status);

        await Task.Factory.StartNew(() => Console.ReadLine());

        Logger.Information("Shutting down...");
    }

    public static async Task Log(LogMessage msg)
    {
        await Task.Factory.StartNew(() =>
        {
            switch (msg.Severity)
            {
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
        });
    }

    public static void DBLog(string msg)
    {
        string _msg = "DB LOG: " + msg;

        switch (_config.LogLevel)
        {
            case LogLevel.DEBUG:
                Logger.Debug(_msg);
                break;
            case LogLevel.INFO:
                Logger.Information(_msg);
                break;
            case LogLevel.WARN:
                Logger.Warning(_msg);
                break;
            case LogLevel.ERROR:
                Logger.Error(_msg);
                break;
            default:
                Logger.Verbose(_msg);
                break;
        }
    }
}


