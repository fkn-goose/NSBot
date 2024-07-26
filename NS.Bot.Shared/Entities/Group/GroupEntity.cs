using System.Collections.Generic;

namespace NS.Bot.Shared.Entities.Group
{
    /// <summary>
    /// Группировка
    /// </summary>
    public class GroupEntity : BaseEntity
    {
        public string Name { get; set; }
        public GuildEntity Guild { get; set; }
        public List<GroupMember> Members { get; set; }
    }
}
