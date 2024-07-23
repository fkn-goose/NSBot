using NS.Bot.Shared.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Interfaces
{
    public interface ITicketService : IBaseService<TicketEntity>
    {
        Task<TicketEntity?> Get(long id);
        Task<TicketEntity?> GetByMessageId(ulong messageId);
        Task<TicketEntity?> GetByChannelId(ulong channelId);
        List<TicketEntity> GetByGuidId(ulong id);
        Task<TicketSettings?> GetSettingsByGuildId(ulong guildId);
        void Update<T>(T entity);
    }
}
