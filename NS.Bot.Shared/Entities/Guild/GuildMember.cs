using System.Collections.Generic;
using NS.Bot.Shared.Entities.Group;
using NS.Bot.Shared.Enums;

namespace NS.Bot.Shared.Entities.Guild
{
    /// <summary>
    /// Участник Discord-сервера
    /// </summary>
    public class GuildMember : BaseEntity
    {
        /// <summary>
        /// Сервер участника
        /// </summary>
        public GuildEntity Guild { get; set; }

        /// <summary>
        /// Группировка участника
        /// </summary>
        public GroupEntity Group { get; set; }

        /// <summary>
        /// Общие данные участника
        /// </summary>
        public long MemberId { get; set; }
        public MemberEntity Member { get; set; }

        public RoleEnum Role { get; set; } = RoleEnum.Player;
    }
}
 