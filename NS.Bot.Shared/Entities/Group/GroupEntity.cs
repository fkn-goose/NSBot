using System.Collections.Generic;
using NS.Bot.Shared.Entities.Guild;
using NS.Bot.Shared.Enums;

namespace NS.Bot.Shared.Entities.Group
{
    /// <summary>
    /// Группировка
    /// </summary>
    public class GroupEntity : BaseEntity
    {
        /// <summary>
        /// Группировка
        /// </summary>
        public GroupsEnum GroupType { get; set; }

        /// <summary>
        /// Сервер к которому принадлежит группировка
        /// </summary>
        public GuildEntity Guild { get; set; }

        /// <summary>
        /// Лидер группировки
        /// </summary>
        public long? Leader { get; set; }
    }
}
