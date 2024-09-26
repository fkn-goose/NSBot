using Discord.Interactions;

namespace NS.Bot.Shared.Enums
{
    public enum HelperNotificationType
    {
        [ChoiceDisplay("Сменить игровой ник")]
        GameNick,
        [ChoiceDisplay("Сменить диксорд ник")]
        DiscrodNick,
        [ChoiceDisplay("Поставить приписку ГП")]
        SetTag,
        [ChoiceDisplay("Удалить приписку ГП")]
        RemoveTag,
        [ChoiceDisplay("В канал зона")]
        InChannel,
        [ChoiceDisplay("В ЖДК")]
        JDK,
        [ChoiceDisplay("В ЖДХ")]
        JDH,
    }
}
