namespace NS.Bot.Shared.Entities.Guild
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

        /// <summary>
        /// Название сервера
        /// </summary>
        public string Name { get; set; }
    }
}