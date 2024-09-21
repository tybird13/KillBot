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
    private IConfiguration _config;

    public Program()
    {
        // Create the configuration
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(path: "config.json");

        // Build the configuration and assign to the config.
        _config = builder.Build();
    }

    public static Task Main(string[] args) => new Program().MainAsync(args);

    public async Task MainAsync(string[] args)
    {
        try{

        IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) => ConfigureServices(services, context))
            .ConfigureHostConfiguration(config =>
            {
                Log.Verbose("Getting configuration..");
                config.AddJsonFile("config.json");
            })
            .Build();

        await host.RunAsync();
        Log.Verbose("SHUTDOWN");
        }
        catch(Exception e)
        {
            Log.Error(e, "FATAL ERROR");
        }
        finally{
            Log.Fatal("AAAGGGHHH");
        }
    }

    public static async Task LogMethod(LogMessage message)
    {
        var severity = message.Severity switch
        {
            LogSeverity.Critical => LogEventLevel.Fatal,
            LogSeverity.Error => LogEventLevel.Error,
            LogSeverity.Warning => LogEventLevel.Warning,
            LogSeverity.Info => LogEventLevel.Information,
            LogSeverity.Verbose => LogEventLevel.Verbose,
            LogSeverity.Debug => LogEventLevel.Debug,
            _ => LogEventLevel.Verbose
        };
        Log.Write(severity, message.Exception, "[DISCORD MESSAGE][{Source}] {Message}", message.Source, message.Message);
        await Task.CompletedTask;
    }

    public static void ConfigureServices(IServiceCollection serviceCollection, HostBuilderContext context)
    {
        LogProvider.CreateLogger(LogEventLevel.Verbose);

    Log.Verbose("Building services...");

        DiscordSocketClient client = new(
            new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All,
                LogLevel = LogSeverity.Verbose
            }
        );

        serviceCollection.AddSingleton(client);
        serviceCollection.AddSingleton(provider => context.Configuration);

    var commandServiceConfig = new CommandServiceConfig();
    commandServiceConfig.LogLevel = LogSeverity.Verbose;
    serviceCollection.AddSingleton(new CommandService(commandServiceConfig));

    serviceCollection.AddSingleton<CommandHandler>();
    serviceCollection.AddDbContext<AppDBContext>();
    serviceCollection.AddHostedService<Worker>();
    Log.Verbose("Finished building the services");
}
}


