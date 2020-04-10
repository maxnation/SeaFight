using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace CustomORM
{
        public class Repository<T> : IRepository<T>
        {
            private SqlConnection connection { get; }
            private string tableName { get; }

            private string updateCommand { get; }
            private string deleteCommand { get; }
            private string insertCommand { get; }

            public Repository(SqlConnection connection)
            {
                this.connection = connection;
            }
            #region CRUD
            public void Add(T entity)
            {
                throw new NotImplementedException();
            }

            public void Delete(int id)
            {
                throw new NotImplementedException();
            }

            public T Find(int id)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<T> GetAll()
            {
                throw new NotImplementedException();
            }

            public void Update(T entity)
            {
                throw new NotImplementedException();
            }
            #endregion
        }
    }

