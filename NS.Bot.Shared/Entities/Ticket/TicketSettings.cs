using NS.Bot.Shared.Entities.Guild;

namespace NS.Bot.Shared.Entities
{
    public class TicketSettings : BaseEntity
    {
        public GuildEntity Related { get; set; }

        #region Helper
        /// <summary>
        /// Категория новых тикетов для хелперов
        /// </summary>
        public ulong NewHelperTicketsCategoryId { get; set; }

        /// <summary>
        /// Канал новых тикетов для хелперов
        /// </summary>
        public ulong NewHelperTicketsChannelId { get; set; }

        /// <summary>
        /// Категория старых тикетов для хелперов
        /// </summary>
        public ulong OldHelperTicketsCategory { get; set; }

        #endregion

        #region Curator

        /// <summary>
        /// Категория новых тикетов для хелперов
        /// </summary>
        public ulong NewCuratorTicketsCategoryId { get; set; }

        /// <summary>
        /// Канал новых тикетов для хелперов
        /// </summary>
        public ulong NewCuratorTicketsChannelId { get; set; }

        /// <summary>
        /// Категория старых тикетов для хелперов
        /// </summary>
        public ulong OldCuratorTicketsCategory { get; set; }

        #endregion

        #region Admin

        /// <summary>
        /// Категория новых тикетов для хелперов
        /// </summary>
        public ulong NewAdminTicketsCategoryId { get; set; }

        /// <summary>
        /// Канал новых тикетов для хелперов
        /// </summary>
        public ulong NewAdminTicketsChannelId { get; set; }

        /// <summary>
        /// Категория старых тикетов для хелперов
        /// </summary>
        public ulong OldAdminTicketsCategory { get; set; }

        #endregion

        /// <summary>
        /// Канал логов для тикетов
        /// </summary>
        public ulong TicketLogs { get; set; }
        public uint HelperTicketsCount { get; set; }
        public uint CuratorTicketsCount { get; set; }
        public uint AdminTicketsCount { get; set; }
    }
}
