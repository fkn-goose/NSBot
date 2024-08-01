using Microsoft.EntityFrameworkCore;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities;
using NS.Bot.Shared.Entities.Guild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Services
{
    internal class MemberService : BaseService<MemberEntity>, IMemberService
    {
        public MemberService(AppDbContext db) : base(db) { }

        public async Task<MemberEntity> GetMemberByGuildMember(GuildMember guildMember)
        {
            return await GetAll().FirstOrDefaultAsync(x=>x.GuildMembers.Select(y=>y.Id).Contains(guildMember.Id));
        }
    }
}
