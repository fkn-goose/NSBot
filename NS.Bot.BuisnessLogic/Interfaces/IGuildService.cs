using NS.Bot.Shared.Entities.Guild;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Interfaces
{
    public interface IGuildService : IBaseService<GuildEntity>
    {
        /// <summary>
        /// Ищет сервер по его DiscrodId
        /// </summary>
        /// <param name="id">uuid discord сервера</param>
        /// <returns>Экземпляр сервера</returns>
        Task<GuildEntity> GetByDiscordId(ulong id);
    }
}
