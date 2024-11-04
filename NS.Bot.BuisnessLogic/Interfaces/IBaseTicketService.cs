using NS.Bot.Shared.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Interfaces
{
    public interface IBaseTicketService<T> : IBaseService<T> where T : TicketBase
    {
        IQueryable<T> GetByGuidId(ulong id);
        Task<T> GetByMessageId(ulong messageId);
        Task<T> GetByChannelId(ulong channelId);
    }
}
