using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
