using NS.Bot.Shared.Entities.Warn;
using System.Linq;

namespace NS.Bot.BuisnessLogic.Interfaces
{
    public interface IWarnService : IBaseService<WarnEntity>
    {
        new public IQueryable<WarnEntity> GetAll();
    }
}
