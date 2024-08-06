using System.Collections.Generic;
using NS.Bot.Shared.Entities.Group;

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
        public virtual GuildEntity Guild { get; set; }

        /// <summary>
        /// Группировка участника
        /// </summary>
        public virtual GroupEntity Group { get; set; }

        /// <summary>
        /// Общие данные участника
        /// </summary>
        public virtual MemberEntity Member { get; set; }
    }
}
