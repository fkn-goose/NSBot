using Discord.Interactions;

namespace NS.Bot.Shared.Enums
{
    public enum TimeUnitEnum
    {
        [ChoiceDisplay("секунд")]
        Seconds,
        [ChoiceDisplay("минут")]
        Minutes,
        [ChoiceDisplay("часов")]
        Hours,
        [ChoiceDisplay("дней")]
        Days
    }
}
