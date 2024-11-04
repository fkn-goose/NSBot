using Microsoft.EntityFrameworkCore;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Services
{
    public class TicketService<T> : BaseService<T>, IBaseTicketService<T> where T : TicketBase, new()
    {
        public TicketService(AppDbContext db) : base(db) { }

        public IQueryable<T> GetByGuidId(ulong id)
        {
            return GetAll().Where(x => x.Guild.GuildId == id);
        }

        public async Task<T> GetByMessageId(ulong messageId)
        {
            return await GetAll().FirstOrDefaultAsync(x=>x.MessageId == messageId);
        }

        public async Task<T> GetByChannelId(ulong channelId)
        {
            return await GetAll().FirstOrDefaultAsync(x => x.ChannelId == channelId);
        }
    }
}
