using CustomLogging;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KillBot.database;
using KillBot.models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text;
using System.Text.RegularExpressions;

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
        public async Task KillAsync([Remainder] string @params)
        {

            // Bots can't activate the kill command
            if (Context.User.IsBot) return;

            if (String.IsNullOrEmpty(@params) || String.IsNullOrWhiteSpace(@params))
            {
                logger.Error("The parameters to the kill command was empty.");
                await ReplyAsync(FormattedHelpMessage());
                return;
            }

            // Match the mention and reason sections
            const string pattern = "^<@(?<mention>\\S+)>\\s*(\"(?<reason>.+)\")?$";
            var match = Regex.Match(@params, pattern);

            if (!match.Success)
            {
                logger.Error("The parameters for the kill command do not match the required pattern of !kill <user mention> \"reason\"");
                await Context.Channel.SendMessageAsync("To kill someone, use the following format: `!kill @user \"Reason you killed them\"");
                return;
            }

            string userMention = match.Groups["mention"].Value;
            string? reason = match.Groups["reason"].Value;

            // Get the username from the user mention
            IAsyncEnumerable<IReadOnlyCollection<IGuildUser>> users = Context.Guild.GetUsersAsync();
            var allUsers = await users.Flatten().ToListAsync();
            IGuildUser? user = allUsers.FirstOrDefault(u => u.Id.ToString().Equals(userMention, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                logger.Error("Target user {0} was not found!", userMention);
                return;
            }

            logger.Verbose("Kill command activated by {0}", Context.User.Username);
            SocketUser callingUser = Context.User;
            logger.Debug("{0} killed {1}", callingUser.Username, user);

            var kill = new Kill()
            {
                CreatedAt = DateTime.Now,
                KillerUsername = callingUser.Username.ToLower(),
                TargetUsername = user.Username.ToLower(),
                Reason = reason
            };

            EntityEntry killEvent = await database.Kills.AddAsync(kill);
            await database.SaveChangesAsync();
            // Figure out how many times this user has killed the target user
            try
            {
                int times = database.Kills
                    .Where(k => k.TargetUsername.ToLower() == user.Username.ToLower() && k.KillerUsername.ToLower() == callingUser.Username.ToLower())
                    .Count();
                string times_str = times == 1 ? "time" : "times";

                // Respond on Discord
                await ReplyAsync($"☠ {callingUser.Username} killed {user.Username}\n☠ {callingUser.Username} has killed {user.Username} **{times} {times_str}**");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occurred while adding a 'kill' record.");
            }
        }

        [Command("killstats")]
        [Summary("Kill This Man! Get statistics")]
        public async Task KillStatsAsync([Remainder] string? user = null)
        {
            logger.Verbose("Kill statistics method invoked by {0}.", Context.User.Username);
            string currentUser;
            if (user == null)
            {
                // get statistics for the current calling user
                currentUser = Context.User.Username;
            }
            else
            {
                // get statistics for the desired user
                currentUser = user;
            }

            // Find the relevant records

            /*
                ------ you've died x times -----

                ------ you've killed these ppl

                ------ you've been killed by these ppl

                ------- top 10 latest deaths by ppl ----- 

            */
            List<Kill> allRelevantKills = database.Kills
                .Where(k => k.KillerUsername.ToLower().Equals(currentUser.ToLower()) || k.TargetUsername.ToLower().Equals(currentUser.ToLower()))
                .ToList();

            List<Kill> userKills = allRelevantKills.Where(k => k.KillerUsername.ToLower().Equals(currentUser.ToLower())).ToList();

            Task<Dictionary<string, int>> userKillsDictTask = Task.Factory.StartNew(() => userKills
                .GroupBy(k => k.TargetUsername)
                .OrderByDescending(k => k.Count())
                .ToDictionary(k => k.Key, k => k.Count()));

            List<Kill> userDeaths = allRelevantKills.Where(k => k.TargetUsername.ToLower().Equals(currentUser.ToLower())).ToList();
            var topTen = userDeaths.Where(k => !String.IsNullOrEmpty(k.Reason)).OrderByDescending(k => k.CreatedAt).Take(10).ToList();

            Task<Dictionary<string, int>> userDeathsDictTask = Task.Factory.StartNew(() => userDeaths
                .GroupBy(k => k.TargetUsername)
                .OrderByDescending(k => k.Count())
                .ToDictionary(k => k.Key, k => k.Count()));

            // var lastTenTimesKilled = database.Kills

            await Task.WhenAll(userKillsDictTask, userDeathsDictTask);

            Dictionary<string, int> userKillsDict = userKillsDictTask.Result;
            Dictionary<string, int> userDeathsDict = userDeathsDictTask.Result;

            int numTimesUserHasBeenKilled = userDeaths.Count;
            // Get statistics 
            using (var ms = new MemoryStream())
            {
                var writer = new StreamWriter(ms);
                writer.WriteLine($"☠ Here are the statistics for {currentUser}:");
                writer.WriteLine($"☠ You have been killed {numTimesUserHasBeenKilled} time(s)");
                writer.WriteLine("☠ You have killed the following users:");
                writer.WriteLine();
                writer.Write("☠ {0,-50}\t{1,10}", "User", "Kills");
                writer.WriteLine();

                // Append '-' 60 times
                writer.Write("☠ ");
                for (int i = 0; i < 65; i++) { writer.Write("-"); }
                writer.WriteLine();

                foreach (KeyValuePair<string, int> kv in userKillsDict)
                {
                    writer.Write("☠ {0,-50}\t{1,10}", kv.Key, kv.Value);
                    writer.WriteLine();

                }

                writer.WriteLine();
                writer.WriteLine("☠ You have been killed by the following users:");
                writer.Write("☠ {0,-50}\t{1,10}", "User", "Kills");
                writer.WriteLine();

                // Append '-' 60 times
                writer.Write("☠ ");
                for (int i = 0; i < 65; i++) { writer.Write("-"); }
                writer.WriteLine();

                foreach (KeyValuePair<string, int> kv in userDeathsDict)
                {
                    writer.Write("☠ {0,-50}\t{1,10}", kv.Key, kv.Value);
                    writer.WriteLine();

                }

                if (topTen.Any())
                {
                    writer.WriteLine();
                    writer.WriteLine("☠ Here are the last 10 times someone killed you and why:");
                    writer.Write("☠ {0,-50}\t{1,15}\t{2, 50}", "User", "Date", "Reason");
                    writer.WriteLine();

                    // Append '-' 115 times
                    writer.Write("☠ ");
                    for (int i = 0; i < 130; i++) { writer.Write("-"); }
                    writer.WriteLine();


                    foreach (Kill k in topTen)
                    {
                        writer.Write("☠ {0,-50}\t{1,15}\t{2, 50}", k.KillerUsername, k.CreatedAt, k.Reason);
                        writer.WriteLine();
                    }
                }

                writer.Flush();
                await Context.Channel.SendFileAsync(stream: writer.BaseStream, filename: $"{currentUser} kill stats.txt");
            }
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
            bldr.AppendLine("☠ For example: `!kill @tybird13 \"Because he did a stupid thing\"`");
            bldr.AppendLine("☠ For statistics on **your** kills, type `!killstats`");
            bldr.AppendLine("☠ For statistics on other users, type `!killstats @<user>`");
            bldr.AppendLine("☠ Bots cannot activate the kill commands.");
            return bldr.ToString();
        }
    }
}
