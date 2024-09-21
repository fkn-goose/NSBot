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

        public async Task<GuildMember> GetByMemberAsync(MemberEntity member, GuildEntity guild)
        {
            var guildMember = await GetAll().FirstOrDefaultAsync(x => x.Member.Id == member.Id && x.Guild.GuildId == guild.GuildId);
            if(guildMember == null)
            {
                guildMember = new GuildMember()
                {
                    Guild = guild,
                    Member = member
                };
                await CreateOrUpdateAsync(guildMember);
            }

            return guildMember;
        }

        public async Task<GuildMember> GetByDiscordIdAsync(ulong discordId, ulong guildId)
        {
            var result = await GetAll().FirstOrDefaultAsync(x=>x.Member.DiscordId == discordId && x.Guild.GuildId == guildId);
            return result;
        }
    }
}
