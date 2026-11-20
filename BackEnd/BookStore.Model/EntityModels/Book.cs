using BookStore.Model.Features;
using Microsoft.Data.SqlTypes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BookStore.Model.EntityModels
{
    /// <summary>
    /// Represents the primary Book entity within the domain.
    /// Implements multiple infrastructure interfaces to automatically handle auditing, 
    /// soft deletion, and multi-tenancy at the DbContext level via SaveChanges interceptors.
    /// </summary>
    // You can define indexes via Attributes (Data Annotations) or Fluent API.
    // Comment the below attribute if you want to use the Fluent API approach in ApplicationDbContext.
    //[Index(nameof(TenantId), nameof(IsDeleted), Name = "IX_Books_Tenant_Active")]
    //[Index(nameof(ISBN))]
    public partial class Book : IAuditable, ISoftDeletable, ITenantEntity, IHardDeletable
    {
        #region Example of the Lazy Loading without proxy classes, requires more boiler plate code but performant compared to proxy class based lazy loading.

        // Reference Implementation: Proxy-free Lazy Loading
        // This approach injects the ILazyLoader directly into the entity. 
        // It avoids the performance overhead of dynamic proxy generation but requires explicit property getters/setters.

        //private readonly ILazyLoader _loader;

        // 1. Standard constructor for manual creation
        //public Book() { }

        //// 2. EF Core constructor injection
        //// EF Core will automatically inject the ILazyLoader when it materializes the entity.
        //private Book(ILazyLoader loader)
        //{
        //    _loader = loader;
        //}

        //#region Navigations with Lazy Loading

        //private Publisher? _publisher;
        //public Publisher? Publisher
        //{
        //    get => _loader.Load(this, ref _publisher); // Triggers the DB call on first access
        //    set => _publisher = value;
        //}

        //private BookDetails? _bookDetails;
        //public BookDetails? BookDetails
        //{
        //    get => _loader.Load(this, ref _bookDetails);
        //    set => _bookDetails = value;
        //}

        //#endregion

        #endregion

        #region Core Data Properties

        // Primary Key definition using Data Annotations.
        [Key]
        public int Id { get; set; }

        // Enforces a non-null database column and sets a strict schema limit.
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(32)]
        public string? ISBN { get; set; }

        public decimal? Price { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        // Reference Note: Native Vector Support in EF Core 10 / SQL Server
        // SqlVector<float> is the heavily optimized, strongly-typed struct for SQL Server 2025.
        // ReadOnlyMemory<float> was the older, generic standard prior to native SQL provider support.
        // The DataType attribute specifies the exact storage dimension requirement in the database schema.

        // Standard .NET type for SQL Server vector columns in EF Core
        // Replace 768 with your local model's dimension (e.g., 384, 1536)
        [DataType("vector(768)")]
        [JsonIgnore]
        //public ReadOnlyMemory<float>? SearchVector { get; set; }
        public SqlVector<float>? SearchVector { get; set; }

        #endregion

        #region EF Core Indexer

        // Internal dictionary to hold the dynamic column data
        private readonly Dictionary<string, object> _dynamicData = new Dictionary<string, object>();

        // DEMO NOTE: The C# Indexer
        // EF Core will detect this indexer and automatically inject/extract values 
        // for any configured 'IndexerProperties' during database operations.
        public object this[string key]
        {
            get => _dynamicData.TryGetValue(key, out var value) ? value : null!;
            set => _dynamicData[key] = value;
        }

        #endregion

        #region Infrastructure & Audit (IAuditable, ISoftDeletable, ITenantEntity)

        // Audit properties populated automatically by ApplicationDbContext tracking.
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public int? CreatedBy { get; set; }

        // [NotMapped] ensures EF Core does not attempt to create a physical column in the SQL table.
        // It is used purely in-memory to signal the DbContext that a true SQL DELETE is required.
        [NotMapped] // CRITICAL: Tells EF Core this isn't a table column
        public bool IsHardDeleteRequested { get; set; }

        // Soft delete and multi-tenancy properties mapped to Global Query Filters.
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid TenantId { get; set; }

        #endregion

        #region Navigation Properties (Relationships)

        // Reference Note: The 'virtual' keyword allows EF Core to override the property to support Lazy-Loading Proxies if the package is enabled in the DbContext.
        // 1. One-to-Many: Publisher (1) -> Book (M)
        [ForeignKey("Publisher")]
        public int? PublisherId { get; set; }
        public virtual Publisher? Publisher { get; set; }

        // 2. One-to-One: Book (1) -> BookDetails (1)
        public virtual BookDetails? BookDetails { get; set; }

        // 3. Many-to-Many: Book <-x-> Author
        // You can toggle between automatic M:M and explicit Join Entities.
        // [JsonIgnore] prevents circular reference serialization exceptions if returning entities directly from an API.

        [JsonIgnore]
        public virtual List<Author> Authors { get; set; } = [];

        // Explicit Join Entity navigation for payload-rich Many-to-Many mapping.
        public virtual List<BookAuthorMaps> BookAuthorMaps { get; set; } = [];

        #endregion
    }
}