using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities;
using NS.Bot.Shared.Entities.Guild;

namespace NS.Bot.BuisnessLogic.Services
{
    internal class MemberService : BaseService<MemberEntity>, IMemberService
    {
        private readonly IBaseService<GuildMember> _guildMemberService;
        public MemberService(AppDbContext db, IBaseService<GuildMember> guildMemberService) : base(db) { }
    }
}
