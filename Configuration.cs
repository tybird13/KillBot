using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using KillBot.services;
using CustomLogging;
using KillBot.database;

namespace KillBot
{
    public class Configuration
    {
        public string AppName { get; set; } = "";
        public string DiscordTokenKey { get; set; } = "";
        public string DatabaseFileName { get; set; } = "";
        public string LogLevel { get; set; }

        public Configuration()
        {
            LogLevel = CustomLogging.LogLevel.VERBOSE;
        }

        public static Configuration GetConfiguration(string pathToConfigJsonFile)
        {

            using (StreamReader sr = new StreamReader(pathToConfigJsonFile))
            {
                string json = sr.ReadToEnd();
                Configuration? configuration = JsonConvert.DeserializeObject<Configuration>(json);
                if (configuration == null)
                {
                    throw new Exception($"Unable to load configuration from file '{pathToConfigJsonFile}'");
                }
                return configuration;
            }
        }

        public static ServiceProvider BuildServiceProvider(ILogger logger, DiscordSocketClient client)
        {
            logger.Verbose("Building services");
            ServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(client);

            // set the logging level to verbose
            var commandServiceConfig = new CommandServiceConfig();
            commandServiceConfig.LogLevel = LogSeverity.Verbose;
            serviceCollection.AddSingleton(new CommandService(commandServiceConfig));
            serviceCollection.AddSingleton<Configuration>();
            serviceCollection.AddSingleton<CommandHandler>();
            serviceCollection.AddSingleton(logger);
            serviceCollection.AddDbContext<AppDBContext>();
            logger.Verbose("Finished building the services");
            return serviceCollection.BuildServiceProvider();
        }

    }
}
