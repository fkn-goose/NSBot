namespace NS.Bot.Shared.Entities.Guild
{
    public class GuildData : BaseEntity
    {
        public long RelatedGuildId { get; set; }
        public virtual GuildEntity RelatedGuild { get; set; }
        public ulong ZoneVoiceId { get; set; }
        public ulong JDKVoiceId { get; set; }
        public ulong JDHVoiceId { get; set; }
    }
}
