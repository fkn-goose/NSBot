using Microsoft.EntityFrameworkCore;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities.Warn;
using System.Linq;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Services
{
    public class WarnSettingsService : BaseService<WarnSettings>, IWarnSettingsService
    {
        public WarnSettingsService(AppDbContext db, IGuildService guildService) : base(db)
        {
        }

        public async Task<WarnSettings> GetWarnSettingsAsync(ulong guildEntityId)
        {
            var warnSettings = await GetAll().FirstOrDefaultAsync(x => x.RelatedGuild.GuildId == guildEntityId);

            return warnSettings;
        }
    }
}
