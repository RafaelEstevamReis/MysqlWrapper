using System;
using Simple.MySql.Schema;

namespace Simple.MySql.Attributes
{
    public class DataTypeAttribute : Attribute
    {
        public MySqlDbType Type { get; }
        public int Len { get; }
        public bool Unsigned { get; }

        public DataTypeAttribute(MySqlDbType type, int len = 0, bool unsigned = false)
        {
            Type = type;
            Len = len;
            Unsigned = unsigned;
        }
    }
}
