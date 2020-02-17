namespace NFe.Core.Interfaces
{
    public interface IRepository<T>
    {
        void Insert(T obj);
        void Save();
    }
}
