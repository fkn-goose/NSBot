using Microsoft.EntityFrameworkCore;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities;
using NS.Bot.Shared.Entities.Guild;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Services
{
    public class MemberService : BaseService<MemberEntity>, IMemberService
    {
        private readonly IBaseService<GuildMember> _guildMemberService;
        public MemberService(AppDbContext db, IBaseService<GuildMember> guildMemberService) : base(db) 
        { 
            _guildMemberService = guildMemberService;
        }

        public async Task<MemberEntity> GetByDiscordIdAsync(ulong discordId)
        {
            var result = await _db.Members.FirstOrDefaultAsync(x => x.DiscordId == discordId);
            return result;
        }

        public async Task<MemberEntity> CreateMemberAsync(ulong discordId, GuildEntity currentGuild)
        {
            var member = await CreateMember(discordId);
            await CreateGuildMember(member, currentGuild);

            return member;
        }

        private async Task<MemberEntity> CreateMember(ulong userId)
        {
            var member = new MemberEntity()
            {
                DiscordId = userId
            };
            await CreateOrUpdateAsync(member);

            return member;
        }

        private async Task<GuildMember> CreateGuildMember(MemberEntity member, GuildEntity currentGuild)
        {
            var guildMember = new GuildMember()
            {
                Guild = currentGuild,
                Member = member
            };
            await _guildMemberService.CreateOrUpdateAsync(guildMember);

            return guildMember;
        }
    }
}
