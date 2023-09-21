using KillBot.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Configuration;

namespace KillBot.database
{
    public class AppDBContext: DbContext
    {
        private readonly IConfiguration _config;

        public DbSet<Kill> Kills { get; set; }

        public AppDBContext(IConfiguration config){
            _config = config;
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.EnableDetailedErrors()
                .LogTo((msg) => DBLog(msg), Microsoft.Extensions.Logging.LogLevel.Warning);

            var folder = "KillBot";

            var pathFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbPath = Path.Join(pathFolder, folder);

            if (!Directory.Exists(dbPath))
            {
                Directory.CreateDirectory(dbPath);
            }
            
            Log.Verbose("Database File Path: {0}", dbPath);

             SqliteConnectionStringBuilder bldr = new SqliteConnectionStringBuilder();
            var filename = _config.GetValue<string>("DatabaseFileName");
            bldr.DataSource = Path.Join(dbPath, filename);
            string conn = bldr.ConnectionString.ToString();
            options.UseSqlite(new SqliteConnection(conn));
            
        }

        public void DBLog(string msg)
        {
            string _msg = "DB LOG: " + msg;

            switch (_config.GetValue<LogEventLevel>("LogLevel"))
            {
                case LogEventLevel.Debug:
                    Log.Debug(_msg);
                    break;
                case LogEventLevel.Information:
                    Log.Information(_msg);
                    break;
                case LogEventLevel.Warning:
                    Log.Warning(_msg);
                    break;
                case LogEventLevel.Error:
                    Log.Error(_msg);
                    break;
                default:
                    Log.Verbose(_msg);
                    break;
            }
        }
    }
}
