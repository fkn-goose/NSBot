using NS.Bot.Shared.Entities;
using NS.Bot.Shared.Entities.Guild;
using System.Linq;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Interfaces
{
    public interface IGuildMemberService : IBaseService<GuildMember>
    {
        /// <summary>
        /// Ищет участника сервера
        /// </summary>
        /// <param name="member">Экземпляр участника</param>
        /// <param name="guild">Экземпляр текущего сервера</param>
        /// <returns>Участник сервера</returns>
        Task<GuildMember> GetByMemberAsync(MemberEntity member, GuildEntity guild);

        /// <summary>
        /// Поиск участника сервера
        /// </summary>
        /// <param name="discordId">Discord Id уастника</param>
        /// <param name="guildId">Discord Id сервера на котором производится поиск</param>
        /// <returns>Участник сервера, если не найден, то null</returns>
        Task<GuildMember> GetByDiscordIdAsync(ulong discordId, ulong guildId);
    }
}
