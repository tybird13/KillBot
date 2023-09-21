using Microsoft.Extensions.Logging;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog.Core;

namespace KillBot
{
    public class LogProvider
    {
        private static LoggingLevelSwitch loggingLevelSwitch = new LoggingLevelSwitch();

        public static void CreateLogger(LogEventLevel logLevel)
        {
            var theme = new SystemConsoleTheme(new Dictionary<ConsoleThemeStyle, SystemConsoleThemeStyle>
            {
                [ConsoleThemeStyle.Text] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White },
                [ConsoleThemeStyle.SecondaryText] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Gray },
                [ConsoleThemeStyle.TertiaryText] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.DarkGray },
                [ConsoleThemeStyle.Invalid] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Yellow },
                [ConsoleThemeStyle.Null] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Blue },
                [ConsoleThemeStyle.Name] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Gray },
                [ConsoleThemeStyle.String] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Cyan },
                [ConsoleThemeStyle.Number] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Magenta },
                [ConsoleThemeStyle.Boolean] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Blue },
                [ConsoleThemeStyle.Scalar] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Green },
                [ConsoleThemeStyle.LevelVerbose] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Magenta },
                [ConsoleThemeStyle.LevelDebug] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Cyan },
                [ConsoleThemeStyle.LevelInformation] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Blue },
                [ConsoleThemeStyle.LevelWarning] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Yellow },
                [ConsoleThemeStyle.LevelError] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White, Background = ConsoleColor.Red },
                [ConsoleThemeStyle.LevelFatal] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White, Background = ConsoleColor.Red },
            });


            switch (logLevel)
            {
                case LogEventLevel.Verbose:
                    loggingLevelSwitch.MinimumLevel = LogEventLevel.Verbose;
                    break;
                case LogEventLevel.Debug:
                    loggingLevelSwitch.MinimumLevel = LogEventLevel.Debug;
                    break;
                case LogEventLevel.Information:
                    loggingLevelSwitch.MinimumLevel = LogEventLevel.Information;
                    break;
                case LogEventLevel.Warning:
                    loggingLevelSwitch.MinimumLevel = LogEventLevel.Warning;
                    break;
                case LogEventLevel.Error:
                    loggingLevelSwitch.MinimumLevel = LogEventLevel.Error;
                    break;
                default:
                    throw new ArgumentException($"{nameof(logLevel)} does not match any logging levels. Value was {logLevel}.");
            }


            // Create the logger
            string template = "[{@Timestamp:yyyy-MM-dd HH:mm:ss} {@Level:u3}] {@Message:lj}{Properties}{NewLine}{Exception}";
            string path = "";
            if (Environment.GetEnvironmentVariable("IS_DOCKER")?.Equals("TRUE") ?? false)
            {
                path = Path.Combine("/app", AppDomain.CurrentDomain.FriendlyName, $"{AppDomain.CurrentDomain.FriendlyName}-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.log");
            }
            else
            {
                path = Path.Combine(Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)), AppDomain.CurrentDomain.FriendlyName, $"{AppDomain.CurrentDomain.FriendlyName}-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.log");
            }

            Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .MinimumLevel.ControlledBy(loggingLevelSwitch)
                    .WriteTo.Console(
                        theme: theme,
                        outputTemplate: template
                    )
                    .WriteTo.File(
                        path: path,
                        outputTemplate: template,
                        fileSizeLimitBytes: 536870912,
                        rollOnFileSizeLimit: true
                    )
                    .CreateLogger();

            Log.Information("Logger initialized. Log files are located at {0}", path);
        }

    }
}
