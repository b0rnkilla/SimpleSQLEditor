namespace SimpleSQLEditor.Infrastructure
{
    public static class SqlDataTypes
    {
        public static readonly IReadOnlyList<string> Allowed =
        [
            /* Integer:   */ "int","bigint","smallint","tinyint",
            /* Decimal:   */ "decimal(18,8)","decimal(18,4)","decimal(18,2)","decimal(18,0)",
            /* Floating:  */ "float","real",
            /* Boolean:   */ "bit",
            /* Date/Time: */ "date","datetime","datetime2","smalldatetime","time",
            /* Guid:      */ "uniqueidentifier",
            /* Text:      */ "nvarchar(50)","nvarchar(100)","nvarchar(255)","nvarchar(500)","nvarchar(max)",
            /* Text:      */ "varchar(50)","varchar(100)","varchar(255)","varchar(500)","varchar(max)",
            /* Binary:    */ "varbinary(max)"
        ];
    }
}
