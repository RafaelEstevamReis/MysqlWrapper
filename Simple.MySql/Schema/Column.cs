using System;
using System.Drawing;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using Simple.DatabaseWrapper.Attributes;
using Simple.DatabaseWrapper.Interfaces;
using Simple.DatabaseWrapper.TypeReader;

namespace Simple.MySql.Schema
{
    /// <summary>
    /// Class to map a column schema
    /// </summary>
    public class Column : IColumn
    {
        /// <summary>
        /// Column name
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// Type on SQLite database
        /// </summary>
        public MySqlDbType MySqlDbType { get; set; }
        /// <summary>
        /// Native object type
        /// </summary>
        public Type NativeType { get; set; }
        /// <summary>
        /// Is PrimaryKey ?
        /// </summary>
        public bool IsPK { get; set; }
        /// <summary>
        /// Is Auto-Increment ?
        /// </summary>
        public bool IsAI { get; set; }
        /// <summary>
        /// Is Unique indexed ?
        /// </summary>
        public bool IsUnique { get; set; }
        /// <summary>
        /// Allow null values ?
        /// </summary>
        public bool AllowNulls { get; set; }
        /// <summary>
        /// Default value on NULL
        /// </summary>
        public object DefaultValue { get; set; }
        public bool Unsigned { get; set; }
        public int Length { get; set; }

        /// <summary>
        /// Create a column schema from TypeInfoItem
        /// </summary>
        public static IColumn FromInfo(TypeInfo info, TypeItemInfo pi)
        {
            var column = createColumnWithType(pi);

            //Props
            column.IsPK = pi.Is(DatabaseWrapper.ColumnAttributes.PrimaryKey);
            // Is AI ?
            if (column.IsPK)
            {
                column.IsAI = (column.MySqlDbType == MySqlDbType.Int
                             || column.MySqlDbType == MySqlDbType.TinyInt
                             || column.MySqlDbType == MySqlDbType.SmallInt
                             || column.MySqlDbType == MySqlDbType.MediumInt
                             || column.MySqlDbType == MySqlDbType.BigInt);
            }

            column.IsUnique = pi.Is(DatabaseWrapper.ColumnAttributes.Unique);
            column.AllowNulls = checkAllowNulls(pi, column);

            column.DefaultValue = null;
            foreach (var attr in pi.DBAttributes)
            {
                if (attr.Attribute is DefaultValueAttribute def)
                {
                    column.DefaultValue = def;
                    break;
                }
            }

            return column;
        }

        private static bool checkAllowNulls(TypeItemInfo pi, Column column)
        {
            // was specified ?
            if (pi.Is(DatabaseWrapper.ColumnAttributes.AllowNull)) return true;
            if (pi.Is(DatabaseWrapper.ColumnAttributes.NotNull)) return false;

            // Auto select
            switch (column.MySqlDbType)
            {
                case MySqlDbType.Text:
                case MySqlDbType.TinyText:
                case MySqlDbType.MediumText:
                case MySqlDbType.LongText:

                case MySqlDbType.Blob:
                case MySqlDbType.TinyBlob:
                case MySqlDbType.MediumBlob:
                case MySqlDbType.LongBlob:
                    return true;
            }
            // default
            return false;
        }

        private static Column createColumnWithType(TypeItemInfo info)
        {
            MySqlDbType dataType;
            bool unsigned = false;
            // Texts
            if (info.Type == typeof(string)) dataType = MySqlDbType.Text;
            else if (info.Type == typeof(Uri)) dataType = MySqlDbType.Text;
            // Float point Numbers
            else if (info.Type == typeof(float)) dataType = MySqlDbType.Float;
            else if (info.Type == typeof(double)) dataType = MySqlDbType.Double;
            // Fixed FloatPoint
            else if (info.Type == typeof(decimal)) dataType = MySqlDbType.Decimal;
            // Integers
            else if (info.Type == typeof(byte))
            {
                dataType = MySqlDbType.TinyInt;
                unsigned = true;
            }
            else if (info.Type == typeof(int)) dataType = MySqlDbType.Int;
            else if (info.Type == typeof(uint))
            {
                dataType = MySqlDbType.Int;
                unsigned = true;
            }
            else if (info.Type == typeof(long)) dataType = MySqlDbType.BigInt;
            else if (info.Type == typeof(ulong))
            {
                dataType = MySqlDbType.BigInt;
                unsigned = true;
            }
            // Others Mapped of NUMERIC
            else if (info.Type == typeof(bool)) dataType = MySqlDbType.Bool;
            else if (info.Type == typeof(DateTime)) dataType = MySqlDbType.DateTime;
            // Other
            else if (info.Type == typeof(Guid)) dataType = MySqlDbType.TinyBlob;
            else if (info.Type == typeof(Color)) dataType = MySqlDbType.TinyBlob;
            else if (info.Type == typeof(byte[])) dataType = MySqlDbType.LongBlob;
            //Int enums
            else if (info.Type.IsEnum) dataType = MySqlDbType.Int;
            else
            {
                throw new Exception($"Type {info.Type.Name} is not supported on field {info.Name}");
            }

            return new Column()
            {
                ColumnName = info.Name,
                MySqlDbType = dataType,
                Unsigned = unsigned,
            };
        }

        /// <summary>
        /// Creates a CREATE TABLE column statment from current schema
        /// </summary>
        public string ExportColumnDefinitionAsStatement()
        {
            if (string.IsNullOrEmpty(ColumnName)) throw new ArgumentNullException("ColumnName can not be null");
            if (ColumnName.Any(c => char.IsWhiteSpace(c))) throw new ArgumentNullException("ColumnName can not contain whitespaces");
            if (ColumnName.Any(c => char.IsSymbol(c))) throw new ArgumentNullException("ColumnName can not contain symbols");

            StringBuilder sb = new StringBuilder();

            sb.Append(ColumnName);
            sb.Append(" ");

            sb.Append(MySqlDbType.ToString());
            sb.Append(" ");

            if (IsPK) sb.Append("PRIMARY KEY ");
            if (IsAI) sb.Append("AUTOINCREMENT ");
            if (IsUnique) sb.Append("UNIQUE ");

            if (!AllowNulls) sb.Append("NOT NULL ");

            if (DefaultValue != null)
            {
                sb.Append($"DEFAULT '{DefaultValue}'");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates a ADD COLUMN from current schema. 
        /// This MAY change de [DefaultValue] when [NotNull] to Comply with Sqlite
        /// </summary>
        /// <returns></returns>
        public string ExportAddColumnAsStatement()
        {
            if (string.IsNullOrEmpty(ColumnName)) throw new ArgumentNullException("ColumnName can not be null");
            if (ColumnName.Any(c => char.IsWhiteSpace(c))) throw new ArgumentNullException("ColumnName can not contain whitespaces");
            if (ColumnName.Any(c => char.IsSymbol(c))) throw new ArgumentNullException("ColumnName can not contain symbols");

            StringBuilder sb = new StringBuilder();

            sb.Append(" ADD COLUMN ");

            sb.Append(ColumnName);
            sb.Append(" ");

            sb.Append(MySqlDbType.ToString());
            sb.Append(" ");

            if (IsAI) sb.Append("AUTOINCREMENT ");

            // Columns with PK cannot be added in Sqlite
            // Columns with UNIQUE cannot be added in Sqlite
            // Columns with [NOT NULL] MUST HAVE a [DEFAULT VALUE]
            if (!AllowNulls)
            {
                sb.Append("NOT NULL ");

                if (DefaultValue == null)
                {
                    setReasonableDefault(this);
                }
            }

            if (DefaultValue != null)
            {
                sb.Append($"DEFAULT '{DefaultValue}'");
            }

            return sb.ToString();
        }

        private void setReasonableDefault(Column column)
        {
            if (column.NativeType == typeof(DateTime))
            {
                column.DefaultValue = DateTime.MinValue;
                return;
            }

            switch (MySqlDbType)
            {
                case MySqlDbType.Float:
                case MySqlDbType.Double:
                case MySqlDbType.Decimal:
                case MySqlDbType.Int:
                case MySqlDbType.TinyInt:
                case MySqlDbType.SmallInt:
                case MySqlDbType.MediumInt:
                case MySqlDbType.BigInt:
                    column.DefaultValue = 0;
                    break;
                // NotNull text, is empty
                case MySqlDbType.Text:
                case MySqlDbType.LongText:
                case MySqlDbType.MediumText:
                case MySqlDbType.TinyText:
                    column.DefaultValue = string.Empty;
                    break;
            }
        }
    }
}
