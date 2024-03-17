using Discord.Interactions;
using System.ComponentModel;

namespace NS2Bot.Enums
{
    public enum ColorsEnum
    {
        [ChoiceDisplay("Красный")]
        [Description("Красный")]
        Red,
        [ChoiceDisplay("Синий")]
        [Description("Синий")]
        Blue,
        [ChoiceDisplay("Зеленый")]
        [Description("Зеленый")]
        Green,
        [ChoiceDisplay("Белый")]
        [Description("Белый")]
        White,
        [ChoiceDisplay("Розовый")]
        [Description("Розовый")]
        Pink,
        [ChoiceDisplay("Желтый")]
        [Description("Желтый")]
        Yellow,
    }
}
