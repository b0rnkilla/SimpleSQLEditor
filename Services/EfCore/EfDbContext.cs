using Microsoft.EntityFrameworkCore;

namespace SimpleSQLEditor.Services.EfCore
{
    public class EfDbContext : DbContext
    {
        #region Fields

        private readonly TrackingModelDescriptor? _trackingDescriptor;

        #endregion

        #region Constructor

        public EfDbContext(DbContextOptions<EfDbContext> options)
            : base(options)
        {
        }

        public EfDbContext(DbContextOptions<EfDbContext> options, TrackingModelDescriptor trackingDescriptor)
            : base(options)
        {
            _trackingDescriptor = trackingDescriptor;
        }

        #endregion

        #region Properties

        public DbSet<DatabaseNameRow> DatabaseNames => Set<DatabaseNameRow>();

        public string? TrackingCacheKey =>
            _trackingDescriptor is null
                ? null
                : $"{_trackingDescriptor.EntityName}|{_trackingDescriptor.TableName}|{_trackingDescriptor.PrimaryKeyColumn}|{string.Join("|", _trackingDescriptor.ColumnTypes.OrderBy(x => x.Key).Select(x => $"{x.Key}:{x.Value.FullName}"))}";

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

            if (_trackingDescriptor is not null)
            {
                var entity = modelBuilder.SharedTypeEntity<Dictionary<string, object>>(_trackingDescriptor.EntityName);

                entity.ToTable(_trackingDescriptor.TableName, schema: "dbo");

                foreach (var kvp in _trackingDescriptor.ColumnTypes)
                {
                    var columnName = kvp.Key;
                    var clrType = kvp.Value;

                    entity.IndexerProperty(clrType, columnName).HasColumnName(columnName);
                }

                entity.HasKey(_trackingDescriptor.PrimaryKeyColumn);
            }

            base.OnModelCreating(modelBuilder);
        }

        #endregion
    }

    #region Supporting Types

    public class DatabaseNameRow
    {
        public string Name { get; set; } = string.Empty;
    }

    public sealed class TrackingModelDescriptor
    {
        public required string EntityName { get; init; }

        public required string TableName { get; init; }

        public required string PrimaryKeyColumn { get; init; }

        public required IReadOnlyDictionary<string, Type> ColumnTypes { get; init; }
    }

    #endregion
}