using NS.Bot.Shared.Entities;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Services
{
    public class BaseService<T> where T : BaseEntity
    {
        protected readonly AppDbContext _db;
        public BaseService(AppDbContext db) 
        {
            _db = db;
        }

        public async Task<long> Update(T entity)
        {
            _db.Update(entity);
            await _db.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<long> Create(T entity)
        {
            _db.Add(entity);
            await _db.SaveChangesAsync();
            return entity.Id;
        }
    }
}
