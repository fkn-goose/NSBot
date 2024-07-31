using Microsoft.EntityFrameworkCore;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities.Guild;
using System.Linq;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Services
{
    public class GuildService : BaseService<GuildEntity>, IGuildService
    {
        public GuildService(AppDbContext db) : base(db) { }

        public async Task<GuildEntity> GetByDiscordId(ulong id) 
        {
            return await GetAll().FirstOrDefaultAsync(x=>x.GuildId == id);
        }
    }
}
