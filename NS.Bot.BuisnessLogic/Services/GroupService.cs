using Microsoft.EntityFrameworkCore;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities.Group;
using NS.Bot.Shared.Entities.Guild;
using NS.Bot.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Services
{
    public class GroupService : BaseService<GroupEntity>, IGroupService
    {
        public GroupService(AppDbContext db) : base(db) { }
        public async Task<GroupEntity> GetGuildMembersGroup(GuildMember member)
        {
            return await GetAll().FirstOrDefaultAsync(x=>x.Guild.GuildId == member.Guild.GuildId && x.Groupmembers.Select(x=>x.Id).Contains(member.Id));
        }

        public async Task<GroupEntity> GetGroupByEnum(GroupsEnum groupEnum, GuildEntity currentGuild)
        {
            return await GetAll().FirstOrDefaultAsync(x => x.Group == groupEnum && x.Guild.GuildId == currentGuild.GuildId);
        }
    }
}
