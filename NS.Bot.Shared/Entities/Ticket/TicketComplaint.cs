namespace NS.Bot.Shared.Entities.Ticket
{
    public class TicketComplaint : TicketBase
    {
        /// <summary>
        /// Предполагаемый нарушитель
        /// </summary>
        public string DiscrodTag { get; set; }

        /// <summary>
        /// Когда было нарушение
        /// </summary>
        public string When { get; set; }
    }
}
