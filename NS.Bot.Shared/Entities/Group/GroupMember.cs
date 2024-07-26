using System.Collections.Generic;

namespace NS.Bot.Shared.Entities.Group
{
    /// <summary>
    /// Член группировки
    /// </summary>
    public class GroupMember
    {
        public long Id { get; set; }
        public GuildEntity Guild { get; set; }
        public MemberEntity Member { get; set; }
        public List<GroupEntity> Group { get; set; }
    }
}
