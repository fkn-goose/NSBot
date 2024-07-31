using NS.Bot.Shared.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Interfaces
{
    public interface ITicketService : IBaseService<TicketEntity>
    {
        IQueryable<TicketEntity> GetByGuidId(ulong id);
        Task<TicketEntity> GetByMessageId(ulong messageId);
        Task<TicketEntity> GetByChannelId(ulong channelId);
        Task<TicketSettings> GetSettingsByGuildId(ulong guildId);
        void Update<T>(T entity);
    }
}
