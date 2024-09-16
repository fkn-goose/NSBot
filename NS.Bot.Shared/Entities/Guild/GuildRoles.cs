namespace NS.Bot.Shared.Entities.Guild
{
    public class GuildRoles : BaseEntity
    {
        public GuildEntity RelatedGuild { get; set; }
        public ulong PlayerRoleId { get; set; }
        public ulong HelperRoleId { get; set; }
        public ulong JuniorCuratorRoleId { get; set; }
        public ulong CuratorRoleId { get; set; }
        public ulong SeniorCuratorRoleId { get; set; }
        public ulong RPAdminRoleId { get; set; }
        public ulong ChiefAdminDeputyRoleId { get; set; }
        public ulong ChiefAdminRoleId { get; set; }
        public ulong JuniorEventmasterRoleId { get; set; }
        public ulong EventmasterRoleId { get; set; }
        public ulong ChiefEventmasterRoleId { get; set; }
        public ulong AdminListMessageChannelId { get; set; }
        public ulong AdminListMessageId { get; set; }
    }
}
