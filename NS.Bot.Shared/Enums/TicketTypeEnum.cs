using System.ComponentModel;

namespace NS.Bot.Shared.Enums
{
    public enum TicketTypeEnum
    {
        [Description("хелпер")]
        Helper,
        [Description("смена позывного")]
        ChangeNick,
        [Description("куратор")]
        Curator,
        [Description("восстановление вещей")]
        ItemsRestor,
        [Description("восстановление бонусов")]
        BonusesRestor,
        [Description("жалоба")]
        Complaint,
        [Description("админ")]
        Admin
    }
}
