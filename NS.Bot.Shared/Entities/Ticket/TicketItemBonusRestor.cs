namespace NS.Bot.Shared.Entities.Ticket
{
    public class TicketItemBonusRestor : TicketBase
    {
        /// <summary>
        /// SteamId того кто подает
        /// </summary>
        public string SteamId { get; set; }

        /// <summary>
        /// Список того что нужно восстановить
        /// </summary>
        public string What { get; set; }

        /// <summary>
        /// Когда были утеряны
        /// </summary>
        public string When { get; set; }

    }
}
