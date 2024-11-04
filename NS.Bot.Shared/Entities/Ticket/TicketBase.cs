using NS.Bot.Shared.Entities.Guild;
using NS.Bot.Shared.Enums;
using System;

namespace NS.Bot.Shared.Entities
{
    public class TicketBase : BaseEntity
    {
        /// <summary>
        /// Сервер к которому принадлежит тикет
        /// </summary>
        public long GuildId { get; set; }
        public virtual GuildEntity Guild { get; set; }

        /// <summary>
        /// Id создателя тикета
        /// </summary>
        public long CreatedById { get; set; }

        /// <summary>
        /// Id ответственного
        /// </summary>
        public long ResponsibleId { get; set; }

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
        /// Приична обращения
        /// </summary>
        public string Reason { get; set; }

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
