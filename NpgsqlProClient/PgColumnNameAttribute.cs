using System;

namespace NpgsqlProClient
{
    public class PgColumnNameAttribute : Attribute
    {
        public PgColumnNameAttribute(string colName)
        {
            ColumnName = colName;
        }

        public string ColumnName;
    }
}
