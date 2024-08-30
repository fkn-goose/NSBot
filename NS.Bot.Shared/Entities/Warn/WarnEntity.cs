namespace NS.Bot.Shared.Entities.Warn
{
    /// <summary>
    /// Предупреждение
    /// </summary>
    public class WarnEntity : BaseEntity
    {
        /// <summary>
        /// Тот кто выдал
        /// </summary>
        public MemberEntity Responsible { get; set; }

        /// <summary>
        /// Тот кому выдали
        /// </summary>
        public MemberEntity IssuedTo { get; set; }

        /// <summary>
        /// Причина
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Длительность(в секундах)
        /// </summary>
        public uint Duration { get; set; }

        /// <summary>
        /// Бессрочный
        /// </summary>
        public bool Indefinite { get; set; }

        /// <summary>
        /// Устный
        /// </summary>
        public bool IsVerbal { get; set; }
    }
}
