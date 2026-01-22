using Microsoft.EntityFrameworkCore;

namespace SimpleSQLEditor.Services.EfCore
{
    public class EfRuntimeContextFactory : IEfRuntimeContextFactory
    {
        #region Methods & Events

        public EfDbContext Create(string connectionString)
        {
            var options = new DbContextOptionsBuilder<EfDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new EfDbContext(options);
        }

        #endregion
    }
}