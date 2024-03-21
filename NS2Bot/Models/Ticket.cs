namespace NS2Bot.Models
{
    public class Ticket
    {
        public ulong NewTicketsCategoryId { get; set; }
        public ulong TicketsChannelId { get; set; }
        public ulong TicketsLogChannelId { get; set; }
        public int TicketsCount { get; set; }
        public Dictionary<ulong, ulong> MessageChannelTickerPair { get; set; }
    }
}
