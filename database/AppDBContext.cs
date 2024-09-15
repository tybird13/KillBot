using KillBot.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Configuration;

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

            var newFolder = "KillBot";

            string pathFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            
            if(string.IsNullOrEmpty(pathFolder))
                pathFolder = Directory.GetCurrentDirectory();

            Log.Verbose("pathFolder: {0}", pathFolder);
            var dbPath = Path.Join(pathFolder, "database", newFolder);
            Log.Verbose("dbPath: {0}", dbPath);
            if (string.IsNullOrEmpty(pathFolder))
                Log.Warning("pathFolder is null or empty!");
            if (string.IsNullOrEmpty(dbPath))
                Log.Warning("dbPath is null or empty!");
            

            SqliteConnectionStringBuilder bldr = new SqliteConnectionStringBuilder();
            var filename = _config.GetValue<string>("DatabaseFileName");


            string fn = Path.Join(dbPath, filename);

            try
            {
                if (!File.Exists(fn))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fn));
                }
            }
            catch (IOException e)
            {
                Log.Error(e, "Something went wrong while creating the database folder {0}", fn);
            }

            Log.Verbose("Database File Path: {0}", fn);
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
