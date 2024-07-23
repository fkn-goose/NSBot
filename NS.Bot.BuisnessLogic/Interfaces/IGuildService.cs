using NS.Bot.Shared.Entities;

namespace NS.Bot.BuisnessLogic.Interfaces
{
    public interface IGuildService
    {
        GuildEntity Get(long id); 
        GuildEntity GetByDiscordId(ulong id);
    }
}
