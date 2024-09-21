using System.ComponentModel;

namespace NS.Bot.Shared.Enums
{
    public enum LogType
    {
        [Description("Info")]
        Info,
        [Description("Warning")]
        Warning,
        [Description("Error")]
        Error,
    }
}
