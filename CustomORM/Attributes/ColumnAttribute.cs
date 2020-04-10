using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomORM.Attributes
{
    public class ColumnAttribute : Attribute
    {
        public string ColumnName;

        public ColumnAttribute(string columnName)
        {
            this.ColumnName = columnName;
        }
    }
}
