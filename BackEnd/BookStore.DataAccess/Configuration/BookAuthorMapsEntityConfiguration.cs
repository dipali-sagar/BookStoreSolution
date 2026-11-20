using BookStore.Model.EntityModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStore.DataAccess.Configuration
{
    /// <summary>
    /// Encapsulates the Fluent API configuration for the BookAuthorMaps join entity.
    /// Demonstrates how to configure Composite Primary Keys and "Payloads" on Many-to-Many relationships.
    /// </summary>
    internal class BookAuthorMapsEntityConfiguration : IEntityTypeConfiguration<BookAuthorMaps>
    {
        public void Configure(EntityTypeBuilder<BookAuthorMaps> builder)
        {
            #region 1. Table & Primary Key Mapping

            builder.ToTable(nameof(BookAuthorMaps));

            // ==========================================
            // DEMO NOTE: COMPOSITE PRIMARY KEYS
            // ==========================================
            // This is how you configure a Composite Primary Key in EF Core.
            // Join tables naturally use the combination of their Foreign Keys as their unique identifier.

            // Original string-based approach (often used for Shadow Properties):
            builder.HasKey("BookId", "AuthorId");

            // Talking Point (Best Practice): You can also use a strongly-typed anonymous object
            // to avoid "magic strings" and get compile-time safety!
            // builder.HasKey(bam => new { bam.BookId, bam.AuthorId });

            #endregion

            #region 2. Column Configurations (The "Payload")

            // ==========================================
            // DEMO NOTE: EXPLICIT JOIN ENTITIES
            // ==========================================
            // EF Core 5+ supports invisible, automatic Many-to-Many join tables.
            // However, the main reason to explicitly create a join entity class (BookAuthorMaps) 
            // instead of letting EF Core generate an invisible one is so you can add extra 
            // "payload" data to the relationship. 
            //
            // For example, knowing an Author wrote a Book is good, but knowing their Description, 
            // Contribution Role (e.g., "Co-Author", "Foreword"), or AuthorOrder is enterprise-grade!

            builder.Property(bam => bam.Description)
                .HasMaxLength(500)
                .IsRequired(false);

            // If using the enriched model we built earlier, you would map those properties here:
            // builder.Property(bam => bam.ContributionRole).HasMaxLength(50);
            // builder.Property(bam => bam.AuthorOrder).IsRequired();

            #endregion

            #region 3. Relationships

            // DEMO NOTE: Keeping it DRY (Don't Repeat Yourself).
            // The Many-to-Many relationship (and the foreign keys) were already explicitly 
            // configured in the BookEntityConfiguration via the .UsingEntity() method.
            // Because EF Core merges configurations into a single model graph during OnModelCreating, 
            // we do not need to redefine the HasOne/WithMany relationships here, keeping this file nice and clean.

            #endregion
        }
    }
}