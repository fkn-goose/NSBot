using NS.Bot.Shared.Entities.Warn;
using System.Collections.Generic;

namespace NS.Bot.Shared.Entities
{
    /// <summary>
    /// Данные игрока
    /// </summary>
    public class MemberEntity : BaseEntity
    {
        public ulong DiscordId { get; set; }
        public string? SteamId { get; set; }
        public ICollection<WarnEntity> Warns { get; set; }
        public uint TotalWarnCount { get; set; }
    }
}
