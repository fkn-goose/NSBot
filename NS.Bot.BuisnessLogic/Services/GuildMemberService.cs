﻿using Microsoft.EntityFrameworkCore;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities;
using NS.Bot.Shared.Entities.Guild;
using System.Linq;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Services
{
    public class GuildMemberService : BaseService<GuildMember>, IGuildMemberService
    {
        public GuildMemberService(AppDbContext db) : base(db) { }

        //public async Task<GuildMember> GetByMember(MemberEntity member)
        //{
        //    return await GetAll().FirstOrDefaultAsync(x=>x.Member.Id == member.Id);
        //}
    }
}
