using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities;
using System.Linq;

namespace NS.Bot.BuisnessLogic.Services
{
    public class GuildService : BaseService<GuildEntity>, IGuildService
    {
        public GuildService(AppDbContext db) : base(db) { }

        public GuildEntity Get(long id)
        {
            return _db.Guilds.FirstOrDefault(x=>x.Id == id) ?? new GuildEntity();
        }

        public GuildEntity GetByDiscordId(ulong id) 
        {
            return _db.Guilds.FirstOrDefault(x=>x.GuildId == id) ?? new GuildEntity();
        }
    }
}
