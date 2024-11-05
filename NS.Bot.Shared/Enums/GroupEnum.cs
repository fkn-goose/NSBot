using Discord.Interactions;
using System.ComponentModel;

namespace NS.Bot.Shared.Enums
{
    public enum GroupEnum
    {
        [ChoiceDisplay("Одиночка")]
        [Description("Одиночка")]
        Loner,
        [ChoiceDisplay("ИИГ")]
        [Description("ИИГ")]
        IRG,
        [ChoiceDisplay("НИГ БЛИК")]
        [Description("НИГ БЛИК")]
        Science,
        [ChoiceDisplay("Грех")]
        [Description("Грех")]
        Sin,
        [ChoiceDisplay("Военсталы")]
        [Description("Военсталы")]
        Voenstal,
        [ChoiceDisplay("Охотники")]
        [Description("Охотники")]
        Hunters,
        [ChoiceDisplay("Вороны")]
        [Description("Вороны")]
        Crows,
        [ChoiceDisplay("Монолит")]
        [Description("Монолит")]
        Monolith,
        [ChoiceDisplay("Альфа псы")]
        [Description("Альфа псы")]
        Alphadogs,
        [ChoiceDisplay("Сотники")]
        [Description("Сотники")]
        Bartenders,
        [ChoiceDisplay("Механики")]
        [Description("Механики")]
        Mehanics,
        [ChoiceDisplay("Нейтралы")]
        [Description("Нейтралы")]
        Neutrals,
        [ChoiceDisplay("Долг")]
        [Description("Долг")]
        Duty,
        [ChoiceDisplay("ОКСОП")]
        [Description("ОКСОП")]
        OKSOP,
        [ChoiceDisplay("Свобода")]
        [Description("Свобода")]
        Freedom,
        [ChoiceDisplay("Ренегаты")]
        [Description("Ренегаты")]
        Renegades,
        [ChoiceDisplay("Чистое небо")]
        [Description("Чистое небо")]
        ClearSky,
        [ChoiceDisplay("Бандиты")]
        [Description("Бандиты")]
        Bandits,
        [ChoiceDisplay("Наёмники")]
        [Description("Наёмники")]
        Mercenaries,
        [ChoiceDisplay("Охрана деревни")]
        [Description("Охрана деревни")]
        Villagers
    }
}
