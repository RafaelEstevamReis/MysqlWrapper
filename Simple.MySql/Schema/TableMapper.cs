using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Simple.DatabaseWrapper.Interfaces;
using Simple.DatabaseWrapper.TypeReader;

namespace Simple.MySql.Schema
{
    class TableMapper : IColumnMapper
    {
        private readonly MysqlDB db;
        private readonly ReaderCachedCollection cachedTypes;
        private readonly List<Table> tables;

        public TableMapper(MysqlDB mysqlDB, ReaderCachedCollection typeCollection)
        {
            db = mysqlDB;
            cachedTypes = typeCollection;
            tables = new List<Table>();
        }

        public IColumnMapper Add<T>() where T : new()
        {
            var info = cachedTypes.GetInfo<T>();
            tables.Add(Table.FromType(info));
            return this;
        }
        public ITableMapper ConfigureTable(Action<ITable> Options)
        {
#if NETSTANDARD || NET45
            Options(tables.Last());
#else
            Options(tables[^1]);
#endif
            return this;
        }

        public ITableCommitResult[] Commit()
        {
            var results = new List<TableCommitResult>();

            foreach (var t in tables)
            {
                var tResult = commitTable(t);
                if (tResult != null) results.Add(tResult);
            }

            tables.Clear();
            return results.ToArray();
        }

        private TableCommitResult commitTable(Table t)
        {
            int val = db.ExecuteNonQuery(t.ExportCreateTable(), null);
            if (val == 0) // table created
            {
                return new TableCommitResult()
                {
                    TableName = t.TableName,
                    WasTableCreated = true,
                    ColumnsAdded = new string[0],
                };
            }

            if (val == -1) // Table not created
            {
                // migrate ?
                var dbColumns = db.GetTableSchema(t.TableName)
                                  .Rows.Cast<DataRow>()
                                  .Select(r => (string)r["ColumnName"]);

                var newColumns = t.Columns
                                  .Where(c => !dbColumns.Contains(c.ColumnName))
                                  .ToArray();

                foreach (var c in newColumns)
                {
                    string addColumn = c.ExportAddColumnAsStatement();
                    db.ExecuteNonQuery($"ALTER TABLE {t.TableName} {addColumn}", null);
                }

                if (newColumns.Length > 0)
                {
                    return new TableCommitResult()
                    {
                        TableName = t.TableName,
                        WasTableCreated = false,
                        ColumnsAdded = newColumns.Select(o => o.ColumnName)
                                                   .ToArray(),
                    };
                }
            }
            return null;
        }

    }
}
