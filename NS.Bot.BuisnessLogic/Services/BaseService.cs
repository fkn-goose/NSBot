using NS.Bot.Shared.Entities;

namespace NS.Bot.BuisnessLogic.Services
{
    public class BaseService<T> where T : BaseEntity
    {
        protected readonly AppDbContext _db;
        public BaseService(AppDbContext db) 
        {
            _db = db;
        }

        public async void Update(T entity)
        {
            _db.Update(entity);
            await _db.SaveChangesAsync();
        }

        public async void Create(T entity)
        {
            _db.Add(entity);
            await _db.SaveChangesAsync();
        }
    }
}
