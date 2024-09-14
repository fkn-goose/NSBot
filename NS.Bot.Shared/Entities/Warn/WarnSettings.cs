using NS.Bot.Shared.Entities.Guild;

namespace NS.Bot.Shared.Entities.Warn
{
    public class WarnSettings : BaseEntity
    {
        public GuildEntity RelatedGuild { get; set; }
        public ulong WarnChannelId { get; set; }
        public ulong FirstWarnRoleId {  get; set; }
        public ulong SecondWarnRoleId { get; set; }
        public ulong ThirdWarnRoleId { get; set; }
        public ulong BanRoleId { get; set; }
        public ulong ReadOnlyRoleId { get; set; }
    }
}
