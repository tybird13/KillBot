using KillBot.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace KillBot.database
{
    public class AppDBContext : DbContext
    {
        private readonly IConfiguration _config;

        public DbSet<Kill> Kills { get; set; }

        public AppDBContext(IConfiguration config)
        {
            _config = config;
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.EnableDetailedErrors()
                .LogTo((msg) => DBLog(msg), Microsoft.Extensions.Logging.LogLevel.Warning);

            var folder = "KillBot";

            var pathFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            Log.Verbose("pathFolder: {0}", pathFolder);
            var dbPath = Path.Join(pathFolder, folder);

            Log.Verbose("IS_DOCKER: {0}", _config.GetValue<bool>("IS_DOCKER"));
            if (_config.GetValue<bool>("IS_DOCKER"))
            {
                pathFolder = Path.Join("app", "database");
            }
            else
            {
                if (!Directory.Exists(dbPath))
                {
                    Directory.CreateDirectory(dbPath);
                }
            }

            Log.Verbose("Database {0}, \t{1}", pathFolder, dbPath);

            SqliteConnectionStringBuilder bldr = new SqliteConnectionStringBuilder();
            var filename = _config.GetValue<string>("DatabaseFileName");
            var fn = Path.Join(dbPath, filename);
            Log.Information("Database File: {0}", Path.GetFullPath(fn));
            bldr.DataSource = fn;
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
