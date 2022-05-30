using CustomLogging;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KillBot.database;
using KillBot.models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Globalization;
using System.Linq;
using System.Text;

namespace KillBot.modules
{
    public class Module : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger logger;
        private readonly AppDBContext database;

        public Module(ILogger logger, AppDBContext database)
        {
            this.logger = logger;
            this.database = database;
        }

        [Command("kill")]
        [Summary("Kill This Man! Admonish someone who says or does something cringy or stupid.")]
        public async Task KillAsync(IUser user)
        {

            // Bots can't activate the kill command
            if (Context.User.IsBot) return;

            logger.Verbose("Kill command activated");
            SocketUser callingUser = Context.User;
            logger.Debug("{0} killed {1}", callingUser.Username, user.Username);
            logger.Verbose("Adding record");

            EntityEntry killEvent = await database.Kills.AddAsync(new Kill() { KillerUsername = callingUser.Username, TargetUsername = user.Username });
            await database.SaveChangesAsync();
            // Figure out how many times this user has killed the target user
            int times = database.Kills
                .Where(k => k.TargetUsername == user.Username && k.KillerUsername == callingUser.Username)
                .Count();
            string times_str = times == 1 ? "time" : "times";

            // Respond on Discord
            await ReplyAsync($"☠ {callingUser.Username} killed {user.Username}\n☠ {callingUser.Username} has killed {user.Username} **{times} {times_str}**");
        }

        [Command("killstats")]
        [Summary("Kill This Man! Get statistics on yourself.")]
        public async Task KillStatsAsync(IUser? user = null)
        {
            logger.Verbose("Kill statistics method invoked. User is {0}", user);
            IUser currentUser;
            if (user == null)
            {
                // get statistics for the current calling user
                currentUser = Context.User;
            }
            else
            {
                // get statistics for the desired user
                currentUser = user;
            }

            // Find the relevent records
            List<Kill> usersKills = database.Kills.Where(k => k.KillerUsername == currentUser.Username).ToList();
            List<Kill> timesCurrentUserWasKilled = database.Kills.Where(k => k.TargetUsername == currentUser.Username).ToList();

            int numTimesUserHasBeenKilled = timesCurrentUserWasKilled.Count;


            var userkillsdictTask = Task.Factory.StartNew(() => usersKills.GroupBy(k => k.TargetUsername).OrderByDescending(k => k.Count()).ToDictionary(k => k.Key, k => k.Count()));
            var userWasKilledDictTask = Task.Factory.StartNew(() => timesCurrentUserWasKilled.GroupBy(k => k.KillerUsername).OrderByDescending(k => k.Count()).ToDictionary(k => k.Key, k => k.Count()));

            await Task.WhenAll(userkillsdictTask, userWasKilledDictTask);

            var userkillsdict = userkillsdictTask.Result;
            var userWasKilledDict = userWasKilledDictTask.Result;

            // Get statistics on yourself
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"☠ Here are the statistics for {currentUser.Username}:")
              .AppendLine($"☠ You have been killed {numTimesUserHasBeenKilled} time(s)")
              .AppendLine("☠ You have killed the following users:")
              .AppendFormat("☠ {0,-50}\t{1,10}", "User", "Kills").AppendLine();

            // Append '-' 60 times
            sb.Append("☠ ");
            for (int i = 0; i < 55; i++) { sb.Append("-"); }
            sb.AppendLine();

            foreach (KeyValuePair<string, int> kv in userkillsdict)
            {
                sb.AppendFormat("☠ {0,-50}\t{1,10}", kv.Key, kv.Value).AppendLine();
            }

            sb.AppendLine("☠")
              .AppendLine("☠")
              .AppendLine("☠ You have been killed by the following users:")
              .AppendFormat("☠ {0,-50}\t{1,10}", "User", "Kills").AppendLine();
            
            // Append '-' 60 times
            sb.Append("☠ ");
            for (int i = 0; i < 55; i++) { sb.Append("-"); }
            sb.AppendLine();

            foreach (KeyValuePair<string, int> kv in userWasKilledDict)
            {
                sb.AppendFormat("☠ {0,-50}\t{1,10}", kv.Key, kv.Value).AppendLine();
            }

            if (user == null)
            {
                await currentUser.SendMessageAsync(sb.ToString());
                await ReplyAsync($"☠ {currentUser.Username} has been killed {numTimesUserHasBeenKilled} time(s)");
            }
            else
            {
                await ReplyAsync(sb.ToString());
            }

        }

        /// <summary>
        /// You can't use text with this commant, return help message
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [Command("killstats")]
        public async Task KillstatsAsync(string? token)
        {
            // Bots can't activate the help command
            if (Context.User.IsBot) return;

            await ReplyAsync(FormattedHelpMessage());
        }

        [Command("kill")]
        public async Task KillHelpAsync(string? token)
        {
            logger.Verbose("Unknown token {0}. Either the user is offline or it's bad text.", token);
            await ReplyAsync($"Unknown token ```{token}```Either this user is offline, or it's bad text.");
        }

        [Command("kill")]
        [Summary("Kill This Man! Admonish someone who says or does something cringy or stupid.")]
        public async Task KillHelpAsync()
        {
            // Bots can't activate the help command
            if (Context.User.IsBot) return;

            await ReplyAsync(FormattedHelpMessage());
        }

        private string FormattedHelpMessage()
        {
            StringBuilder bldr = new StringBuilder();
            bldr.AppendLine("☠ Thanks for using KillThisMan!");
            bldr.AppendLine("☠ Admonish someone who says or does something cringy or stupid.");
            bldr.AppendLine("☠ To use this bot, type `!kill` and then @ the user you wish to kill");
            bldr.AppendLine("☠ For example: `!kill @tybird13`");
            bldr.AppendLine("☠ For statistics on **your** kills, type `!killstats`");
            bldr.AppendLine("☠ For statistics on other users, type `!killstats @<user>");
            bldr.AppendLine("☠ Bots cannot activate the kill commands.");
            return bldr.ToString();
        }
    }
}
