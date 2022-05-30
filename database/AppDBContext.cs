using KillBot.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace KillBot.database
{
    public class AppDBContext: DbContext
    {
        public DbSet<Kill> Kills { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.EnableDetailedErrors()
                .LogTo((msg) => Program.DBLog(msg), Microsoft.Extensions.Logging.LogLevel.Warning);

            var folder = "KillBot";

            var pathFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbPath = Path.Join(pathFolder, folder);

            if (!Directory.Exists(dbPath))
            {
                Directory.CreateDirectory(dbPath);
            }
            
            Program.Logger.Verbose("Database File Path: {0}", dbPath);

             SqliteConnectionStringBuilder bldr = new SqliteConnectionStringBuilder();
            bldr.DataSource = Path.Join(dbPath, Program._config.DatabaseFileName);
            string conn = bldr.ConnectionString.ToString();
            options.UseSqlite(new SqliteConnection(conn));

        }
    }
}
