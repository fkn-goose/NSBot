using NS.Bot.Shared.Entities.Guild;

namespace NS.Bot.Shared.Entities.Radio
{
    public class RadioEntity : BaseEntity
    {
        /// <summary>
        /// Discord-id рации
        /// </summary>
        public ulong VoiceChannelId { get; set; }

        /// <summary>
        /// Название канала
        /// </summary>
        public string VoiceName { get; set; }

        /// <summary>
        /// Сервер, на котором создана рация
        /// </summary>
        public GuildEntity Guild { get; set; }
    }
}
