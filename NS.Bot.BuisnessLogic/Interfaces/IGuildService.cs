using NS.Bot.Shared.Entities;

namespace NS.Bot.BuisnessLogic.Interfaces
{
    public interface IGuildService : IBaseService<GuildEntity>
    {
        GuildEntity Get(long id); 
        GuildEntity GetByDiscordId(ulong id);
    }
}
