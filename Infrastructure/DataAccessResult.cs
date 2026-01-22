namespace SimpleSQLEditor.Infrastructure
{
    public sealed class DataAccessResult<T>
    {
        public required string Provider { get; init; }

        public required T Data { get; init; }
    }
}