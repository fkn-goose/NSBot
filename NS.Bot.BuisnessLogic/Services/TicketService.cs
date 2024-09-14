using Microsoft.EntityFrameworkCore;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Services
{
    public class TicketService : BaseService<TicketEntity>, ITicketService
    {
        public TicketService(AppDbContext db) : base(db) { }

        public IQueryable<TicketEntity> GetByGuidId(ulong id)
        {
            return GetAll().Where(x => x.Guild.GuildId == id);
        }

        public async Task<TicketEntity> GetByMessageId(ulong messageId)
        {
            return await GetAll().FirstOrDefaultAsync(x=>x.MessageId == messageId);
        }

        public async Task<TicketEntity> GetByChannelId(ulong channelId)
        {
            return await GetAll().FirstOrDefaultAsync(x => x.ChannelId == channelId);
        }

        public async Task<TicketSettings> GetSettingsByGuildId(ulong guildId)
        {
            return await _db.TicketSettings.FirstOrDefaultAsync(x=>x.Related.GuildId == guildId);
        }
        
        public async void Update<T>(T entity)
        {
            if (entity == null)
                return;
            _db.Update(entity);
            await _db.SaveChangesAsync();
        }
    }
}
