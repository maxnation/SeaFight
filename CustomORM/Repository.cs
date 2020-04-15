using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Reflection;
using System.Data;
using System.Collections;

namespace CustomORM
{
    public class Repository<T> : IRepository<T>
    {
        private string connectionString;
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
            isInheritedEntity = IsInheritedEntity(typeof(T));
            SetTableName();
            SetTableRelations();
            SetCommands();
        }
        private void SetCommands()
        {
            tableColumnNames = GetTableColumnsNames(); // All columns of the table          
            var currentEntityColumnsOnly = tableColumnNames.Intersect(GetPropertiesNames()); // [All columns] minus [Parent columns]
            var columnsToSet = currentEntityColumnsOnly.Except(new string[] { "Id", "Discriminator" });
            var columnsToGet =  tableColumnNames;

            SetUpdateCommandText(columnsToSet);
            SetInsertCommandText(columnsToSet);
            SetDeleteCommandText();
            SetSelectCommandText(columnsToGet);
        }
        private void SetUpdateCommandText(IEnumerable<string> props)
        {
            string updateCommandText = $"UPDATE [{tableName}] SET ";
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
            string insertCommandText = $"INSERT INTO [{tableName}] (";
            foreach (var prop in props)
            {
                insertCommandText += $"{prop}, ";
            }

            if (isInheritedEntity)
            {
                insertCommandText += "Discriminator";
            }
            insertCommandText = insertCommandText.TrimEnd(',', ' ');
            insertCommandText += ") VALUES ( ";

            foreach (var prop in props)
            {
                insertCommandText += $"@{prop}, ";
            }

            if (isInheritedEntity)
            {
                insertCommandText += $"'{typeof(T).Name}'";
            }

            insertCommandText = insertCommandText.TrimEnd(',', ' ');
            insertCommandText += $"); SELECT Id FROM [{tableName}] WHERE Id = @@IDENTITY";
            this.insertCommandText = insertCommandText;
        }
        private void SetDeleteCommandText()
        {
            string deleteCommandText = $"DELETE FROM [{tableName}] WHERE Id= @Id";
            this.deleteCommandText = deleteCommandText;
        }
        private void SetSelectCommandText(IEnumerable<string> props)
        {
            selectCommandText = $"SELECT * FROM [{tableName}]";

            if (isInheritedEntity)
            {
                selectCommandText += $" WHERE Discriminator = '{typeof(T).Name}'";
            }
        }
        private bool IsInheritedEntity(Type entityType)
        {
            return entityType.BaseType.FullName == typeof(object).FullName ? false : true;   
        }
        private void SetTableName()
        {
            var type = typeof(T);
            if (isInheritedEntity)
            {
                type = type.BaseType;
            }

             var tableAttribute = type
                ?.GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault() as TableAttribute;

            tableName = tableAttribute == null ? type.Name : tableAttribute.TableName;
        }

        private IEnumerable<string> GetTableColumnsNames()
        {
            string selectFieldsNamesCommandText = $"SELECT INFORMATION_SCHEMA.COLUMNS.COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME= '{tableName}'";
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
        private IEnumerable<string> GetPropertiesNames()
        {
            Type entityType = this.GetType().GetGenericArguments().FirstOrDefault();
            PropertyInfo[] properties = entityType.GetProperties( BindingFlags.Instance | BindingFlags.Public);
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
            foreach (var columnName in tableColumnNames)
            {
                var property = GetPropertyByColumnName(columnName, entity.GetType());
                if (property != null)
                {
                    object value = property.GetValue(entity);
                    command.Parameters.AddWithValue("@" + columnName, value);
                }               
            }
        }

        private void SetTableRelations()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand("sp_getFkData", connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@tableName", tableName);

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
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(insertCommandText, connection);
                SetCommandParameters(command, entity);

                connection.Open();
                object id = command.ExecuteScalar();

                PropertyInfo idProperty = GetPropertyByColumnName("Id", entity.GetType());
                idProperty.SetValue(entity, id);
            }
        }

        public void Delete(T entity)
        {
            if (entity == null) throw new ArgumentNullException();

            var idProperty = GetPropertyByColumnName("Id", entity.GetType());
            int id = (int)idProperty.GetValue(entity);

            Delete(id);
        }

        public void Delete(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(deleteCommandText, connection);
                command.Parameters.AddWithValue("Id", id);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public T Find(int id, bool loadRelatedData)
        {
            T entity = Find(id);

            if (loadRelatedData == true)
            {
                LoadRelatedData(entity);
            }
            return entity;
        }

        public T Find(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand();
                
                command.CommandText = isInheritedEntity ? selectCommandText + "AND Id = @Id" : selectCommandText + "WHERE Id = @Id"; 
                command.Connection = connection;
                command.Parameters.AddWithValue("@Id", id);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                reader.Read();

                T entity = Map<T>(reader);
                reader.Close();
                return entity;
            }
        }

        public IEnumerable<T> GetAll(bool loadRelatedData = false)
        {
            var entities = GetAll();
            if (loadRelatedData == true)
            {
                LoadRelatedData(entities);     
            }
            return entities;
        }

        public IEnumerable<T> GetAll()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand();
                command.CommandText = selectCommandText;
                command.Connection = connection;
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                List<T> entities = new List<T>();
                while (reader.Read())
                {
                    T entity = Map<T>(reader);
                    entities.Add(entity);
                }
                reader.Close();
                return entities;
            }
        }

        public void Update(T entity)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(updateCommandText, connection);
                SetCommandParameters(command, entity);
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
            object entity=null;
            try
            {
                discriminator = (string) reader["Discriminator"];
            }
            catch(Exception e){   }
           
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
            catch (Exception e){   }
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
                var property = GetPropertyByColumnName(columnName, entity.GetType());
                property?.SetValue(entity, reader[i]);
            }
        }     
        #endregion
       
        public void  LoadRelatedData(IEnumerable<T> entities)
        {
            // Получили  навигационные свойства коллекционного типа сущности
            var navigationalProps = GetNavigationalProperties(typeof(T));

            // Проходим по каждому отношению, в котором состоит сущность типа T
            foreach (RelationData relation in tableRelations)
            {
                if (this.tableName == relation.Parent_Table)
                {
                    var navigProperty = navigationalProps[relation.Child_Table];
                    Type navPropGenericArgType = navigProperty.PropertyType.GenericTypeArguments[0];

                    var propertyCtor = navPropGenericArgType.GetConstructor(
                                                    BindingFlags.Instance | BindingFlags.Public,
                                                    binder: null, types: new Type[] { }, modifiers: null);

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        SqlCommand command = new SqlCommand();
                        command.Connection = connection;
                        command.CommandText = $"SELECT * FROM [{relation.Child_Table}] WHERE {relation.FK_ColumnName} IS NOT NULL";
                        connection.Open();

                        Type generic = typeof(List<>);
                        Type[] typeArgs = { navPropGenericArgType };
                        Type constructed = generic.MakeGenericType(typeArgs);

                        IList collection = constructed.GetConstructor(new Type[] { }).Invoke(new object[] { }) as IList;

                        //Заполнение collection
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                object instance = propertyCtor.Invoke(new object[] { });
                                Map(reader, ref instance);
                                collection.Add(instance);
                            }                        
                        }
                   
                        foreach(var entity in entities)
                        {                                              
                            var primaryKeyProp = this.GetPropertyByColumnName(relation.PK_ColumnName, entity.GetType());
                            var pk = primaryKeyProp.GetValue(entity);

                            // Создаем коллекцию типа навигационного свойства
                            Type genericNavig = typeof(List<>);
                            Type[] typeArgsNavig = { navPropGenericArgType };
                            Type constructedNavig = generic.MakeGenericType(typeArgs);                       
                            IList collectionNavig = constructed.GetConstructor(new Type[] { }).Invoke(new object[] { }) as IList;

                            foreach (var item in collection)
                            {
                                int fk = (int)item.GetType().GetProperty(relation.FK_ColumnName).GetValue(item);

                                if (fk == (int)pk)
                                {
                                    collectionNavig.Add(item);
                                }
                            }
                            navigProperty.SetValue(entity, collectionNavig);
                        }
                    }
                }
                else if (this.tableName == relation.Child_Table)
                {
                    var navigationalProperty = typeof(T).GetProperty(relation.Parent_Table);

                    var propertyCtor = navigationalProperty.PropertyType.GetConstructor(
                                                    BindingFlags.Instance | BindingFlags.Public,
                                                    binder: null, types: new Type[] { }, modifiers: null);

                    Type genericSub = typeof(List<>);
                    Type[] typeArgsSub = { navigationalProperty.PropertyType };
                    Type constructedSub = genericSub.MakeGenericType(typeArgsSub);

                    // List<someEntity>
                    IList collectionSub = constructedSub.GetConstructor(new Type[] { }).Invoke(new object[] { }) as IList;

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        SqlCommand command = new SqlCommand();
                        command.Connection = connection;
                        command.CommandText = $"SELECT * FROM [{relation.Parent_Table}]";
                        connection.Open();

                        SqlDataReader reader = command.ExecuteReader();
                       
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                object propertyInstance = propertyCtor.Invoke(new object[] { });
                                Map(reader, ref propertyInstance);
                                collectionSub.Add(propertyInstance);
                            }
                            foreach (T entity in entities)
                            {
                                int fk = (int)typeof(T).GetProperty(relation.FK_ColumnName).GetValue(entity);
                                var foreignProp = typeof(T).GetProperty(relation.Parent_Table);

                                foreach (var parentEntity in collectionSub)
                                {
                                    var pkProp = GetPropertyByColumnName(relation.PK_ColumnName, parentEntity.GetType());
                                    int pk = (int)pkProp.GetValue(parentEntity);
                                    
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
            }
        }

        public void LoadRelatedData(T entity)
        {
            var idProperty = this.GetPropertyByColumnName("Id", entity.GetType());
            var id = idProperty.GetValue(entity);

            // Получили её навигационные свойства коллекционного типа
            var navigationalProps = this.GetNavigationalProperties(entity.GetType());

            // Проходим по каждому отношению, в котором состоит таблица ДАННОЙ сущность
            foreach (RelationData relation in tableRelations)
            {
                if (this.tableName == relation.Parent_Table)
                {
                    var navigProperty = navigationalProps[relation.Child_Table];
                    Type navPropGenericArgType = navigProperty.PropertyType.GenericTypeArguments[0];

                    var propertyCtor = navPropGenericArgType.GetConstructor(
                                                    BindingFlags.Instance | BindingFlags.Public,
                                                    binder: null, types: new Type[] { }, modifiers: null);
                   
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        SqlCommand command = new SqlCommand();
                        command.Connection = connection;
                        command.CommandText = $"SELECT * FROM [{relation.Child_Table}] WHERE {relation.FK_ColumnName} = @Id";
                        command.Parameters.AddWithValue("@Id", id);
                        connection.Open();

                        Type generic = typeof(List<>);
                        Type[] typeArgs = { navPropGenericArgType };
                        Type constructed = generic.MakeGenericType(typeArgs);
                        object collection = constructed.GetConstructor(new Type[] { }).Invoke(new object[] { });

                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                object instance = propertyCtor.Invoke(new object[] { });
                                Map(reader, ref instance);
                                constructed.GetMethod("Add").Invoke(collection, new object[] { instance });

                            }
                        }
                        navigProperty.SetValue(entity, collection);
                    }
                }
                else if (this.tableName == relation.Child_Table)
                {
                    var navigationalProperty = entity.GetType().GetProperty(relation.Parent_Table);

                    var propertyCtor = navigationalProperty.PropertyType.GetConstructor(
                                                    BindingFlags.Instance | BindingFlags.Public,
                                                    binder: null, types: new Type[] { }, modifiers: null);

                    object propertyInstance = propertyCtor.Invoke(new object[] { });

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        SqlCommand command = new SqlCommand();
                        command.Connection = connection;
                        command.CommandText = $"SELECT * FROM [{relation.Parent_Table}] WHERE {relation.PK_ColumnName} = @Id";
                        command.Parameters.AddWithValue("@Id", id);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        reader.Read();
                        if (reader.HasRows)
                        {
                            Map(reader, ref propertyInstance);
                        }
                        navigationalProperty.SetValue(entity, propertyInstance);
                    }
                }
            }
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