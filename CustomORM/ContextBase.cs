using System;
using System.Linq;
using System.Configuration;
using System.Reflection;

namespace CustomORM
{
    public abstract class ContextBase
    {
        protected string connectionString;

        protected ContextBase(string connectionStringName)
        {
            this.connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;

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
