using Discord.Interactions;

namespace NS.Bot.Shared.Enums
{
    public enum WarnType
    {
        [ChoiceDisplay("Устный")]
        Verbal,
        [ChoiceDisplay("Обычный")]
        Ordinary,
        [ChoiceDisplay("ReadOnly")]
        ReadOnly,
        [ChoiceDisplay("Выговор")]
        Rebuke,
    }
}
