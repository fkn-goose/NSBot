using Discord.Rest;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Interfaces
{
    public interface ILogToFileService
    {
        Task Info(string message);
        Task Error(string message);
    }
}
