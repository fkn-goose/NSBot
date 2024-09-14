using NS.Bot.Shared.Entities.Warn;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Interfaces
{
    public interface IWarnSettingsService : IBaseService<WarnSettings>
    {
        Task<WarnSettings> GetWarnSettingsAsync(ulong guildEntityId);
    }
}
