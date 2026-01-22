using Microsoft.EntityFrameworkCore;

namespace SimpleSQLEditor.Services.EfCore
{
    public class EfDbContext : DbContext
    {
        #region Constructor

        public EfDbContext(DbContextOptions<EfDbContext> options)
            : base(options)
        {
        }

        #endregion

        #region Properties

        public DbSet<DatabaseNameRow> DatabaseNames => Set<DatabaseNameRow>();

        #endregion

        #region Methods & Events

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DatabaseNameRow>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null);
                entity.Property(x => x.Name).HasColumnName("name");
            });

            base.OnModelCreating(modelBuilder);
        }

        #endregion
    }
}