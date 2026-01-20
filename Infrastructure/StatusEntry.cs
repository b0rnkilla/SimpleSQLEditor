namespace SimpleSQLEditor.Infrastructure
{
    public sealed class StatusEntry
    {
        public DateTime Timestamp { get; init; }
        public StatusLevel Level { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}