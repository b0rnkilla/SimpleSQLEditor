namespace SimpleSQLEditor.Services.EfCore
{
    public interface IEfRuntimeContextFactory
    {
        EfDbContext Create(string connectionString);
    }
}