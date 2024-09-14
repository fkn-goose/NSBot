using NS.Bot.Shared.Entities.Guild;

namespace NS.Bot.Shared.Entities.Radio
{
    public class RadioSettings : BaseEntity
    {
        /// <summary>
        /// Сервер дискорда для которого используются настройки
        /// </summary>
        public GuildEntity RelatedGuild { get; set; }

        /// <summary>
        /// Включение системы раций
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// ID канала в который необходимо отправлять команду
        /// </summary>
        public ulong CommandChannelId { get; set; }

        /// <summary>
        /// ID категории в которой будут создаваться рации
        /// </summary>
        public ulong RadiosCategoryId { get; set; }
    }
}
