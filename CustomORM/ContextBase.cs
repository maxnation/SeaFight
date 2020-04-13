using System;
using System.Linq;
using System.Configuration;
using System.Reflection;
using System.Data.SqlClient;

namespace CustomORM
{
    public abstract class ContextBase
    {
        protected string connectionString;

        private void CreateRelationDataProc()
        {
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandText = ORMResource.sp_getFkData_Check_Existense;
                connection.Open();

                int procExistence = (int)command.ExecuteScalar();

                if (procExistence == 0)
                {
                    command.CommandText = ORMResource.sp_getFkData_Creation_Script;
                    command.ExecuteNonQuery();
                }
            }
        }

        protected ContextBase(string connectionStringName)
        {
            this.connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            this.CreateRelationDataProc();
            var type = this.GetType();

            PropertyInfo[] inheritorDbProperties = type.GetProperties()
                .Where(p => p.PropertyType.Name == typeof(Repository<>).Name).ToArray();

            foreach (PropertyInfo contextProperty in inheritorDbProperties) 
            {
                Type propertyType = contextProperty.PropertyType;          
                var propertyCtor = propertyType.GetConstructor(
                                BindingFlags.Instance | BindingFlags.NonPublic,
                                binder: null, types: new Type[] { typeof(string) }, modifiers: null);

                object propertyInstance = propertyCtor.Invoke(new object[] { connectionString });
                contextProperty.SetValue(this, propertyInstance);
            }
        }
    }
}
