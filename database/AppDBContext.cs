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
                .LogTo((msg) => Program.DBLog(msg), Microsoft.Extensions.Logging.LogLevel.Debug);
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbPath = Path.Join(folder, Program._config.DatabaseFileName);

             SqliteConnectionStringBuilder bldr = new SqliteConnectionStringBuilder();
            bldr.DataSource = dbPath;
            string conn = bldr.ConnectionString.ToString();
            options.UseSqlite(new SqliteConnection(conn));

        }
    }
}
