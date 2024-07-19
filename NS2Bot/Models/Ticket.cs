using NS2Bot.Enums;

namespace NS2Bot.Models
{
    public class Ticket
    {
        public TicketSettings TicketSettings { get; set; }
        public List<TicketData> TicketsData { get; set; }
        public uint HelperTicketsCount { get; set; }
        public uint CuratorTicketsCount { get; set; }
        public uint AdminTicketsCount { get; set; }
    }

    /// <summary>
    /// Настройки каналов для тикетов
    /// </summary>
    public class TicketSettings
    {
        //Категория для новых каналов с тикетами, канал для сообщений о появлении нового тикета
        public ulong NewHelperTicketsCategoryId { get; set; }
        public ulong NewHelperTicketsChannelId { get; set; }
        public ulong OldHelperTicketsCategory { get; set; }
        public ulong NewCuratorTicketsCategoryId { get; set; }
        public ulong NewCuratorTicketsChannelId { get; set; }
        public ulong OldCuratorTicketsCategory { get; set; }
        public ulong NewAdminTicketsCategoryId { get; set; }
        public ulong NewAdminTicketsChannelId { get; set; }
        public ulong OldAdminTicketsCategory { get; set; }
        public ulong TicketLogs {  get; set; }
    }

    public class TicketData
    {
        /// <summary>
        /// Айди канала тикета
        /// </summary>
        public ulong ChannelId { get; set; }

        /// <summary>
        /// Айди сообщения по которому с кнопкой "Взять тикет в работу"
        /// </summary>
        public ulong MessageId { get; set; }

        /// <summary>
        /// Тип тикета
        /// </summary>
        public TicketTypeEnum Type { get; set; }

        /// <summary>
        /// Время после которого тикет можно удалять
        /// </summary>
        public DateTime? DeleteTime { get; set; }

        /// <summary>
        /// Закрыт ли тикет
        /// </summary>
        public bool IsFinished { get; set; } = false;
    }
}
