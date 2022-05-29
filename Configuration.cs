using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using KillBot.services;
using CustomLogging;

namespace KillBot
{
    public class Configuration
    {
        public string AppName { get; set; } = "";
        public string DiscordTokenKey { get; set; } = "";

        public static Configuration GetConfiguration(string pathToConfigJsonFile)
        {
            using (StreamReader sr = new StreamReader(pathToConfigJsonFile))
            {
                Configuration? configuration = JsonConvert.DeserializeObject<Configuration>(sr.ReadToEnd());
                if(configuration == null)
                {
                    throw new Exception($"Unable to load configuration from file '{pathToConfigJsonFile}'");
                }
                return configuration;
            }
        }

        public static ServiceProvider BuildServiceProvider(ILogger logger, DiscordSocketClient client) {
            logger.Verbose("Building services");
            ServiceCollection sc = new ServiceCollection();

            sc.AddSingleton(client);

            // set the logging level to verbose
            var commandServiceConfig = new CommandServiceConfig();
            commandServiceConfig.LogLevel = LogSeverity.Verbose;

            sc.AddSingleton(new CommandService(commandServiceConfig));
            sc.AddSingleton<CommandHandler>();
            sc.AddSingleton(logger);
            logger.Verbose("Finished building the services");
            return sc.BuildServiceProvider();
        }

    }
}
