using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities.Warn;

namespace NS.Bot.BuisnessLogic.Services
{
    public class WarnService : BaseService<WarnEntity>, IWarnService
    {
        public WarnService(AppDbContext db) : base(db)
        {
        }
    }
}
