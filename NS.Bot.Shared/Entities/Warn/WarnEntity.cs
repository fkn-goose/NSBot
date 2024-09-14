using System;

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
        /// Когда выдан
        /// </summary>
        public DateTime FromDate { get; set; }

        /// <summary>
        /// Когда заканчивается
        /// </summary>
        public DateTime ToDate { get; set; }

        /// <summary>
        /// Длительность в секундах
        /// </summary>
        public uint Duration { get; set; }

        /// <summary>
        /// Бессрочный
        /// </summary>
        public bool Permanent { get; set; } = false;

        /// <summary>
        /// Устный
        /// </summary>
        public bool IsVerbal { get; set; } = false;

        /// <summary>
        /// Выговор
        /// </summary>
        public bool IsRebuke { get; set; } = false;

        /// <summary>
        /// Активен
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
