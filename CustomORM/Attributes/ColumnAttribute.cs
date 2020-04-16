using System;

namespace CustomORM
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ColumnAttribute : Attribute
    {
        public string ColumnName { get; set; }

        public ColumnAttribute(string columnName)
        {
            this.ColumnName = columnName;
        }
    }
}