namespace NS2Bot.Models
{
    public class Ticket
    {
        public ulong NewTicketsCategoryId { get; set; }
        public ulong OldTicketsCategory { get; set; }
        public ulong NewTicketsChannelId { get; set; }
        public ulong OldTicketsChannelId { get; set; }
        public uint TicketsCount { get; set; }

        /// <summary>
        /// Ключ - сообщение по которому даётся доступ к каналу из значения
        /// </summary>
        public Dictionary<ulong, ulong> MessageTitcketPair { get; set; }

        /// <summary>
        /// Ключ - время удаление канала, значение - канал для удаления
        /// </summary>
        public Dictionary<DateTime, ulong> OldTickets { get; set; }
    }
}
