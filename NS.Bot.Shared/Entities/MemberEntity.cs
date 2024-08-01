using NS.Bot.Shared.Entities.Guild;
using System.Collections.Generic;

namespace NS.Bot.Shared.Entities
{
    /// <summary>
    /// Данные игрока
    /// </summary>
    public class MemberEntity : BaseEntity
    {
        public ulong DiscordId { get; set; }
        public ulong? SteamId { get; set; }
    }
}
