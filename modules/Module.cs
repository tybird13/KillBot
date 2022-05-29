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
        public async Task KillAsync(IUser user)
        {
            logger.Verbose("Kill command activated");
            await ReplyAsync($"☠ User: {user.Username} ☠");
        }

    }
}
