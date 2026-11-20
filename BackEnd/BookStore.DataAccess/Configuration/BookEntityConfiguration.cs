using BookStore.Model.EntityModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace BookStore.DataAccess.Configuration
{
    /// <summary>
    /// Encapsulates the Fluent API configuration for the Book entity.
    /// This pattern keeps the ApplicationDbContext clean and enforces separation of concerns 
    /// (Database Schema constraints vs. C# Domain logic).
    /// </summary>
    internal class BookEntityConfiguration : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> builder)
        {
            #region 1. Table & Primary Key Mapping

            // Explicitly map to the pluralized table name
            builder.ToTable($"{nameof(Book)}s");

            // DEMO NOTE: Fluent API way to declare the Primary key. 
            // This overrides EF Core's default convention (which looks for 'Id' or 'BookId').
            // You can also use the [Key] Data Annotation attribute on the entity class.
            builder.HasKey(nameof(Book.Id));

            #endregion

            #region 2. Column Configurations & Constraints

            builder.Property(u => u.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.ISBN)
                .HasMaxLength(32);

            // Create a Unique Constraint in the database to prevent duplicate ISBNs
            builder.HasIndex(u => u.ISBN)
                .IsUnique();

            // Specifying precise database types to avoid truncation or rounding errors
            builder.Property(x => x.Price)
                .HasColumnType("decimal(18, 4)");

            // PostgreSQL Specific config
            //builder.Property(x => x.Price)
                //.HasColumnType("numeric(18, 4)"); // 'numeric' and 'decimal' are identical in PostgreSQL

            // AI DEMO NOTE: Map the C# SqlVector property to SQL Server's native representation. 
            // In SQL Server 2025 / EF Core 10, this translates directly to the optimized VECTOR type.
            builder.Property(b => b.SearchVector)
                .HasColumnType("vector(768)"); // 768 represents the exact dimension output of your chosen AI model

            // DEMO NOTE: Default Values Sandbox
            // Uncomment this during the presentation to show how to force database-level defaults 
            // without assigning them in C# memory.
            //builder.Property(b => b.StockCount)
            //    .IsRequired()                 // Makes it NOT NULL
            //    .HasDefaultValue(10);         // Sets a hardcoded value for existing and new rows
            //    //.HasDefaultValueSql("1 + 1"); // Executes a raw SQL fragment on the DB engine

            #endregion

            #region 3. Audit Fields (Database Defaults & Shadow Properties)

            // 1. Configure CreatedOn to automatically grab the DB server's time on Insert
            builder.Property(e => e.CreatedOn)
                  .HasDefaultValueSql("GETUTCDATE()")
                  .ValueGeneratedOnAdd(); // Tells EF Core to ignore any C# values provided during an Insert

            // 2. Configure UpdatedOn
            // NOTE: SQL Server doesn't natively support triggerless "On Update" default values like MySQL, 
            // so EF Core relies on SaveChanges Interceptors (which we built in ApplicationDbContext) 
            // alongside this configuration to manage updates.
            builder.Property(e => e.UpdatedOn)
                  .HasDefaultValueSql("GETUTCDATE()")
                  .ValueGeneratedOnAddOrUpdate();

            #region PostgreSQL specific changes
            //builder.Property(e => e.CreatedOn)
            //        .HasDefaultValueSql("now() at time zone 'utc'")
            //        .ValueGeneratedOnAdd();

            //builder.Property(e => e.UpdatedOn)
            //      .HasDefaultValueSql("now() at time zone 'utc'")
            //      .ValueGeneratedOnAddOrUpdate(); 
            #endregion

            // ==========================================
            // DEMO NOTE: SHADOW PROPERTIES
            // ==========================================
            // Shadow properties are a powerful enterprise pattern. We are telling EF Core: 
            // "The Book table has a column called 'LastUpdated', even though the C# Book class 
            // doesn't have that property!" This is perfect for hiding DB-only audit columns from business logic.

            //builder.Property<DateTime>("LastUpdated");

            // To access the shadow property, use the code below in the BookDataService:
            // We access the 'hidden' property through the EF Core Entry API
            // _db.Entry(myBook).Property("LastUpdated").CurrentValue = DateTime.UtcNow;

            // This is like giving your LINQ query 'X-ray vision' to see the hidden column during a search:
            //var recentlyUpdatedBooks = await _db.Books
            //    .Where(b => EF.Property<DateTime>(b, "LastUpdated") > DateTime.UtcNow.AddDays(-1))
            //    .ToListAsync();

            #endregion

            #region 4. Indexer Properties
            // ==========================================
            // DEMO NOTE: MAPPING INDEXER PROPERTIES
            // ==========================================
            // These columns WILL be created in the SQL Server table via Migrations, 
            // but they DO NOT exist as hardcoded properties in the C# class!

            builder.IndexerProperty<string?>("CustomCoverColor")
                .HasMaxLength(30)
                .IsRequired(false);

            builder.IndexerProperty<bool?>("IsLimitedEdition")
                .HasDefaultValue(false);

            builder.IndexerProperty<int?>("PromoCodeId")
                .IsRequired(false);

            #endregion

            #region 5. Relationships (Navigation Properties)

            // 1. One-To-Many relationship (Configured from the Many Side)
            builder.HasOne(b => b.Publisher)       // A Book has exactly one Publisher
                .WithMany(p => p.Books)            // A Publisher has many Books
                .HasForeignKey(b => b.PublisherId) // Explicitly map the Foreign Key property
                .OnDelete(DeleteBehavior.Cascade); // If Publisher is deleted, cascade delete all their Books

            // 2. One-To-One relationship
            builder.HasOne(b => b.BookDetails)     // A Book has exactly one BookDetails record
                .WithOne(bd => bd.Book)            // A BookDetails record belongs to exactly one Book
                .HasForeignKey<BookDetails>(bd => bd.BookId) // The Foreign Key lives in the BookDetails table
                .OnDelete(DeleteBehavior.Cascade);

            // 3. Many-To-Many relationship using an explicit Join Entity (Payload Table)
            // DEMO NOTE: Using .UsingEntity() is critical here because it allows our Many-to-Many join table 
            // (BookAuthorMaps) to have extra "Payload" columns (like 'AuthorOrder' or 'ContributionRole') 
            // rather than just being a hidden two-column bridge table.

            // Comment out the below code if you're configuring the many-to-many relationship purely 
            // via Data Annotations (e.g., [ForeignKey]). 
            builder
            .HasMany(b => b.Authors)
            .WithMany(a => a.Books)
            .UsingEntity<BookAuthorMaps>(
                j => j
                    .HasOne(bam => bam.Author)
                    .WithMany(a => a.BookAuthorMaps)
                    .HasForeignKey(bam => bam.AuthorId)
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne(bam => bam.Book)
                    .WithMany(b => b.BookAuthorMaps)
                    .HasForeignKey(bam => bam.BookId)
                    .OnDelete(DeleteBehavior.Cascade),
                j => j.HasKey("BookId", "AuthorId")); // Defines the explicit Composite Primary Key for the Join Table

            #endregion
        }
    }
}