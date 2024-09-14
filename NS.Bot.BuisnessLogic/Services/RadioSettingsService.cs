using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities.Guild;
using NS.Bot.Shared.Entities.Radio;
using System.Linq;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Services
{
    public class RadioSettingsService : BaseService<RadioSettings>, IRadioSettingsService
    {
        public readonly IGuildService _guildService;
        public RadioSettingsService(AppDbContext db, IGuildService guildService) : base(db) 
        {
            _guildService = guildService;
        }

        public async Task<RadioSettings> GetRadioSettingsAsync(ulong guildEntityId)
        {
            var curGuild = await _guildService.GetByDiscordId(guildEntityId);
            var radioSettings = GetAll().FirstOrDefault(x=>x.RelatedGuild.Id == curGuild.Id);

            if (radioSettings == null)
            {
                radioSettings = new RadioSettings
                {
                    RelatedGuild = curGuild,
                    IsEnabled = false,
                    CommandChannelId = 0,
                    RadiosCategoryId = 0,
                };

                await CreateOrUpdateAsync(radioSettings);
            }

            return radioSettings;
        }
    }
}
