namespace NS.Bot.Shared.Entities
{
    /// <summary>
    /// Сервер дискорда
    /// </summary>
    public class GuildEntity : BaseEntity
    {
        /// <summary>
        /// Id сервера в дискорде
        /// </summary>
        public ulong GuildId { get; set; }
        public string Name { get; set; }
    }
}