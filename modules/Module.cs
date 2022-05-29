using CustomLogging;
using Discord;
using Discord.Commands;

namespace KillBot.modules
{
    public class Module: ModuleBase<SocketCommandContext>
    {
        private readonly ILogger logger;

        public Module(ILogger logger)
        {
            this.logger = logger;
        }

        [Command("kill")]
        [Summary("Kill This Man! Admonish someone who says or does something cringy or stupid.")]
        public async Task KillAsync(IUser user)
        {
            logger.Verbose("Kill command activated");
            var callingUser = Context.User;
            logger.Debug("{0} killed {1}", callingUser.Username, user.Username);

            // Firugre out how many times this user has killed the target user
            var times = 1;
            string times_str = times == 1 ? "time" : "times";

            // Respond on Discord
            await ReplyAsync($"☠ {callingUser.Username} killed {user.Username} ☠\n☠ {callingUser.Username} has killed {user.Username} **{times} {times_str}** ☠");
        }

    }
}
