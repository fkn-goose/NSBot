using NS.Bot.Shared.Entities;
using NS.Bot.Shared.Entities.Guild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Interfaces
{
    public interface IMemberService : IBaseService<MemberEntity>
    {
        Task<MemberEntity> GetMemberByGuildMember(GuildMember guildMember);
    }
}
