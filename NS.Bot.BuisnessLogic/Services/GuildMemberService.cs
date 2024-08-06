using Microsoft.EntityFrameworkCore;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities;
using NS.Bot.Shared.Entities.Guild;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Services
{
    public class GuildMemberService : BaseService<GuildMember>, IGuildMemberService
    {
        public GuildMemberService(AppDbContext db) : base(db) { }

        public async Task<GuildMember> GetByMember(MemberEntity member, GuildEntity guild)
        {
            return await base.GetAll().FirstOrDefaultAsync(x => x.Member.Id == member.Id && x.Guild.GuildId == guild.GuildId);
        }

        //Я блять не знаю почему, но если не перезаписывать метод с инклюдом, то он не включает Member`ов никогда. Даже если отключить lazy
        new public IQueryable<GuildMember> GetAll()
        {
            return _db.GuildMembers.Include(x => x.Member).Include(x => x.Guild).Include(x => x.Group);
        }
    }
}
