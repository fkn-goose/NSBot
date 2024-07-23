namespace NS.Bot.BuisnessLogic.Interfaces
{
    public interface IBaseService<T>
    {
        void Update(T entity);
        void Create(T entity);
    }
}
