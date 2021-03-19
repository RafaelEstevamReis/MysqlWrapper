using Simple.DatabaseWrapper.Interfaces;

namespace Simple.MySql.Schema
{
    public class TableCommitResult : ITableCommitResult
    {
        public string TableName { get; set; }
        public bool WasTableCreated { get; set; }
        public string[] ColumnsAdded { get; set; }
    }
}
