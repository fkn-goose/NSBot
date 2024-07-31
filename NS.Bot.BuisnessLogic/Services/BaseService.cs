using Microsoft.EntityFrameworkCore;
using NS.Bot.BuisnessLogic.Interfaces;
using NS.Bot.Shared.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Services
{
    public class BaseService<T> : IBaseService<T> where T : BaseEntity, new()
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

        public async Task<T> Get(long id)
        {
            return await _db.Set<T>().AsNoTracking().FirstOrDefaultAsync(x=>x.Id == id);
        }

        public IQueryable<T> GetAll()
        {
            return _db.Set<T>().AsNoTracking();
        }
    }
}
