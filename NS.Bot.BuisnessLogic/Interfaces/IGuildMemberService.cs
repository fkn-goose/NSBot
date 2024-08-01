using NS.Bot.Shared.Entities;
using NS.Bot.Shared.Entities.Guild;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Interfaces
{
    public interface IGuildMemberService : IBaseService<GuildMember>
    {
        /// <summary>
        /// Ищет участника сервера
        /// </summary>
        /// <param name="member">Экземпляр участника</param>
        /// <returns>Участник сервера</returns>
        //Task<GuildMember> GetByMember(MemberEntity member);
    }
}
