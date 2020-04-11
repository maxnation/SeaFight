using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomORM
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple =false)]
    public class TableAttribute : Attribute
    {
        public string TableName;

        public TableAttribute(string tableName)
        {
            this.TableName = tableName;
        }
    }
}
