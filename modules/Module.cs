using Discord.Commands;

namespace KillBot.modules
{
    public class Module: ModuleBase<SocketCommandContext>
    {

        [Command("say")]
        public Task SayAsync([Remainder] [Summary("The text to echo")] string str) => ReplyAsync(str);

    }
}
