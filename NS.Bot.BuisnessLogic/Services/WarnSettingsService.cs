using Microsoft.EntityFrameworkCore;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities.Warn;
using System.Linq;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Services
{
    public class WarnSettingsService : BaseService<WarnSettings>, IWarnSettingsService
    {
        private readonly IGuildService _guildService;
        public WarnSettingsService(AppDbContext db, IGuildService guildService) : base(db)
        {
            _guildService = guildService;
        }

        public async Task<WarnSettings> GetWarnSettingsAsync(ulong guildEntityId)
        {
            var curGuild = await _guildService.GetByDiscordId(guildEntityId);
            var warnSettings = await GetAll().FirstOrDefaultAsync(x => x.RelatedGuild.Id == curGuild.Id);

            return warnSettings;
        }
    }
}
