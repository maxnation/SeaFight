using System;

namespace CustomORM
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple =false)]
    public class TableAttribute : Attribute
    {
        public string TableName { get; set; }

        public TableAttribute(string tableName)
        {
            this.TableName = tableName;
        }
    }
}