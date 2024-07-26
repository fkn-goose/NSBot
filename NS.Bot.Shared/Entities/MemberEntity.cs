namespace NS.Bot.Shared.Entities
{
    /// <summary>
    /// Данные игрока
    /// </summary>
    public class MemberEntity
    {
        public long Id { get; set; }
        public ulong DiscordId { get; set; }
        public long SteamId { get; set; }
    }
}
