using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;

namespace CustomORM
{
    public abstract class ContextBase
    {
        protected SqlConnection connection;

        protected ContextBase(string connectionStringName)
        {
            string connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            this.connection = new SqlConnection(connectionString);
        }
    }
}
