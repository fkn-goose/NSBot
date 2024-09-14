using Microsoft.EntityFrameworkCore;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities;
using NS.Bot.Shared.Entities.Warn;
using System.Linq;

namespace NS.Bot.BuisnessLogic.Services
{
    public class WarnService : BaseService<WarnEntity>, IWarnService
    {
        private readonly IBaseService<MemberEntity> _memberService;
        public WarnService(AppDbContext db) : base(db)
        {
        }

        new public IQueryable<WarnEntity> GetAll()
        {
            return _db.Warns.Include(x => x.IssuedTo);
        }
    }
}
