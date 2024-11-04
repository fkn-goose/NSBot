using System.Collections.Generic;

namespace NS.Bot.Shared.Models
{
    public class TicketSettingsList
    {
        public List<TicketSettingsModel> Settings { get; set; }
    }

    public class TicketSettingsModel
    {
        public string RelatedGuildName { get; set; }
        public ulong RelatedGuildId { get; set; }

        public ulong HelperNewTicketsCategoryId { get; set; }

        /// <summary>
        /// Канал новых тикетов для хелперов
        /// </summary>
        public ulong HelperNewTicketsChannelId { get; set; }

        /// <summary>
        /// Категория закрытых тикетов для хелперов
        /// </summary>
        public ulong HelperOldTicketsChannelId { get; set; }


        #region Curator

        /// <summary>
        /// Категория новых тикетов для кураторов
        /// </summary>
        public ulong CuratorNewTicketsCategoryId { get; set; }

        /// <summary>
        /// Канал новых тикетов для кураторов
        /// </summary>
        public ulong CuratorNewTicketsChannelId { get; set; }

        /// <summary>
        /// Категория закрытых тикетов для кураторов
        /// </summary>
        public ulong CuratorOldTicketsChannelId { get; set; }

        #endregion

        #region Admin

        /// <summary>
        /// Категория новых тикетов для админов
        /// </summary>
        public ulong AdminNewTicketsCategoryId { get; set; }

        /// <summary>
        /// Канал новых тикетов для админов
        /// </summary>
        public ulong AdminNewTicketsChannelId { get; set; }

        /// <summary>
        /// Категория закрытых тикетов для админов
        /// </summary>
        public ulong AdminOldTicketsChannelId { get; set; }

        #endregion

        /// <summary>
        /// Канал логов для тикетов
        /// </summary>
        public ulong TicketLogs { get; set; }

        /// <summary>
        /// Категория закрытых тикетов
        /// </summary>
        public ulong ClosedTicketsCategoryId { get; set; }

        #region MenuData

        /// <summary>
        /// Канал для новых тикетов получения бонусов
        /// </summary>
        public ulong BonusesNewTicketChannel { get; set; }

        /// <summary>
        /// Канал для новых тикетов восстановления вещей
        /// </summary>
        public ulong ItemsNewTicketChannel { get; set; }

        /// <summary>
        /// Канал для новых тикетов жалоб на игроков
        /// </summary>
        public ulong ComplaintPlayerNewTicketChannel { get; set; }

        /// <summary>
        /// Канал для новых тикетов жалоб на администрацию
        /// </summary>
        public ulong ComplaintAdminNewTicketChannel { get; set; }

        /// <summary>
        /// Канал для закрытых тикетов получения бонусов
        /// </summary>
        public ulong NameChangeNewTicketChannel { get; set; }

        /// <summary>
        /// Канал для закрытых тикетов получения бонусов
        /// </summary>
        public ulong BonusesOldTicketChannel { get; set; }

        /// <summary>
        /// Канал для закрытых тикетов восстановления вещей
        /// </summary>
        public ulong ItemsOldTicketChannel { get; set; }

        /// <summary>
        /// Канал для закрытых тикетов жалоб на игроков
        /// </summary>
        public ulong ComplaintPlayerOldTicketChannel { get; set; }

        /// <summary>
        /// Канал для закрытых тикетов жалоб на администрацию
        /// </summary>
        public ulong ComplaintAdminOldTicketChannel { get; set; }

        /// <summary>
        /// Канал для новых тикетов смены позывного
        /// </summary>
        public ulong NameChangeOldTicketChannel { get; set; }

        #endregion
    }
}
