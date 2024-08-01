using Microsoft.EntityFrameworkCore;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities;
using NS.Bot.Shared.Entities.Guild;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Services
{
    internal class MemberService : BaseService<MemberEntity>, IMemberService
    {
        private readonly IBaseService<GuildMember> _guildMemberService;
        public MemberService(AppDbContext db, IBaseService<GuildMember> guildMemberService) : base(db) { }

        public async Task<MemberEntity> GetMemberByGuildMember(GuildMember guildMember)
        {
            return await GetAll().FirstOrDefaultAsync(x=>x.GuildMembers.Select(y=>y.Id).Contains(guildMember.Id));
        }

        public GuildMember GetCurrentGuildMember(MemberEntity member, GuildEntity guild)
        {
            if (member.GuildMembers == null)
                return null;

            var guildMember = member.GuildMembers.FirstOrDefault(x => x.Guild.GuildId == guild.GuildId);
            return guildMember;
        }
    }
}
