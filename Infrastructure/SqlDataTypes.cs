namespace EfPlayground.Infrastructure
{
    public static class SqlDataTypes
    {
        public static readonly IReadOnlyList<string> Allowed =
        [
            "int",
            "bit",
            "datetime",
            "nvarchar(50)",
            "nvarchar(100)"
        ];
    }
}
