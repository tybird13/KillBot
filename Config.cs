using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog.Events;

namespace KillBot
{
    public class Config
    {
        public string AppName { get; set; } = "";
        public string DiscordTokenKey { get; set; } = "";
        public string DatabaseFileName { get; set; } = "";
        public LogEventLevel LogLevel { get; set; } = LogEventLevel.Verbose;        

    }
}
