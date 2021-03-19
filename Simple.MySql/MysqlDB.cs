using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using Simple.DatabaseWrapper.Helpers;
using Simple.DatabaseWrapper.Interfaces;
using Simple.DatabaseWrapper.TypeReader;

namespace Simple.MySql
{
    public class MysqlDB
    {
        private readonly string cnnString;
        private readonly ReaderCachedCollection typeCollection;

        public MysqlDB(string host, string database, string user, string password)
        {
            MySqlConnectionStringBuilder stringBuilder = new MySqlConnectionStringBuilder();

            stringBuilder.Server = host;
            stringBuilder.Database = database;
            stringBuilder.UserID = user;
            stringBuilder.Password = password;

            cnnString = stringBuilder.ToString();
            typeCollection = new ReaderCachedCollection();
        }
        public MysqlDB(MySqlConnectionStringBuilder stringBuilder)
        {
            cnnString = stringBuilder.ToString();
            typeCollection = new ReaderCachedCollection();
        }

        private MySqlConnection getConnection()
        {
            var cnn = new MySqlConnection(cnnString);
            cnn.Open();
            return cnn;
        }
        /// <summary>
        /// Builds the table creation sequence, should be finished with Commit()
        /// </summary>
        public ITableMapper CreateTables()
        {
            return new Schema.TableMapper(this, typeCollection);
        }
        /// <summary>
        /// Get a list of all tables
        /// </summary>
        public string[] GetAllTables()
        {
            var dt = ExecuteReader(@"SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;", null);
            return dt.Rows.Cast<DataRow>()
                          .Select(row => (string)row[0])
                          .ToArray();
        }
        /// <summary>
        /// Gets the schema for a table
        /// </summary>
        public DataTable GetTableSchema(string TableName)
        {
            using var cnn = getConnection();
            using var cmd = cnn.CreateCommand();

            cmd.CommandText = $"SELECT * FROM {TableName} LIMIT 0";

            var reader = cmd.ExecuteReader();

            return reader.GetSchemaTable();
        }
        /// <summary>
        /// Executes a NonQUery command, this method locks the execution
        /// </summary>
        public int ExecuteNonQuery(string Text, object Parameters = null)
        {
            using var cnn = getConnection();
            using var cmd = cnn.CreateCommand();

            cmd.CommandText = Text;
            fillParameters(cmd, Parameters);

            return cmd.ExecuteNonQuery();
        }

        private string[] getSchemaColumns(IDataReader reader)
        {
            return Enumerable.Range(0, reader.FieldCount)
                             .Select(idx => reader.GetName(idx))
                             .ToArray();
        }

        /// <summary>
        /// Executes a Scalar commands and return the value as T
        /// </summary>
        public T ExecuteScalar<T>(string Text, object Parameters)
        {
            using var cnn = getConnection();
            using var cmd = cnn.CreateCommand();

            cmd.CommandText = Text;
            fillParameters(cmd, Parameters);

            var obj = cmd.ExecuteScalar();

            // In SQLite DateTime is returned as STRING after aggregate operations
            if (typeof(T) == typeof(DateTime))
            {
                if (DateTime.TryParse(obj.ToString(), out DateTime dt))
                {
                    return (T)(object)dt;
                }
                return default;
            }

            return (T)Convert.ChangeType(obj, typeof(T));
        }
        /// <summary>
        /// Executes a query and returns as DataTable
        /// </summary>
        public DataTable ExecuteReader(string Text, object Parameters)
        {
            using var cnn = getConnection();
            using var cmd = cnn.CreateCommand();

            cmd.CommandText = Text;
            fillParameters(cmd, Parameters);

            DataTable dt = new DataTable();
            var da = new MySqlDataAdapter(cmd.CommandText, cnn);
            da.Fill(dt);
            return dt;
        }
        /// <summary>
        /// Executes a query and returns the value as a T collection
        /// </summary>
        public IEnumerable<T> ExecuteQuery<T>(string Text, object Parameters)
        {
            var typeT = typeof(T);
            using var cnn = getConnection();
            using var cmd = cnn.CreateCommand();

            cmd.CommandText = Text;
            fillParameters(cmd, Parameters);

            using var reader = cmd.ExecuteReader();

            if (!reader.HasRows) yield break;

            var colNames = getSchemaColumns(reader);
            while (reader.Read())
            {
                // build new
                if (typeT.CheckIfSimpleType())
                {
                    yield return (T)TypeMapper.ReadValue(reader, typeT, 0);
                }
                else
                {
                    yield return TypeMapper.MapObject<T>(colNames, reader, typeCollection);
                }
            }
        }

        /// <summary>
        /// Gets a single T with specified table KeyValue on KeyColumn
        /// </summary>
        public T Get<T>(object KeyValue) => Get<T>(null, KeyValue);
        /// <summary>
        /// Gets a single T with specified table KeyValue on KeyColumn
        /// </summary>
        public T Get<T>(string KeyColumn, object KeyValue)
        {
            var info = typeCollection.GetInfo<T>();

            string keyColumn = KeyColumn
                            ?? info.Items.Where(o => o.Is(DatabaseWrapper.ColumnAttributes.PrimaryKey))
                                   .Select(o => o.Name)
                                   .FirstOrDefault()
                            ?? "_rowid_";

            return ExecuteQuery<T>($"SELECT * FROM {info.TypeName} WHERE {keyColumn} = @KeyValue ", new { KeyValue })
                    .FirstOrDefault();
        }
        /// <summary>
        /// Queries the database to all T rows in the table
        /// </summary>
        public IEnumerable<T> GetAll<T>() => ExecuteQuery<T>($"SELECT * FROM {typeof(T).Name} ", null);

        /// <summary>
        /// Queries the database to all T rows in the table with specified table KeyValue on KeyColumn
        /// </summary>
        public IEnumerable<T> GetAllWhere<T>(string FilterColumn, object FilterValue)
        {
            if (FilterColumn is null) throw new ArgumentNullException(nameof(FilterColumn));

            return ExecuteQuery<T>($"SELECT * FROM {typeof(T).Name} WHERE {FilterColumn} = @FilterValue ", new { FilterValue });
        }

        private void fillParameters(MySqlCommand cmd, object Parameters, TypeInfo type = null)
        {
            if (Parameters == null) return;

            if (type == null) type = typeCollection.GetInfo(Parameters.GetType());

            foreach (var p in type.Items)
            {
                var value = TypeHelper.ReadParamValue(p, Parameters);
                adjustInsertValue(ref value, p, Parameters);

                cmd.Parameters.AddWithValue(p.Name, value);
            }
        }
        private void adjustInsertValue(ref object value, TypeItemInfo p, object parameters)
        {
            if (!p.Is(DatabaseWrapper.ColumnAttributes.PrimaryKey)) return;

            if (p.Type == typeof(int) || p.Type == typeof(long))
            {
                if (!value.Equals(0)) return;
                // PK ints are AI
                value = null;
            }
            else if (p.Type == typeof(Guid))
            {
                if (!value.Equals(Guid.Empty)) return;

                value = Guid.NewGuid();
                // write new guid on object
                p.SetValue(parameters, value);
            }
        }
    }
}
