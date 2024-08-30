using NS.Bot.Shared.Entities.Guild;
using NS.Bot.Shared.Entities.Radio;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Interfaces
{
    public interface IRadioSettingsService : IBaseService<RadioSettings>
    {
        Task<RadioSettings> GetRadioSettingsAsync(ulong guildEntityId);
    }
}
