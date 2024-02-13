using Discord;
using Discord.Commands;

namespace NS2Bot.CommandModules
{
    public class PrefixModule : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public async Task Pong()
        {
            await Context.Message.ReplyAsync("Suka!");
        }
    }
}
