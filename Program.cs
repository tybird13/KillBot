using Discord;
using KillBot;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using KillBot.services;
using Serilog;
using Microsoft.Extensions.Hosting;
using Discord.Commands;
using KillBot.database;
using Microsoft.Extensions.Configuration;
using Serilog.Events;

public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync()
    {

        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) => BuildServices(services, context))
            .ConfigureHostConfiguration(config =>
            {
                Log.Verbose("Getting configuration..");
                config.AddJsonFile("config.json");
            })
            .Build();

        await host.RunAsync();
    }

    public static async Task LogMethod(LogMessage msg)
    {
        await Task.Factory.StartNew(() =>
        {
            switch (msg.Severity)
            {
                case LogSeverity.Debug:
                    Log.Debug(msg.ToString());
                    break;
                case LogSeverity.Info:
                    Log.Information(msg.ToString());
                    break;
                case LogSeverity.Warning:
                    Log.Warning(msg.ToString());
                    break;
                case LogSeverity.Error:
                    Log.Error(msg.ToString());
                    break;
                default:
                    Log.Verbose(msg.ToString());
                    break;
            }
        });
    }

    public static void BuildServices(IServiceCollection serviceCollection, HostBuilderContext context)
    {
        LogProvider.CreateLogger(LogEventLevel.Verbose);

        Log.Verbose("Building services...");

        serviceCollection.AddSingleton<IConfiguration>(provider => context.Configuration);

        var commandServiceConfig = new CommandServiceConfig();
        commandServiceConfig.LogLevel = LogSeverity.Verbose;
        serviceCollection.AddSingleton(new CommandService(commandServiceConfig));

        serviceCollection.AddSingleton<CommandHandler>();
        serviceCollection.AddDbContext<AppDBContext>();
        serviceCollection.AddSingleton<DiscordSocketClient>();
        serviceCollection.AddHostedService<Worker>();
        Log.Verbose("Finished building the services");
    }
}


