using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace CustomORM
{
    public abstract class ContextBase
    {
        protected string ConnectionString { get; set; }

        private void CreateRelationDataProc()
        {
            using (SqlConnection connection = new SqlConnection(this.ConnectionString))
            {
                SqlCommand command = new SqlCommand
                {
                    Connection = connection,
                    CommandText = ORMResource.sp_getFkData_Check_Existense
                };
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
            this.ConnectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            this.CreateRelationDataProc();
            var type = this.GetType();

            PropertyInfo[] inheritorDbProperties = type.GetProperties()
                .Where(p => p.PropertyType.Name == typeof(Repository<>).Name).ToArray();

            foreach (PropertyInfo contextProperty in inheritorDbProperties)
            {
                Type propertyType = contextProperty.PropertyType;
                var propertyCtor = propertyType.GetConstructor(
                                BindingFlags.Instance | BindingFlags.NonPublic,
                                binder: null,
                                types: new Type[] { typeof(string) },
                                modifiers: null);

                object propertyInstance = propertyCtor.Invoke(new object[] { this.ConnectionString });
                contextProperty.SetValue(this, propertyInstance);
            }
        }
    }
}