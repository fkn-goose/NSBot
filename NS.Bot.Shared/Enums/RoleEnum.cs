using Discord.Interactions;

namespace NS.Bot.Shared.Enums
{
    public enum RoleEnum
    {
        [ChoiceDisplay("Игрок")]
        Player,
        [ChoiceDisplay("Хелпер")]
        Helper,
        [ChoiceDisplay("Младший куратор")]
        JuniorCurator,
        [ChoiceDisplay("Куратор")]
        Curator,
        [ChoiceDisplay("Старший куратор")]
        SeniorCurator,
        [ChoiceDisplay("РП-Админ")]
        RPAdmin,
        [ChoiceDisplay("Зам. главного администратора")]
        ChiefAdminDeputy,
        [ChoiceDisplay("Главный администратор")]
        ChiefAdmin,
        [ChoiceDisplay("Младший ивентолог")]
        JuniorEventmaster,
        [ChoiceDisplay("Ивентолог")]
        Eventmaster,
        [ChoiceDisplay("Гланвый ивентолог")]
        ChiefEventmaster,
    }
}
