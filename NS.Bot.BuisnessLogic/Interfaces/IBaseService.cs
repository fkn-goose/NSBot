using System.Threading.Tasks;

namespace NS.Bot.BuisnessLogic.Interfaces
{
    public interface IBaseService<T>
    {
        Task<long> Update(T entity);
        Task<long> Create(T entity);
    }
}
