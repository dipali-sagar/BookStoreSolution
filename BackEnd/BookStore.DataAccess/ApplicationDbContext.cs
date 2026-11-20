using BookStore.DataAccess.Configuration;
using BookStore.DataAccess.Utilities;
using BookStore.Model.EntityModels;
using BookStore.Model.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Reflection;

namespace BookStore.DataAccess
{
    /// <summary>
    /// The core Entity Framework Core context. 
    /// Highlights advanced Enterprise patterns including Multi-Tenancy, Automatic Auditing, Soft Deletion, 
    /// Dynamic Query Filters via Expression Trees, and Dynamic Indexing.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public Guid? _currentTenantId;

        #region DbSets (Tables)

        public DbSet<Product> Products { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookDetails> BookDetails { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<BookAuthorMaps> BookAuthorMaps { get; set; }

        #endregion

        /// <summary>
        /// Constructor injection. ITenantService is injected to capture the current user's tenant context 
        /// at the exact moment the DbContext is scoped to the HTTP request.
        /// </summary>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantService? tenantService) : base(options)
        {
            _currentTenantId = tenantService?.GetCurrentTenantId();
        }

        #region Demo: OnConfiguring & Dynamic Connection Strings

        // DEMO NOTE: This commented section is perfect for showing how to dynamically route 
        // tenants to completely separate physical databases at runtime based on their TenantId.

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    // Logs SQL queries to the console
        //    optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);

        //    // You can also add this to see parameter values (Warning: Security risk in production!)
        //    optionsBuilder.EnableSensitiveDataLogging();

        //    // Build the tenant specific connection string in case if you don't want to use the same database for all the tenants. 
        //    // This method is called every time a DbContext is initialized.
        //    // We use it here to override the default connection string based on the tenant.
        //    if (_currentTenantId.HasValue)
        //    {
        //        // Logic to resolve the connection string.
        //        // In a real app, this might come from a 'Tenants' lookup table or a dictionary.
        //        string tenantConnectionString = ResolveConnectionString(_currentTenantId.Value);

        //        if (!string.IsNullOrEmpty(tenantConnectionString))
        //        {
        //            optionsBuilder.UseSqlServer(tenantConnectionString);
        //        }
        //    }

        //    base.OnConfiguring(optionsBuilder);
        //}

        //private string ResolveConnectionString(Guid tenantId)
        //{
        //    // Demo Simulation:
        //    // Amazon Tenant -> DB_Amazon
        //    // Crossword Tenant -> DB_Crossword

        //    return tenantId.ToString().ToUpperInvariant() switch
        //    {
        //        "226DC44F-5A83-4D39-901E-87F7A790197B" => "Server=(LocalDb)\\MSSqlLocalDb;Database=BookStore_Amazon;TrustServerCertificate=true;Trusted_Connection=true;",
        //        "864D14E2-615C-4D8F-85B6-24B521DD4BD9" => "Server=(LocalDb)\\MSSqlLocalDb;Database=BookStore_Crossword;TrustServerCertificate=true;Trusted_Connection=true;",
        //        _ => null // Fallback to default
        //    };
        //}

        #endregion

        #region Override SaveChanges (Interceptors / State Management)

        //1. Override Synchronous SaveChanges
        public override int SaveChanges()
        {
            HandleAuditSoftDeleteAndMulti_Tenancy();
            return base.SaveChanges();
        }

        // 2. Override Asynchronous SaveChanges
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            HandleAuditSoftDeleteAndMulti_Tenancy();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Centralized state management logic. Rather than setting CreatedOn, UpdatedOn, and IsDeleted 
        /// manually across dozens of services, we intercept the EF Core Change Tracker right before the SQL is generated.
        /// </summary>
        private void HandleAuditSoftDeleteAndMulti_Tenancy()
        {
            var now = DateTime.UtcNow;

            // 1. Handle Soft Delete Interception
            // We find entries marked as 'Deleted' and switch them to 'Modified'
            var deletedEntries = ChangeTracker.Entries<ISoftDeletable>()
                .Where(e => e.State == EntityState.Deleted);

            foreach (var entry in deletedEntries)
            {
                // Check if we have explicitly requested a Hard Delete
                // This could be via a custom state or a simple check on a local [NotMapped] property
                if (entry.Entity is IHardDeletable { IsHardDeleteRequested: true })
                {
                    // Do nothing, let the state remain 'Deleted' so EF generates a DELETE statement
                    continue;
                }

                // Stop the hard delete by changing the tracker state
                entry.State = EntityState.Modified;

                // Set the soft delete flags
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedOn = now;
            }

            // 2. Handle Auditing (CreatedOn / UpdatedOn)
            // Note: Because we changed the state of soft-deleted items to 'Modified' above,
            // they will effectively be caught here and have their UpdatedOn updated as well.
            var auditableEntries = ChangeTracker.Entries<IAuditable>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in auditableEntries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedOn = now;
                }

                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedOn = now;
                }
            }

            // 3. Handle Multi-Tenancy automatically on Insert
            var tenantEntities = ChangeTracker.Entries<ITenantEntity>()
                .Where(e => e.State == EntityState.Added);

            foreach (var entry in tenantEntities)
            {
                // Ensures data cannot be orphaned without a tenant
                entry.Entity.TenantId = _currentTenantId ?? Guid.NewGuid();
            }
        }

        #endregion

        #region OnModelCreating & Configurations

        /// <summary>
        /// OnModelCreating does not run every time a DbContext object is created. In a production environment, EF Core is designed for high performance. The model-building process is computationally expensive, so EF Core uses a Model Cache.
        /// 
        /// How it works:
        ///     First Request: When the first instance of your DbContext is initialized after the application starts, EF Core runs OnModelCreating to build the internal "In-Memory Model."
        ///     Caching: EF Core caches this compiled model using the type of your DbContext as the key.
        ///     Subsequent Requests: For every new ApplicationDbContext() thereafter, EF Core pulls the model from the cache and skips OnModelCreating entirely.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Postgres specific config
            // Instructs PostgreSQL to ensure the pgvector extension is installed
            //modelBuilder.HasPostgresExtension("vector");
            #endregion

            // --- Set Index for the entities having Tenant specific entries ---
            SetTenantIndexForEntities(modelBuilder);

            // --- Entity Configuration ---
            // Option 1: Entity specific configuration file for e.g. 
            //modelBuilder.ApplyConfiguration(new ProductEntityConfiguration());
            //modelBuilder.ApplyConfiguration(new BookEntityConfiguration());
            //modelBuilder.ApplyConfiguration(new BookAuthorMapsEntityConfiguration());

            // Option 2: Global entity configuration. It will load all entity configurations from this assembly. 
            // Highly useful for enterprise apps with hundreds of entity configurations.
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // --- Initial Seed Data ---
            modelBuilder.SeedData();

            // --- Global Named query filter ---
            // Option 1: Entity specific Global Filter
            //modelBuilder.Entity<Book>()
            //    .HasQueryFilter(FilterNames.SoftDelete, x => !x.IsDeleted)
            //    .HasQueryFilter(FilterNames.Tenant, x => x.TenantId == _currentTenantId);

            // Options 2: Truly global filter dynamically applied via Interfaces and Reflection
            SetGlobalQueryFilter(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        #endregion

        #region Dynamic Global Query Filters

        /// <summary>
        /// Iterates through every entity tracked by EF Core. If it implements our interfaces, 
        /// we attach the global query filters dynamically, ensuring we never forget to filter a table.
        /// </summary>
        private void SetGlobalQueryFilter(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;

                // Apply Soft Delete Filter dynamically
                if (typeof(ISoftDeletable).IsAssignableFrom(clrType))
                {
                    modelBuilder.Entity(clrType).HasQueryFilter(
                        FilterNames.SoftDelete,
                        ConvertFilter<ISoftDeletable>(e => !e.IsDeleted, clrType)
                    );
                }

                // Apply Tenant Filter dynamically
                if (typeof(ITenantEntity).IsAssignableFrom(clrType))
                {
                    modelBuilder.Entity(clrType).HasQueryFilter(
                        FilterNames.Tenant,
                        ConvertFilter<ITenantEntity>(e => e.TenantId == this._currentTenantId, clrType)
                    );
                }
            }
        }

        /// <summary>
        /// DEMO NOTE: Expression Tree Manipulation. 
        /// EF Core requires Lambda Expressions targeted directly at the concrete entity type (e.g., 'Book').
        /// Because we are iterating via an interface ('ISoftDeletable'), we use an ExpressionVisitor to 
        /// rewrite the lambda parameter from the interface type to the actual concrete class type at runtime.
        /// </summary>
        private static LambdaExpression ConvertFilter<TInterface>(Expression<Func<TInterface, bool>> filterExpression, Type entityType)
        {
            var parameter = Expression.Parameter(entityType);
            var body = ReplacingExpressionVisitor.Replace(filterExpression.Parameters[0], parameter, filterExpression.Body);
            return Expression.Lambda(body, parameter);
        }

        #endregion

        #region Dynamic Indexing

        /// <summary>
        /// Dynamically builds indexes for Multi-Tenant architecture so queries don't hit full table scans.
        /// </summary>
        private void SetTenantIndexForEntities(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Automatically index TenantId for all ITenantEntity implementations
                if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .HasIndex(nameof(ITenantEntity.TenantId))
                        .HasDatabaseName($"IX_{entityType.GetTableName()}_TenantId")
                        // DEMO NOTE: Filtered index. Saves disk space and speeds up inserts by only indexing 
                        // rows that are NOT in the soft-deleted recycle bin.
                        .HasFilter("[IsDeleted] = 0");
                }
            }
        }

        #endregion
    }
}