using BookStore.Model.EntityModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStore.DataAccess.Configuration
{
    /// <summary>
    /// Encapsulates the Fluent API configuration for the BookDetails entity.
    /// Demonstrates schema best practices for strict One-to-One relationship mapping in EF Core.
    /// </summary>
    internal class BookDetailsEntityConfiguration : IEntityTypeConfiguration<BookDetails>
    {
        public void Configure(EntityTypeBuilder<BookDetails> builder)
        {
            #region 1. Table & Primary Key Mapping

            builder.ToTable(nameof(BookDetails));

            // DEMO NOTE: Fluent API way to declare the Primary Key. 
            // Talking Point: In a strict One-to-One relationship, it is an enterprise best practice 
            // for the dependent entity (BookDetails) to use its Foreign Key (BookId) as its Primary Key.
            // Why? This physically guarantees at the database schema level that a Book can never 
            // accidentally have more than one BookDetails record associated with it!
            builder.HasKey(nameof(BookDetails.BookId));

            #endregion

            #region 2. Column Configurations & Constraints

            // Basic property configurations ensuring data integrity at the database level
            builder.Property(bd => bd.NumberOfPages)
                .IsRequired();

            builder.Property(bd => bd.NumberOfChapters)
                .IsRequired();

            #endregion

            #region 3. Relationships (Navigation Properties)

            // DEMO NOTE: Less is more! 
            // The One-to-One relationship was already fully defined from the Principal side 
            // inside BookEntityConfiguration. Because EF Core builds a single, unified model graph 
            // during OnModelCreating, we intentionally do not need to redefine the inverse relationship here. 
            // This keeps your Fluent API configuration DRY (Don't Repeat Yourself) and prevents 
            // accidental circular mapping rules.

            #endregion
        }
    }
}