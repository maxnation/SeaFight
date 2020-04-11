﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Reflection;

namespace CustomORM
{
    public class Repository<T> : IRepository<T>
    {
        private string connectionString;
        private string tableName;
        private string updateCommand;
        private string deleteCommand;
        private string insertCommand;

        private Repository(string connectionString)
        {
            this.connectionString = connectionString;
            this.Initialize();
        }
        #region Initialization
        private void Initialize()
        {
            SetTableName();
            SetCommands();
        }
        private void SetCommands()
        {
            var props = GetDbFiledsNames();
            SetUpdateCommandText(props);
            SetInsertCommandText(props);
            SetDeleteCommandText(props);
        }
        private void SetUpdateCommandText(IEnumerable<string> props)
        {
            string updateCommandText = $"UPDATE {tableName} SET ";
            foreach (var prop in props)
            {
                updateCommandText += $"{prop} = @{prop}, ";
            }
            updateCommandText = updateCommandText.TrimEnd(',', ' ');
            updateCommandText += " WHERE Id = @Id";
            updateCommand = updateCommandText;
        }
        private void SetInsertCommandText(IEnumerable<string> props)
        {
            string insertCommandText = $"INSERT INTO {tableName} (";
            foreach (var prop in props)
            {
                insertCommandText += $"{prop}, ";
            }

            insertCommandText = insertCommandText.TrimEnd(',', ' ');
            insertCommandText += ") VALUES ( ";

            foreach (var prop in props)
            {
                insertCommandText += $"@{prop}, ";
            }

            insertCommandText = insertCommandText.TrimEnd(',', ' ');
            insertCommandText += $"; SELECT Id FROM {tableName} WHERE Id = @@IDENTITY";
            insertCommand = insertCommandText;

        }
        private void SetDeleteCommandText(IEnumerable<string> props)
        {

            string deleteCommandText = $"DELETE FROM {tableName} WHERE Id= @Id";
            deleteCommand = deleteCommandText;
        }
        private void SetTableName()
        {
            Type type = this.GetType();
            var tableAttribute = (type.GetGenericArguments()?.FirstOrDefault()
                ?.GetCustomAttributes(typeof(TableAttribute), false) as TableAttribute[])
                ?.FirstOrDefault();

            tableName = tableAttribute == null ? type.GetGenericArguments().First().Name : tableAttribute.TableName;
        }
        private List<string> GetDbFiledsNames()
        {
            string selectFieldsNamesCommandText = $"SELECT INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME= '{tableName}' AND COLUMN_NAME<>'Id'";
            List<string> columnNames = new List<string>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(selectFieldsNamesCommandText, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    columnNames.Add(reader["COLUMN_NAME"] as string);
                }
                reader.Close();
            }
            return columnNames;
        }
        private string[] GetPropertiesNames()
        {
            Type entityType = this.GetType().GetGenericArguments().FirstOrDefault();
            PropertyInfo[] properties = entityType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);

            string[] propertyNames = new string[properties.Length];

            // Проходим по каждому свойству сущности. Если к нему применен атрибут ColumnName, берем имя из атрибута, иначе - само имя свойства
            for (int i = 0; i < properties.Length; i++)
            {
                propertyNames[i] = properties[i].GetCustomAttributes<ColumnAttribute>()?.FirstOrDefault()?.ColumnName;
                if (propertyNames[i] == null)
                {
                    propertyNames[i] = properties[i].Name;
                }
            }

            return propertyNames;
        }
        #endregion

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

