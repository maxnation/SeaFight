using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace CustomORM
{
    public class Repository<T> : IRepository<T>
    {
        private readonly string connectionString;
        private string tableName;
        private string updateCommandText;
        private string deleteCommandText;
        private string insertCommandText;
        private string selectCommandText;
        private IEnumerable<string> tableColumnNames;
        private IEnumerable<RelationData> tableRelations;
        private bool isInheritedEntity;

        private Repository(string connectionString)
        {
            this.connectionString = connectionString;
            this.Initialize();
        }

        #region Initialization
        private void Initialize()
        {
            this.isInheritedEntity = this.IsInheritedEntity(typeof(T));
            this.SetTableName();
            this.SetTableRelations();
            this.SetCommands();
        }

        private void SetCommands()
        {
            this.tableColumnNames = this.GetTableColumnsNames(); // All columns of the table
            var currentEntityColumnsOnly = this.tableColumnNames.Intersect(this.GetPropertiesNames()); // [All columns] minus [Parent columns]
            var columnsToSet = currentEntityColumnsOnly.Except(new string[] { "Id", "Discriminator" });

            this.SetUpdateCommandText(columnsToSet);
            this.SetInsertCommandText(columnsToSet);
            this.SetDeleteCommandText();
            this.SetSelectCommandText();
        }

        private void SetUpdateCommandText(IEnumerable<string> props)
        {
            string updateCommandText = $"UPDATE [{this.tableName}] SET ";
            foreach (var prop in props)
            {
                updateCommandText += $"{prop} = @{prop}, ";
            }

            updateCommandText = updateCommandText.TrimEnd(',', ' ');
            updateCommandText += " WHERE Id = @Id";
            this.updateCommandText = updateCommandText;
        }

        private void SetInsertCommandText(IEnumerable<string> props)
        {
            string insertCommandText = $"INSERT INTO [{this.tableName}] (";
            foreach (var prop in props)
            {
                insertCommandText += $"{prop}, ";
            }

            if (this.isInheritedEntity)
            {
                insertCommandText += "Discriminator";
            }

            insertCommandText = insertCommandText.TrimEnd(',', ' ');
            insertCommandText += ") VALUES ( ";

            foreach (var prop in props)
            {
                insertCommandText += $"@{prop}, ";
            }

            if (this.isInheritedEntity)
            {
                insertCommandText += $"'{typeof(T).Name}'";
            }

            insertCommandText = insertCommandText.TrimEnd(',', ' ');
            insertCommandText += $"); SELECT Id FROM [{this.tableName}] WHERE Id = @@IDENTITY";
            this.insertCommandText = insertCommandText;
        }

        private void SetDeleteCommandText()
        {
            string deleteCommandText = $"DELETE FROM [{this.tableName}] WHERE Id= @Id";
            this.deleteCommandText = deleteCommandText;
        }

        private void SetSelectCommandText()
        {
            this.selectCommandText = $"SELECT * FROM [{this.tableName}]";

            if (this.isInheritedEntity)
            {
                this.selectCommandText += $" WHERE Discriminator = '{typeof(T).Name}'";
            }
        }

        private bool IsInheritedEntity(Type entityType)
        {
            return entityType.BaseType.FullName == typeof(object).FullName ? false : true;
        }

        private void SetTableName()
        {
            var type = typeof(T);
            if (this.isInheritedEntity)
            {
                type = type.BaseType;
            }

            var tableAttribute = type
               ?.GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault() as TableAttribute;

            this.tableName = tableAttribute == null ? type.Name : tableAttribute.TableName;
        }

        private IEnumerable<string> GetTableColumnsNames()
        {
            string selectFieldsNamesCommandText = $"SELECT INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME= '{this.tableName}'";
            List<string> columnNames = new List<string>();
            using (SqlConnection connection = new SqlConnection(this.connectionString))
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

        private IEnumerable<string> GetPropertiesNames()
        {
            Type entityType = this.GetType().GetGenericArguments().FirstOrDefault();
            PropertyInfo[] properties = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            string[] propertyNames = new string[properties.Length];

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

        private void SetCommandParameters(SqlCommand command, T entity)
        {
            foreach (var columnName in this.tableColumnNames)
            {
                var property = this.GetPropertyByColumnName(columnName, entity.GetType());
                if (property != null)
                {
                    object value = property.GetValue(entity);
                    command.Parameters.AddWithValue("@" + columnName, value);
                }
            }
        }

        private void SetTableRelations()
        {
            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                SqlCommand command = new SqlCommand("sp_getFkData", connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@tableName", this.tableName);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                List<RelationData> relations = new List<RelationData>();
                while (reader.Read())
                {
                    RelationData relationData = this.Map<RelationData>(reader);
                    relations.Add(relationData);
                }

                reader.Close();
                this.tableRelations = relations;
            }
        }
        #endregion

        #region CRUD
        public void Add(T entity)
        {
            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                SqlCommand command = new SqlCommand(this.insertCommandText, connection);
                this.SetCommandParameters(command, entity);

                connection.Open();
                object id = command.ExecuteScalar();

                PropertyInfo idProperty = this.GetPropertyByColumnName("Id", entity.GetType());
                idProperty.SetValue(entity, id);
            }
        }

        public void Delete(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException();
            }

            var idProperty = this.GetPropertyByColumnName("Id", entity.GetType());
            int id = (int)idProperty.GetValue(entity);

            this.Delete(id);
        }

        public void Delete(int id)
        {
            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                SqlCommand command = new SqlCommand(this.deleteCommandText, connection);
                command.Parameters.AddWithValue("Id", id);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public T Find(int id, bool loadRelatedData)
        {
            T entity = this.Find(id);

            if (loadRelatedData == true)
            {
                this.LoadRelatedData(entity);
            }

            return entity;
        }

        public T Find(int id)
        {
            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                SqlCommand command = new SqlCommand
                {
                    CommandText = this.isInheritedEntity ? this.selectCommandText + "AND Id = @Id" : this.selectCommandText + "WHERE Id = @Id",
                    Connection = connection
                };
                command.Parameters.AddWithValue("@Id", id);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                reader.Read();

                T entity = this.Map<T>(reader);
                reader.Close();
                return entity;
            }
        }

        public IEnumerable<T> GetAll(bool loadRelatedData = false)
        {
            var entities = this.GetAll();
            if (loadRelatedData == true)
            {
                this.LoadRelatedData(entities);
            }

            return entities;
        }

        public IEnumerable<T> GetAll()
        {
            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                SqlCommand command = new SqlCommand
                {
                    CommandText = this.selectCommandText,
                    Connection = connection
                };
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                List<T> entities = new List<T>();
                while (reader.Read())
                {
                    T entity = this.Map<T>(reader);
                    entities.Add(entity);
                }

                reader.Close();
                return entities;
            }
        }

        public void Update(T entity)
        {
            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                SqlCommand command = new SqlCommand(this.updateCommandText, connection);
                this.SetCommandParameters(command, entity);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        #endregion

        #region Mapping
        private T Map<T>(SqlDataReader reader)
        {
            // Check if the entity is of inherited type
            string discriminator = string.Empty;
            object entity = null;
            try
            {
                discriminator = (string)reader["Discriminator"];
            }
            catch (Exception)
            {
            }

            // If there is a discriminator presented, create entity not of a type T, but of the discriminator type
            if (!string.IsNullOrEmpty(discriminator))
            {
                string @namespace = typeof(T).Namespace;
                string assemblyName = typeof(T).Assembly.GetName().Name;
                string fullTypeName = $"{@namespace}.{discriminator}, {assemblyName}";
                Type t = Type.GetType(fullTypeName);
                entity = t.GetConstructor(new Type[] { }).Invoke(new Type[] { });
            }
            else
            {
                entity = typeof(T).GetConstructor(new Type[] { }).Invoke(new Type[] { });
            }

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i);
                var property = this.GetPropertyByColumnName(columnName, entity.GetType());
                property?.SetValue(entity, reader[i]);
            }

            return (T)entity;
        }

        private void Map<T>(SqlDataReader reader, ref T entity)
        {
            // Check if the entity is of inherited type
            string discriminator = string.Empty;
            try
            {
                discriminator = (string)reader["Discriminator"];
            }
            catch (Exception)
            {
            }

            // If there is a discriminator presented, create entity not of a type T, but of the discriminator type
            if (!string.IsNullOrEmpty(discriminator))
            {
                string @namespace = entity.GetType().Namespace;
                string assemblyName = entity.GetType().Assembly.GetName().Name;
                string fullTypeName = $"{@namespace}.{discriminator}, {assemblyName}";
                Type t = Type.GetType(fullTypeName);

                entity = (T)t.GetConstructor(new Type[] { }).Invoke(new Type[] { });
            }

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i);
                var property = this.GetPropertyByColumnName(columnName, entity.GetType());
                property?.SetValue(entity, reader[i]);
            }
        }
        #endregion

        public void LoadRelatedData(IEnumerable<T> entities)
        {
            foreach (RelationData relation in this.tableRelations)
            {
                if (this.tableName == relation.Parent_Table)
                {
                    this.SetChildsCollection(ref entities, relation);
                }
                else if (this.tableName == relation.Child_Table)
                {
                    this.SetParentProperty(ref entities, relation);
                }
            }
        }

        public void LoadRelatedData(T entity)
        {
            foreach (RelationData relation in this.tableRelations)
            {
                if (this.tableName == relation.Parent_Table)
                {
                    this.SetChildsCollection(ref entity, relation);
                }
                else if (this.tableName == relation.Child_Table)
                {
                    this.SetParentProperty(ref entity, relation);
                }
            }
        }

        private void SetParentProperty(ref IEnumerable<T> entities, RelationData relation)
        {
            PropertyInfo navigationalProperty = typeof(T).GetProperty(relation.Parent_Table);
            Type navPropertyType = navigationalProperty.PropertyType;

            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                SqlCommand command = connection.CreateCommand();
                command.CommandText = $"SELECT * FROM [{relation.Parent_Table}]";
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    IList collectionSub = this.CreateListOfType(navPropertyType) as IList;
                    while (reader.Read())
                    {
                        object propertyInstance = this.CreateInstanceFromType(navPropertyType);
                        this.Map(reader, ref propertyInstance);
                        collectionSub.Add(propertyInstance);
                    }

                    foreach (T entity in entities)
                    {
                        int fk = (int)typeof(T).GetProperty(relation.FK_ColumnName).GetValue(entity);
                        PropertyInfo foreignProp = typeof(T).GetProperty(relation.Parent_Table);

                        foreach (var parentEntity in collectionSub)
                        {
                            int pk = (int)this.GetPropertyByColumnName("Id", parentEntity.GetType()).GetValue(parentEntity);
                            if (fk == pk)
                            {
                                foreignProp.SetValue(entity, parentEntity);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void SetParentProperty(ref T entity, RelationData relation)
        {
            var id = this.GetPropertyByColumnName("Id", entity.GetType()).GetValue(entity);

            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                SqlCommand command = connection.CreateCommand();
                command.CommandText = $"SELECT * FROM [{relation.Parent_Table}] WHERE {relation.PK_ColumnName} = {id}";
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    PropertyInfo navigationalProperty = entity.GetType().GetProperty(relation.Parent_Table);
                    object propertyInstance = this.CreateInstanceFromType(navigationalProperty.PropertyType);
                    this.Map(reader, ref propertyInstance);
                    navigationalProperty.SetValue(entity, propertyInstance);
                }
            }
        }

        private void SetChildsCollection(ref IEnumerable<T> entities, RelationData relation)
        {
            var navigationalProps = this.GetNavigationalProperties(typeof(T)); // Navigational props of collection type

            PropertyInfo navigProperty = navigationalProps[relation.Child_Table];
            Type navPropGenericArgType = navigProperty.PropertyType.GenericTypeArguments[0];

            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                SqlCommand command = connection.CreateCommand();
                command.CommandText = $"SELECT * FROM [{relation.Child_Table}] WHERE {relation.FK_ColumnName} IS NOT NULL";

                connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    IList collection = this.CreateListOfType(navPropGenericArgType) as IList;
                    while (reader.Read())
                    {
                        object instance = this.CreateInstanceFromType(navPropGenericArgType);
                        this.Map(reader, ref instance);
                        collection.Add(instance);
                    }

                    foreach (var entity in entities)
                    {
                        int pk = (int)this.GetPropertyByColumnName("Id", entity.GetType()).GetValue(entity);

                        IList collectionNavig = this.CreateListOfType(navPropGenericArgType) as IList;

                        foreach (var item in collection)
                        {
                            int fk = (int)this.GetPropertyByColumnName(relation.FK_ColumnName, item.GetType()).GetValue(item);
                            if (fk == pk)
                            {
                                collectionNavig.Add(item);
                            }
                        }

                        navigProperty.SetValue(entity, collectionNavig);
                    }
                }
            }
        }

        private void SetChildsCollection(ref T entity, RelationData relation)
        {
            var id = this.GetPropertyByColumnName("Id", entity.GetType()).GetValue(entity);
            var navigationalProps = this.GetNavigationalProperties(entity.GetType()); // Navigational props of collection type

            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                SqlCommand command = connection.CreateCommand();
                command.CommandText = $"SELECT * FROM [{relation.Child_Table}] WHERE {relation.FK_ColumnName} = {id}";
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    PropertyInfo navigProperty = navigationalProps[relation.Child_Table];
                    Type navPropGenericArgType = navigProperty.PropertyType.GenericTypeArguments[0];

                    IList collection = this.CreateListOfType(navPropGenericArgType) as IList;
                    while (reader.Read())
                    {
                        object instance = this.CreateInstanceFromType(navPropGenericArgType);
                        this.Map(reader, ref instance);
                        collection.Add(instance);
                    }

                    navigProperty.SetValue(entity, collection);
                }
            }
        }

        private object CreateListOfType(Type type)
        {
            Type generic = typeof(List<>);
            Type[] typeArgs = { type };
            Type constructed = generic.MakeGenericType(typeArgs);
            object list = constructed.GetConstructor(new Type[] { }).Invoke(new object[] { });
            return list;
        }

        private object CreateInstanceFromType(Type type)
        {
            var propertyCtor = type.GetConstructor(
                                                   BindingFlags.Instance | BindingFlags.Public,
                                                   binder: null,
                                                   types: new Type[] { },
                                                   modifiers: null);

            object instance = propertyCtor.Invoke(new object[] { });
            return instance;
        }

        private PropertyInfo GetPropertyByColumnName(string columnnName, Type entityType)
        {
            PropertyInfo property = entityType.GetProperty(columnnName);
            if (property == null)
            {
                property = entityType
                      .GetProperties()
                      .FirstOrDefault(p => (p.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute)
                      ?.ColumnName == columnnName);
            }

            return property;
        }

        private Dictionary<string, PropertyInfo> GetNavigationalProperties(Type type)
        {
            var navigationalProps = type.GetProperties()
                    .Where(p => p.PropertyType.GetGenericArguments().Length == 1)
                    .ToDictionary(p => p.PropertyType.GetGenericArguments()[0].Name, p => p);

            return navigationalProps;
        }
    }
}