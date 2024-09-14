using NS.Bot.Shared.Entities;
using NS.Bot.Shared.Entities.Guild;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Interfaces
{
    public interface IMemberService : IBaseService<MemberEntity>
    {
        /// <summary>
        /// Получение пользователя
        /// </summary>
        /// <param name="discordId">Discord ID</param>
        /// <returns>Пользователь, если не найден, то null</returns>
        Task<MemberEntity> GetByDiscordIdAsync(ulong discordId);

        /// <summary>
        /// Создание пользователя
        /// </summary>
        /// <param name="discordId">Discord ID</param>
        /// <param name="currentGuild">Текущий сервер</param>
        /// <returns>Новый созданный пользователь</returns>
        Task<MemberEntity> CreateMemberAsync(ulong discordId, GuildEntity currentGuild);
    }
}
