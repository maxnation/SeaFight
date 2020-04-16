using System.Collections.Generic;

namespace CustomORM
{
   public interface IRepository<T>
    {
        IEnumerable<T> GetAll();

        T Find(int id);

        void Add(T entity);

        void Delete(int entityId);

        void Update(T entity);
    }
}