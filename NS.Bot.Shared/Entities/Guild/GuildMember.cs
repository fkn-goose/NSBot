using System.Collections.Generic;
using NS.Bot.Shared.Entities.Group;

namespace NS.Bot.Shared.Entities.Guild
{
    /// <summary>
    /// Участник Discord-сервера
    /// </summary>
    public class GuildMember : BaseEntity
    {
        public GuildEntity Guild { get; set; }
    }
}
