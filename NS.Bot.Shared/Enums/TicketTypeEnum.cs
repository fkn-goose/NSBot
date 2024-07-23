using System.ComponentModel;

namespace NS.Bot.Shared.Enums
{
    public enum TicketTypeEnum
    {
        [Description("хелпер")]
        Helper,
        [Description("куратор")]
        Curator,
        [Description("админ")]
        Admin
    }
}
