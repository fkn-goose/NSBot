using NS.Bot.Shared.Entities.Warn;

namespace NS.Bot.Shared.Entities
{
    /// <summary>
    /// Данные игрока
    /// </summary>
    public class MemberEntity : BaseEntity
    {
        public ulong DiscordId { get; set; }
        public ulong? SteamId { get; set; }
        public WarnEntity FirstWarn { get; set; }
        public WarnEntity SecondWarn { get; set; }
        public WarnEntity ThirdWarn { get; set; }
        public uint TotalWarnCount { get; set; }
    }
}
