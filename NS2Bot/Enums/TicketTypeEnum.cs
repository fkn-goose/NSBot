using System.ComponentModel;

namespace NS2Bot.Enums
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
